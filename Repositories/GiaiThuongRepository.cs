using MongoDB.Driver;
using BotTrungThuong.Dtos;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotTrungThuong.Repositories
{
    public interface IGiaiThuongRepository : IBaseRepository<GiaiThuongDto>
    {

        Task UpdateWinnersCount(ObjectId prizeId);
    }

    public class GiaiThuongRepository : BaseRepository<GiaiThuongDto>, IGiaiThuongRepository
    {
        public GiaiThuongRepository(IMongoDatabase database) : base(database, "GiaiThuong")
        {

        }
        public async Task UpdateWinnersCount(ObjectId prizeId)
        {
            var filter = Builders<GiaiThuongDto>.Filter.Eq(p => p.Id, prizeId);
            var update = Builders<GiaiThuongDto>.Update.Inc(p => p.WinnersCount, 1);
            await _collection.UpdateOneAsync(filter, update);
        }

    }
}
