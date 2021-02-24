using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Stonkbot.Models;

namespace Stonkbot.Services
{
    public class DatabaseManager
    {
        private readonly IMongoCollection<User> _collection;
        private const decimal _defaultMoney = 100;

        public DatabaseManager(string connectionURL)
        {
#if DEBUG
            IMongoDatabase database = new MongoClient(connectionURL).GetDatabase("development");
#else
            IMongoDatabase database = new MongoClient(connectionURL).GetDatabase("production");
#endif
            _collection = database.GetCollection<User>("users");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<User> FindUser(ulong id)
        {
            return await _collection.Find(user => user.id == id).FirstAsync();
        }

        /// <summary>
        /// This method will find a user and if that user is not found will create a default user.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<User> FindUserAndCreate(ulong id) =>
            await _collection.Find(user => user.id == id)
                .FirstAsync() ??
            new User
            {
                id = id,
                money = _defaultMoney
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task UpdateUser(User user)
        {
            await _collection.ReplaceOneAsync<User>(_user => _user.id == user.id, user);
        }
    }
}