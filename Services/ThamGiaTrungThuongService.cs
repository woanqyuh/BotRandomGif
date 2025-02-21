
using BotTrungThuong.Models;
using BotTrungThuong.Repositories;
using MongoDB.Bson;
using BotTrungThuong.Dtos;
using AutoMapper;
using Quartz;
using Microsoft.Extensions.Caching.Memory;


namespace BotTrungThuong.Services
{
    public interface IThamGiaTrungThuongService
    {

        Task<ApiResponse<List<ThamGiaTrungThuongViewModel>>> GetAll();

        Task<ApiResponse<string>> DeleteAsync(ObjectId id);

        Task<ApiResponse<string>> DeleteBySettingIdAsync(ObjectId settingId);

        Task<ApiResponse<List<ThamGiaTrungThuongViewModel>>> GetNonWinners();
    }

    public class ThamGiaTrungThuongService : IThamGiaTrungThuongService
    {
        private readonly IMapper _mapper;

        private readonly IThamGiaTrungThuongRepository _thamGiaTrungThuongRepository;

        private readonly IMemoryCache _cache;

        private readonly IDanhSachTrungThuongOnlineRepository _danhSachTrungThuongOnlineRepository;

        public ThamGiaTrungThuongService(IMapper mapper, IThamGiaTrungThuongRepository thamGiaTrungThuongRepository, IMemoryCache cache, IDanhSachTrungThuongOnlineRepository danhSachTrungThuongOnlineRepository)
        {

            _mapper = mapper;
            _cache = cache;
            _thamGiaTrungThuongRepository = thamGiaTrungThuongRepository;
            _danhSachTrungThuongOnlineRepository = danhSachTrungThuongOnlineRepository;
        }
        public async Task<ApiResponse<List<ThamGiaTrungThuongViewModel>>> GetAll()
        {
            try
            {
                var participants = await _thamGiaTrungThuongRepository.GetAllAsync();

                return ApiResponse<List<ThamGiaTrungThuongViewModel>>.Success(_mapper.Map<List<ThamGiaTrungThuongViewModel>>(participants));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ThamGiaTrungThuongViewModel>>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }

        }

        public async Task<ApiResponse<List<ThamGiaTrungThuongViewModel>>> GetNonWinners()
        {
            try
            {
                _cache.Remove("ParticipantsList");
                var participants = await _thamGiaTrungThuongRepository.GetAllAsync();
                var winnerList = await _danhSachTrungThuongOnlineRepository.GetAllAsync();
                var winnerUserIds = winnerList.Select(w => w.UserId).ToList();

                var filteredParticipants = participants
                        .Where(p => !winnerUserIds.Contains(p.UserId))
                        .ToList();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60),
                    SlidingExpiration = TimeSpan.FromMinutes(10)
                };

                _cache.Set("ParticipantsList", filteredParticipants, cacheOptions);

                return ApiResponse<List<ThamGiaTrungThuongViewModel>>.Success(_mapper.Map<List<ThamGiaTrungThuongViewModel>>(filteredParticipants));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ThamGiaTrungThuongViewModel>>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }

        }

        public async Task<ApiResponse<string>> DeleteAsync(ObjectId id)
        {
            try
            {
                var tl = await _thamGiaTrungThuongRepository.GetByIdAsync(id);

                if (tl == null)
                {
                    return ApiResponse<string>.Fail("Setting not found", StatusCodeEnum.NotFound);
                }

                await _thamGiaTrungThuongRepository.DeleteAsync(id);
                return ApiResponse<string>.Success("Setting deleted successfully", StatusCodeEnum.None);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }

        public async Task<ApiResponse<string>> DeleteBySettingIdAsync(ObjectId settingId)
        {
            try
            {
                var itemsToDelete = await _thamGiaTrungThuongRepository.GetAllBySettingIdAsync(settingId);

                if (itemsToDelete == null || !itemsToDelete.Any())
                {
                    return ApiResponse<string>.Fail("No items found with the given SettingId", StatusCodeEnum.NotFound);
                }

                var ids = itemsToDelete.Select(x => x.Id);
                await _thamGiaTrungThuongRepository.DeleteManyAsync(ids);
                return ApiResponse<string>.Success("Items deleted successfully", StatusCodeEnum.None);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
    }

}
