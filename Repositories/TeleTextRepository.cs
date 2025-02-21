using MongoDB.Driver;
using BotTrungThuong.Dtos;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BotTrungThuong.Repositories
{
    public interface ITeleTextRepository : IBaseRepository<TeleTextDto>
    {


        Task<TeleTextDto> GetSingleAsync();
    }

    public class TeleTextRepository : BaseRepository<TeleTextDto>, ITeleTextRepository
    {
        public TeleTextRepository(IMongoDatabase database) : base(database, "TeleText")
        {
        }

        public async Task<TeleTextDto> GetSingleAsync()
        {
            return await _collection.Find(Builders<TeleTextDto>.Filter.Empty).FirstOrDefaultAsync();
        }
    }
}
