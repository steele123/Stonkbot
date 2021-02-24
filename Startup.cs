using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using IEXSharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stonkbot.Services;

namespace Stonkbot
{
    public class Startup
    {
        public IConfigurationRoot config;

        public Startup(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json");
            config = builder.Build();
        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new Startup(args);
            await startup.RunAsync();
        }

        public async Task RunAsync()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<CommandHandler>();

            await provider.GetRequiredService<StartupService>().StartAsync();
            // This will ensure that the Task thread is not exiting unless the program is
            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig()
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 1000
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig()
                {
                    LogLevel = LogSeverity.Verbose,
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton(new IEXCloudClient(config["tokens:iex:token"],
                    config["tokens:iex:secret"],
                    signRequest: false,
                    useSandBox: false))
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartupService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<Random>()
                .AddSingleton(config);
        }
    }
}