using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Stonkbot.Core.Data;
using Stonkbot.Utils;

namespace Stonkbot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private const string configPath = "Resources/config.json";
        private const string defaultToken = "YOUR_TOKEN HERE";

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();

            await _client.LoginAsync(TokenType.Bot, GetConfig().token);
            await _client.StartAsync();

            // This will make task wait until program is closed
            await Task.Delay(-1);
        }

        // TODO: Make this properly handle a new config.
        private static Config GetConfig()
        {
            if (!File.Exists(configPath))
            {
                Logger.Log($"Couldn't find a config file at '{configPath}'");
                CreateDefaultConfig();
            }

            string json = File.ReadAllText(configPath);
            if (string.IsNullOrWhiteSpace(json))
            {
                Logger.Log($"Couldn't find a valid config in '{configPath}'");
                throw new ConfigException();
            }

            var config = JsonConvert.DeserializeObject<Config>(json);
            if (config.token != defaultToken && !string.IsNullOrWhiteSpace(config.token))
            {
                return config;
            }
            else
            {
                Logger.LogError($"Couldn't find a valid token in '{config.token}'");
                throw new ConfigException();
            }
        }

        private static void CreateDefaultConfig()
        {
            var config = new Config { token = defaultToken };
            File.WriteAllText(configPath, JsonConvert.SerializeObject(config));
        }
    }
}
