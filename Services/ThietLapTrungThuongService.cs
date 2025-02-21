
using BotTrungThuong.Models;
using BotTrungThuong.Repositories;
using MongoDB.Bson;
using BotTrungThuong.Dtos;
using AutoMapper;
using Quartz;
using BotTrungThuong.Jobs;
using static BotTrungThuong.Dtos.ThietLapTrungThuongDto;


namespace BotTrungThuong.Services
{
    public interface IThietLapTrungThuongService
    {

        Task<ApiResponse<List<ThietLapTrungThuongViewModel>>> GetAll();
        Task<ApiResponse<ThietLapTrungThuongViewModel>> GetByIdAsync(ObjectId id);
        Task<ApiResponse<ThietLapTrungThuongViewModel>> CreateAsync(CreateThietLapTrungThuongRequest model, ObjectId userId);
        Task<ApiResponse<ThietLapTrungThuongViewModel>> UpdateAsync(ObjectId id, CreateThietLapTrungThuongRequest model);
        Task<ApiResponse<string>> DeleteAsync(ObjectId id);

        Task<ApiResponse<string>> StartJobAsync(ObjectId id);

        Task<ApiResponse<string>> StopJobAsync(ObjectId id);
    }

    public class ThietLapTrungThuongService : IThietLapTrungThuongService
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IThietLapTrungThuongRepository _thietLapTrungThuongRepository;
        private readonly IAuthService _authService;
        private readonly JobScheduler _scheduler;



        public ThietLapTrungThuongService(
            IUserRepository userRepository,
            IAuthService authService,
            IMapper mapper,
            IThietLapTrungThuongRepository thietLapTrungThuongRepository,
            JobScheduler scheduler)
        {
            _userRepository = userRepository;
            _authService = authService;
            _mapper = mapper;
            _thietLapTrungThuongRepository = thietLapTrungThuongRepository;
            _scheduler = scheduler;
        }
        public async Task<ApiResponse<List<ThietLapTrungThuongViewModel>>> GetAll()
        {
            try
            {
                var tlListDto = await _thietLapTrungThuongRepository.GetAllAsync();

                return ApiResponse<List<ThietLapTrungThuongViewModel>>.Success(_mapper.Map<List<ThietLapTrungThuongViewModel>>(tlListDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ThietLapTrungThuongViewModel>>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }

        }
        public async Task<ApiResponse<ThietLapTrungThuongViewModel>> GetByIdAsync(ObjectId id)
        {
            try
            {
                var tlDto = await _thietLapTrungThuongRepository.GetByIdAsync(id);
                if (tlDto == null)
                {
                    return ApiResponse<ThietLapTrungThuongViewModel>.Fail("Setting not found", StatusCodeEnum.NotFound);
                }

                return ApiResponse<ThietLapTrungThuongViewModel>.Success(_mapper.Map<ThietLapTrungThuongViewModel>(tlDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<ThietLapTrungThuongViewModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }


        }
        public async Task<ApiResponse<ThietLapTrungThuongViewModel>> CreateAsync(CreateThietLapTrungThuongRequest model, ObjectId userId)
        {
            try
            {
                var tl = new ThietLapTrungThuongDto
                {
                    Id = ObjectId.GenerateNewId(),
                    CreatedBy = userId,
                    Command = model.Command,
                    Name = model.Name,
                    ImageUrl = model.ImageUrl,
                    Status = (int)GiftSettingStatus.Inactive,
                    ChatId = model.ChatId,
                    Content = model.Content,

                    
                    
                    RewardSchedules = model.RewardSchedules.Select(schedule => new RewardSchedule
                    {
                        ResultTime = schedule.ResultTime,
                        Name = schedule.Name,
                        Rewards = _mapper.Map<List<RewardItem>>(schedule.Rewards)
                    }).ToList()
                };
                await _thietLapTrungThuongRepository.AddAsync(tl);
                await _scheduler.ScheduleJobs(tl);

                var tlModel = _mapper.Map<ThietLapTrungThuongViewModel>(tl);
                return ApiResponse<ThietLapTrungThuongViewModel>.Success(tlModel, "Setting created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ThietLapTrungThuongViewModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }



        public async Task<ApiResponse<ThietLapTrungThuongViewModel>> UpdateAsync(ObjectId id, CreateThietLapTrungThuongRequest model)
        {
            try
            {
                var tl = await _thietLapTrungThuongRepository.GetByIdAsync(id);

                if (tl == null)
                {
                    return ApiResponse<ThietLapTrungThuongViewModel>.Fail("Setting not found", StatusCodeEnum.NotFound);
                }


                tl.Command = model.Command;
                tl.Name = model.Name;
                tl.ImageUrl = model.ImageUrl;
                tl.ChatId = model.ChatId;
                tl.Content = model.Content;

                tl.RewardSchedules = model.RewardSchedules.Select(schedule => new RewardSchedule
                {
                    ResultTime = schedule.ResultTime,
                    Name = schedule.Name,
                    Rewards = _mapper.Map<List<RewardItem>>(schedule.Rewards)
                }).ToList();

                await _scheduler.RescheduleJobs(tl);
                await _thietLapTrungThuongRepository.UpdateAsync(id, tl);

                var tlModel = _mapper.Map<ThietLapTrungThuongViewModel>(tl);
                return ApiResponse<ThietLapTrungThuongViewModel>.Success(tlModel, "Setting updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<ThietLapTrungThuongViewModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }



        public async Task<ApiResponse<string>> DeleteAsync(ObjectId id)
        {
            try
            {
                var tl = await _thietLapTrungThuongRepository.GetByIdAsync(id);

                if (tl == null)
                {
                    return ApiResponse<string>.Fail("Setting not found", StatusCodeEnum.NotFound);
                }

               
                await _scheduler.DeleteScheduledJobs(id);
                await _thietLapTrungThuongRepository.DeleteAsync(id);

                return ApiResponse<string>.Success("Setting deleted successfully", StatusCodeEnum.None);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
        public async Task<ApiResponse<string>> StartJobAsync(ObjectId id)
        {
            try
            {
                var tl = await _thietLapTrungThuongRepository.GetByIdAsync(id);

                if (tl == null)
                {
                    return ApiResponse<string>.Fail("Setting not found", StatusCodeEnum.NotFound);
                }
                tl.Status = (int)GiftSettingStatus.InProgress;
               

                await _scheduler.ScheduleJobs(tl);
                await _thietLapTrungThuongRepository.UpdateAsync(id, tl);

                return ApiResponse<string>.Success("Start job successfully", StatusCodeEnum.None);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }

        public async Task<ApiResponse<string>> StopJobAsync(ObjectId id)
        {
            try
            {
                var tl = await _thietLapTrungThuongRepository.GetByIdAsync(id);

                if (tl == null)
                {
                    return ApiResponse<string>.Fail("Setting not found", StatusCodeEnum.NotFound);
                }
                tl.Status = (int)GiftSettingStatus.Inactive;


                await _scheduler.DeleteScheduledJobs(id);
                await _thietLapTrungThuongRepository.UpdateAsync(id, tl);

                return ApiResponse<string>.Success("Stop job successfully", StatusCodeEnum.None);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }


    }

}
