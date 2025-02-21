using MongoDB.Driver;
using BotTrungThuong.Dtos;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BotTrungThuong.Repositories
{
    public interface IThamGiaTrungThuongRepository : IBaseRepository<ThamGiaTrungThuongDto>
    {
        Task<List<ThamGiaTrungThuongDto>> GetAllBySettingIdAsync(ObjectId settingId);
        Task<List<ThamGiaTrungThuongDto>> GetAllByChatIdAsync(long chatId);

        Task<bool> CheckIfUserJoinedAsync(ObjectId settingId, string userId, long chatId, int messId);

        Task DeleteManyAsync(IEnumerable<ObjectId> ids);

        Task AddRangeAsync(IEnumerable<ThamGiaTrungThuongDto> entities);
    }

    public class ThamGiaTrungThuongRepository : BaseRepository<ThamGiaTrungThuongDto>, IThamGiaTrungThuongRepository
    {
        public ThamGiaTrungThuongRepository(IMongoDatabase database) : base(database, "thamgiatrungthuong")
        {

        }

        public async Task<List<ThamGiaTrungThuongDto>> GetAllBySettingIdAsync(ObjectId settingId)
        {
            var filter = Builders<ThamGiaTrungThuongDto>.Filter.And(
                    Builders<ThamGiaTrungThuongDto>.Filter.Eq(x => x.IsDeleted, false),
                    Builders<ThamGiaTrungThuongDto>.Filter.Eq(ds => ds.ThietLapId, settingId)
                );
            var result = await _collection.Find(filter).ToListAsync();
            return result;
        }
        public async Task<List<ThamGiaTrungThuongDto>> GetAllByChatIdAsync(long chatId)
        {
            var filter = Builders<ThamGiaTrungThuongDto>.Filter.And(
                    Builders<ThamGiaTrungThuongDto>.Filter.Eq(x => x.IsDeleted, false),
                    Builders<ThamGiaTrungThuongDto>.Filter.Eq(ds => ds.FromChatId, chatId)
                );
            var result = await _collection.Find(filter).ToListAsync();
            return result;
        }


        public async Task<bool> CheckIfUserJoinedAsync(ObjectId settingId, string userId, long chatId,int messId)
        {
            var filter = Builders<ThamGiaTrungThuongDto>.Filter.And(
                Builders<ThamGiaTrungThuongDto>.Filter.Eq(x => x.IsDeleted, false),
                Builders<ThamGiaTrungThuongDto>.Filter.Eq(t => t.ThietLapId, settingId),
                Builders<ThamGiaTrungThuongDto>.Filter.Eq(t => t.UserId, userId),
                Builders<ThamGiaTrungThuongDto>.Filter.Eq(t => t.FromChatId, chatId),
                Builders<ThamGiaTrungThuongDto>.Filter.Eq(t => t.FromMessageId, messId)
            );

            return await _collection.Find(filter).AnyAsync();
        }

        public async Task DeleteManyAsync(IEnumerable<ObjectId> ids)
        {
            var filter = Builders<ThamGiaTrungThuongDto>.Filter.In(x => x.Id, ids);
            var update = Builders<ThamGiaTrungThuongDto>.Update.Set(x => x.IsDeleted, true);
            await _collection.UpdateManyAsync(filter, update);
        }

        public async Task AddRangeAsync(IEnumerable<ThamGiaTrungThuongDto> entities)
        {
            if (entities == null || !entities.Any())
                return;

            await _collection.InsertManyAsync(entities);
        }
    }
}
