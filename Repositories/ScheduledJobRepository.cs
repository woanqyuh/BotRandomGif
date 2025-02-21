using MongoDB.Driver;
using BotTrungThuong.Dtos;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BotTrungThuong.Repositories
{
    public interface IScheduledJobRepository : IBaseRepository<ThietLapTrungThuongDto>
    {
    }

    public class ScheduledJobRepository : BaseRepository<ThietLapTrungThuongDto>, IScheduledJobRepository
    {
        public ScheduledJobRepository(IMongoDatabase database) : base(database, "thietlaptrungthuong")
        {

        }
    }
}
