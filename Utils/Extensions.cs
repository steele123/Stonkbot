using System.Collections.Generic;
using Stonkbot.Models;

namespace Stonkbot.Extensions
{
    public static class Extensions
    {
        public static void AddStock(this User user, Stock stock)
        {
            if (user.stocks == null)
            {
                user.stocks = new List<Stock>();
            }
            user.stocks.Add(stock);
        }
    }
}