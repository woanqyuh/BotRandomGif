using MongoDB.Driver;
using BotTrungThuong.Dtos;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BotTrungThuong.Repositories
{
    public interface IBotConfigurationRepository : IBaseRepository<BotConfigurationDto>
    {
        Task<BotConfigurationDto> GetByKeyValueAsync(string keyvalue);

        Task<BotConfigurationDto> GetSingleAsync();
    }

    public class BotConfigurationRepository : BaseRepository<BotConfigurationDto>, IBotConfigurationRepository
    {
        public BotConfigurationRepository(IMongoDatabase database) : base(database, "BotConfiguration")
        {
        }
        public async Task<BotConfigurationDto> GetByKeyValueAsync(string keyvalue)
        {
            var filter = Builders<BotConfigurationDto>.Filter.Eq(x => x.KeyValue, keyvalue);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<BotConfigurationDto> GetSingleAsync()
        {
            return await _collection.Find(Builders<BotConfigurationDto>.Filter.Empty).FirstOrDefaultAsync();
        }
    }
}
