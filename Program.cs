using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Stonkbot
{
    public class Program
    {
        public static Task Main(string[] args) => Startup.RunAsync(args);
    }
}
