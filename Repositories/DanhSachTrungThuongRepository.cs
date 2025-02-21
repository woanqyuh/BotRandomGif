using MongoDB.Driver;
using BotTrungThuong.Dtos;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BotTrungThuong.Repositories
{
    public interface IDanhSachTrungThuongRepository : IBaseRepository<DanhSachTrungThuongDto>
    {

        Task<bool> CheckIfUserWinnedAsync(string chatId,string userId);

    }

    public class DanhSachTrungThuongRepository : BaseRepository<DanhSachTrungThuongDto>, IDanhSachTrungThuongRepository
    {
        public DanhSachTrungThuongRepository(IMongoDatabase database) : base(database, "danhsachtrungthuong")
        {

        }


        public async Task<bool> CheckIfUserWinnedAsync(string chatId , string userId)
        {
            var filter = Builders<DanhSachTrungThuongDto>.Filter.And(
                Builders<DanhSachTrungThuongDto>.Filter.Eq(x => x.IsDeleted, false),
                Builders<DanhSachTrungThuongDto>.Filter.Eq(t => t.ChatId, chatId),
                Builders<DanhSachTrungThuongDto>.Filter.Eq(t => t.UserId, userId)
            );

            return await _collection.Find(filter).AnyAsync();
        }

    }
}
