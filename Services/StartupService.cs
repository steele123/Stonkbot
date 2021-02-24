using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Stonkbot.Services
{
    public class StartupService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        public StartupService(
            IServiceProvider provider,
            DiscordSocketClient client,
            CommandService commands,
            IConfigurationRoot config)
        {
            _provider = provider;
            _config = config;
            _client = client;
            _commands = commands;
        }

        public async Task StartAsync()
        {
            string token = _config["tokens:discord"];
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Couldn't find your token in the 'config.json' file");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }
    }
}