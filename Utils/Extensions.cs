using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stock FindTicker(this List<Stock> stocks, string ticker)
        {
            return stocks.FirstOrDefault(stock => stock.ticker == ticker);
        }
    }
}