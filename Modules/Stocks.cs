using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.Configuration;

namespace Stonkbot.Modules
{
    public class Stocks : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;

        public Stocks(CommandService service, IConfigurationRoot config)
        {
            _service = service;
            _config = config;
        }

        [Command("Portfolio")]
        public async Task PortfolioCommand()
        {
            
        }
    }
}