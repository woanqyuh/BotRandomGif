
using BotTrungThuong.Models;
using BotTrungThuong.Repositories;
using MongoDB.Bson;
using BotTrungThuong.Dtos;
using AutoMapper;


namespace BotTrungThuong.Services
{
    public interface IDanhSachTrungThuongService
    {

        Task<ApiResponse<List<DanhSachTrungThuongViewModel>>> GetAll();

        Task<ApiResponse<string>> DeleteAsync(ObjectId id);
    }

    public class DanhSachTrungThuongService : IDanhSachTrungThuongService
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IThietLapTrungThuongRepository _thietLapTrungThuongRepository;
        private readonly IAuthService _authService;
        private readonly IDanhSachTrungThuongRepository _danhSachTrungThuongRepository;

        public DanhSachTrungThuongService(IUserRepository userRepository, IAuthService authService, IMapper mapper, IThietLapTrungThuongRepository thietLapTrungThuongRepository, IDanhSachTrungThuongRepository danhSachTrungThuongRepository)
        {
            _userRepository = userRepository;
            _authService = authService;
            _mapper = mapper;
            _thietLapTrungThuongRepository = thietLapTrungThuongRepository;
            _danhSachTrungThuongRepository = danhSachTrungThuongRepository;
        }
        public async Task<ApiResponse<List<DanhSachTrungThuongViewModel>>> GetAll()
        {
            try
            {
                var tlListDto = await _danhSachTrungThuongRepository.GetAllAsync();

                return ApiResponse<List<DanhSachTrungThuongViewModel>>.Success(_mapper.Map<List<DanhSachTrungThuongViewModel>>(tlListDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DanhSachTrungThuongViewModel>>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }

        }

        public async Task<ApiResponse<string>> DeleteAsync(ObjectId id)
        {
            try
            {
                var tl = await _danhSachTrungThuongRepository.GetByIdAsync(id);

                if (tl == null)
                {
                    return ApiResponse<string>.Fail("Setting not found", StatusCodeEnum.NotFound);
                }

                await _danhSachTrungThuongRepository.DeleteAsync(id);
                return ApiResponse<string>.Success("Setting deleted successfully", StatusCodeEnum.None);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
    }
}

