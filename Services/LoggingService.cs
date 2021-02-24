using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Stonkbot.Services
{
    public class LoggingService
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        private string _logPath { get; }
        private string _logFile => Path.Combine(_logPath, $"{DateTime.UtcNow:yyyy-MM-dd}.txt");

        public LoggingService(DiscordSocketClient client, CommandService commands)
        {
            _logPath = Path.Combine(AppContext.BaseDirectory, "logs");
            _client = client;
            _commands = commands;

            client.Log += OnLogAsync;
            commands.Log += OnLogAsync;
        }

        private Task OnLogAsync(LogMessage message)
        {
            if (!Directory.Exists(_logPath))
                Directory.CreateDirectory(_logPath);
            if (!File.Exists(_logFile))
                File.Create(_logFile).Dispose();

            string log = $"{DateTime.UtcNow:hh:mm:ss} [{message.Severity}] {message.Source}: {message.Exception?.ToString() ?? message.Message}";
            
            // TODO: Implement file logger with mutex or something
            //File.AppendAllText(_logFile, log + "\n");

            return Console.Out.WriteLineAsync(log);
        }
    }
}