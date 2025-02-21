using MongoDB.Driver;
using BotTrungThuong.Dtos;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace BotTrungThuong.Repositories
{
    public interface IDanhSachTrungThuongOnlineRepository : IBaseRepository<DanhSachTrungThuongOnlineDto>
    {

    }

    public class DanhSachTrungThuongOnlineRepository : BaseRepository<DanhSachTrungThuongOnlineDto>, IDanhSachTrungThuongOnlineRepository
    {
        public DanhSachTrungThuongOnlineRepository(IMongoDatabase database) : base(database, "DanhSachTrungThuongOnline")
        {

        }
    }
}
