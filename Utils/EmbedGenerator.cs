using System;
using Discord;

namespace Stonkbot.Extensions
{
    public static class EmbedUtils
    {
        public static Embed ErrorEmbed(string errorMessage)
        {
            var embedBuilder = new EmbedBuilder
            {
                Title = "Error", 
                Description = errorMessage, 
                Color = Color.Red, 
                Timestamp = DateTimeOffset.Now
            };
            return embedBuilder.Build();
        }

        public static EmbedBuilder ToError(this EmbedBuilder builder, string errorMessage)
        {
            return new()
            {
                Title = "Error",
                Description = errorMessage,
                Color = Color.Red,
                Timestamp = DateTimeOffset.Now
            };
        }

        public static Embed ToErrorEmbed(this EmbedBuilder builder, string errorMessage)
        {
            builder = new EmbedBuilder()
            {
                Title = "Error",
                Description = errorMessage,
                Color = Color.Red,
                Timestamp = DateTimeOffset.Now
            };
            return builder.Build();
        }
    }
}