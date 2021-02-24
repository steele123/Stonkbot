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

        [Command("portfolio")]
        public async Task PortfolioCommand()
        {
            EmbedBuilder builder = new EmbedBuilder {Title = "Your Portfolio"};

            var user = await _manager.FindUser(Context.User.Id);
            if (user == null)
            {
                await ReplyAsync(embed: builder.ToErrorEmbed("Couldn't find a portfolio for your account"));
            }


        }

        [Command("purchase")]
        [Summary("Purchases a certain quantity of shares of a security/stock.")]
        public async Task PurchaseCommand(string ticker, int quantity)
        {
            var response = await _cloudClient.StockPrices.PriceAsync(ticker);

            if (response == null)
            {
                await ReplyAsync(embed: EmbedUtils.ErrorEmbed($"Could not find the stock ticker '{ticker}'!"));
                return;
            }

            var user = await _manager.FindUserAndCreate(Context.User.Id);

            if (user.money < quantity * response.Data)
            {
                await ReplyAsync(embed: EmbedUtils.ErrorEmbed(
                    $"You don't have enough money to purchase {quantity} shares of {ticker.ToUpper()}."));
            }
            else
            {
                await ReplyAsync(
                    $"Your {quantity} shares of {ticker} have been purchased for {quantity * response.Data}!");
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
                builder.Title = "";
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

            var stock = new Stock {ticker = ticker};

            if (!user.stocks.Contains(stock))
            {
                builder = builder.ToError($"You do not have enough shares of '{ticker}'");
            }
            else
            {
                builder.Title = $"Sell Order For {ticker} Completed!";
                builder.Description = $"You have sold {quantity} shares for {quantity * response.Data}";
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
    }
}