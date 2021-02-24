using System.Globalization;
using System.Threading.Tasks;
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
    [Name(("Stocks"))]
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
            MongoClient client = new MongoClient(_config["mongodb"]);
            var database = client.GetDatabase("production");
            var collection = database.GetCollection<User>("users");
            await collection.InsertOneAsync(new User { id = Context.User.Id });
        }

        [Command("purchase")]
        [Summary("Purchases a certain quantity of shares of a security/stock.")]
        public async Task PurchaseCommand(string ticker, int quantity)
        {
            var response = await _cloudClient.StockPrices.PriceAsync(ticker);

            if (response == null)
            {
                await ReplyAsync($"Could not find the stock ticker '{ticker}'!");
                return;
            }

            var user = await _manager.FindUserAndCreate(Context.User.Id);

            if (user.money < quantity * response.Data)
            {
                await ReplyAsync($"You don't have enough money to purchase {quantity} shares of {ticker.ToUpper()}.");
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

            if (response != null)
            {
                await ReplyAsync(response.Data.ToString(CultureInfo.InvariantCulture));
            }
        }

        [Command("sell")]
        [Summary("Submits a sell order on a certain security")]
        public async Task SellCommand()
        {

        }

        [Command("quote")]
        [Summary("Gets the stock quote of a stock with a stock ticker.")]
        public async Task QuoteCommand([Remainder] string ticker)
        {

        }
    }
}