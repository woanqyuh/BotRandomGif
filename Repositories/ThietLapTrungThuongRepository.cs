using MongoDB.Driver;
using BotTrungThuong.Dtos;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotTrungThuong.Repositories
{
    public interface IThietLapTrungThuongRepository : IBaseRepository<ThietLapTrungThuongDto>
    {
        Task<List<ThietLapTrungThuongDto>> GetActiveRecordsAsync();

        Task<ThietLapTrungThuongDto> GetSingleAsync();
    }

    public class ThietLapTrungThuongRepository : BaseRepository<ThietLapTrungThuongDto>, IThietLapTrungThuongRepository
    {
        public ThietLapTrungThuongRepository(IMongoDatabase database) : base(database, "thietlaptrungthuong")
        {

        }
        public async Task<List<ThietLapTrungThuongDto>> GetActiveRecordsAsync()
        {
            var filter = Builders<ThietLapTrungThuongDto>.Filter.And(
                Builders<ThietLapTrungThuongDto>.Filter.Eq(x => x.IsDeleted, false),
                Builders<ThietLapTrungThuongDto>.Filter.Eq(x => x.Status, (int)GiftSettingStatus.InProgress)
            );
            return await _collection.Find(filter).ToListAsync();
        }
        public async Task<ThietLapTrungThuongDto> GetSingleAsync()
        {
            var filter = Builders<ThietLapTrungThuongDto>.Filter.Eq(x => x.IsDeleted, false);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

    }
}
