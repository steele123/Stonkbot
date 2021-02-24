using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using IEXSharp;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Stonkbot.Extensions;
using Stonkbot.Models;
using Stonkbot.Services;

namespace Stonkbot.Modules
{
    [Name(("Stock Commands"))]
    public class StockModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;
        private readonly IEXCloudClient _cloudClient;
        private readonly DatabaseManager _manager;

        public StockModule(CommandService service,
            IConfigurationRoot config,
            IEXCloudClient cloudClient,
            DatabaseManager manager)
        {
            _manager = manager;
            _service = service;
            _config = config;
            _cloudClient = cloudClient;
        }

        [Command("balance")]
        public async Task BalanceCommand()
        {

        }

        [Command("portfolio")]
        public async Task PortfolioCommand()
        {
            var user = await _manager.FindUserAndCreate(Context.User.Id);

            var builder = new EmbedBuilder();

            builder.Title = "Your Portfolio";
            builder.AddField("Account Balance", user.money, true);
            builder.AddField("Highest Balance", user.highest, true);
            builder.Color = Color.DarkGreen;
            builder.Timestamp = DateTimeOffset.Now;

            if (user.stocks != null)
            {
                foreach (var stock in user.stocks)
                {
                    builder.AddField(stock.ticker, stock.quantity * stock.price, true);
                }
            }

            await ReplyAsync(embed: builder.Build());
        }

        [Command("purchase")]
        [Alias("buy")]
        [Summary("Purchases a certain quantity of shares of a security/stock.")]
        public async Task PurchaseCommand(string ticker, int quantity)
        {
            ticker = ticker.ToUpper();

            var response = await _cloudClient.StockPrices.PriceAsync(ticker);

            var builder = new EmbedBuilder();

            if (response == null)
            {
                builder.ToStockTickerError(ticker);
                await ReplyAsync(embed: builder.Build());
                return;
            }

            var user = await _manager.FindUserAndCreate(Context.User.Id);

            if (user.money < quantity * response.Data)
            {
                builder.ToError($"You don't have enough money to purchase {quantity} shares of {ticker.ToUpper()}.");
            }
            else
            {
                builder.Description =
                    $"Your {quantity} shares of {ticker} have been purchased for {quantity * response.Data}!";
                builder.Color = Color.DarkGreen;
                builder.Title = $"Buy Order For {ticker} Completed!";
                user.AddStock(new Stock{ticker = ticker, price = (float)response.Data, quantity = quantity});
                await _manager.UpdateUser(user);
            }
        }

        [Command("price")]
        [Summary("Gets the price of a specified security/stock with a stock ticker.")]
        public async Task PriceCommand([Remainder] string ticker)
        {
            var response = await _cloudClient.StockPrices.PriceAsync(ticker);

            var builder = new EmbedBuilder();

            if (response.ErrorMessage == null)
            {
                builder.Title = "Success!";
                builder.Color = Color.DarkGreen;
                builder.Description = $"The current market price of {ticker.ToUpper()} is {response.Data}";
                builder.Timestamp = DateTimeOffset.Now;
            }
            else
            {
                builder = builder.ToStockTickerError(ticker);
            }

            await ReplyAsync(embed: builder.Build());
        }

        [Command("sell")]
        [Summary("Submits a sell order on a certain security")]
        public async Task SellCommand(string ticker, int quantity)
        {
            ticker = ticker.ToUpper();

            var response = await _cloudClient.StockPrices.PriceAsync(ticker);

            var builder = new EmbedBuilder();

            if (response.ErrorMessage != null)
            {
                builder = builder.ToStockTickerError(ticker);
                await ReplyAsync(embed: builder.Build());
                return;
            }

            var user = await _manager.FindUserAndCreate(Context.User.Id);

            if (user.stocks == null)
            {
                builder = builder.ToError("You do not have any stocks to sell");
                await ReplyAsync(embed: builder.Build());
                return;
            }

            var stock = user.stocks.FindTicker(ticker);

            if (stock == null)
            {
                builder = builder.ToError($"You do not have enough shares of '{ticker}'");
            }
            else
            {
                var stockPrice = quantity * response.Data;
                builder.Color = Color.DarkGreen;
                builder.Title = $"Sell Order For {ticker} Completed!";
                builder.Description = $"You have sold {quantity} shares for {stockPrice}";
                user.money += stockPrice;
                user.stocks.Remove(stock);
                await _manager.UpdateUser(user);
            }

            await ReplyAsync(embed: builder.Build());
        }

        [Command("quote")]
        [Summary("Gets the stock quote of a stock with a stock ticker.")]
        public async Task QuoteCommand([Remainder] string ticker)
        {
            var response = await _cloudClient.StockPrices.QuoteAsync(ticker);

            var builder = new EmbedBuilder();

            if (response.ErrorMessage == null)
            {
                builder.Title = $"Stock Quotes for {ticker}";
                builder.Color = Color.DarkGreen;
                builder.AddField("Extended Price", response.Data.extendedPrice, true);
                builder.AddField("Change", response.Data.change, true);
                builder.AddField("Change %", response.Data.changePercent, true);
                builder.AddField("Volume", response.Data.avgTotalVolume, true);
                builder.Timestamp = DateTimeOffset.Now;
            }
            else
            {
                builder = builder.ToStockTickerError(ticker);
            }

            await ReplyAsync(embed: builder.Build());
        }

        [Command("shares")]
        [Summary("Gets the shares you currently own in a stock.")]
        public async Task SharesCommand([Remainder] string ticker)
        {
            ticker = ticker.ToUpper();

            var user = await _manager.FindUserAndCreate(Context.User.Id);

            var builder = new EmbedBuilder();
            
            if (user.stocks != null)
            {
                var stock = user.stocks.FindTicker(ticker);
                if (stock == null)
                {
                    builder = builder.ToError($"You don't have any shares for '{ticker}'.");
                }
                else
                {
                    builder.Title = $"Current Shares For {ticker}";
                    builder.Description = $"You currently have {stock.quantity} shares of {stock.ticker}!";
                    builder.Color = Color.DarkGreen;
                }
            }
            else
            {
                builder = builder.ToError($"You don't have any shares for '{ticker}'.");
            }

            await ReplyAsync(embed: builder.Build());
        }
    }
}