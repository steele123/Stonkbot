using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Stonkbot.Models
{
    /// <summary>
    /// A database user for using stocks.
    /// </summary>
    public class User
    {
        /// <summary>
        /// This is a discord ID
        /// </summary>
        [BsonId]
        public ulong id;
        public decimal money;
        public decimal highest;
        public List<Stock> stocks;
    }

    public class Stock
    {
        public int quantity;
        public string ticker;
        public float price;
    }
}