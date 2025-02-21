
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
    public interface IGiaiThuongService
    {

        Task<ApiResponse<List<GiaiThuongViewModel>>> GetAll();
        Task<ApiResponse<GiaiThuongViewModel>> GetByIdAsync(ObjectId id);
        Task<ApiResponse<GiaiThuongViewModel>> CreateAsync(GiaiThuongRequest model);
        Task<ApiResponse<GiaiThuongViewModel>> UpdateAsync(ObjectId id, GiaiThuongRequest model);
        Task<ApiResponse<string>> DeleteAsync(ObjectId id);

    }

    public class GiaiThuongService : IGiaiThuongService
    {
        private readonly IMapper _mapper;
        private readonly IGiaiThuongRepository _giaiThuongRepository;




        public GiaiThuongService(
            IGiaiThuongRepository giaiThuongRepository,
            IMapper mapper)
        {
            _mapper = mapper;
            _giaiThuongRepository = giaiThuongRepository;
        }
        public async Task<ApiResponse<List<GiaiThuongViewModel>>> GetAll()
        {
            try
            {
                var tlListDto = await _giaiThuongRepository.GetAllAsync();

                return ApiResponse<List<GiaiThuongViewModel>>.Success(_mapper.Map<List<GiaiThuongViewModel>>(tlListDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<GiaiThuongViewModel>>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }

        }
        public async Task<ApiResponse<GiaiThuongViewModel>> GetByIdAsync(ObjectId id)
        {
            try
            {
                var tlDto = await _giaiThuongRepository.GetByIdAsync(id);
                if (tlDto == null)
                {
                    return ApiResponse<GiaiThuongViewModel>.Fail("Setting not found", StatusCodeEnum.NotFound);
                }

                return ApiResponse<GiaiThuongViewModel>.Success(_mapper.Map<GiaiThuongViewModel>(tlDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<GiaiThuongViewModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }


        }
        public async Task<ApiResponse<GiaiThuongViewModel>> CreateAsync(GiaiThuongRequest model)
        {
            try
            {
                if ((model.ChildrenGifts != null && model.ChildrenGifts.Count > 0 && model.Quantity != null) ||
                    (model.ChildrenGifts == null || model.ChildrenGifts.Count == 0) && model.Quantity == null)
                {
                    return ApiResponse<GiaiThuongViewModel>.Fail(
                        "Either 'ChildrenGifts' must be provided and 'Quantity' should be null, or 'Quantity' must be provided and 'ChildrenGifts' should be empty.",
                        StatusCodeEnum.Invalid);
                }
                var tl = new GiaiThuongDto
                {
                    Id = ObjectId.GenerateNewId(),
                    Name = model.Name,
                    Quantity = model.Quantity,
                    IsShow = model.IsShow,
                    ImageUrl = model.ImageUrl,
                    WinnersCount = model.Quantity == null ? null : 0,
                    Time = model.Time,
                    ChildrenGifts = model.ChildrenGifts != null ? model.ChildrenGifts.Select(z => new ChildrenGift
                    {

                        Name = z.Name,
                        Quantity = z.Quantity,
                        WinnersCount = 0

                    }).ToList() : []
                };
                await _giaiThuongRepository.AddAsync(tl);

                var tlModel = _mapper.Map<GiaiThuongViewModel>(tl);
                return ApiResponse<GiaiThuongViewModel>.Success(tlModel, "Gift created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<GiaiThuongViewModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }



        public async Task<ApiResponse<GiaiThuongViewModel>> UpdateAsync(ObjectId id, GiaiThuongRequest model)
        {
            try
            {
                if (((model.ChildrenGifts == null || model.ChildrenGifts.Count == 0) && model.Quantity == null) || ((model.ChildrenGifts != null || model.ChildrenGifts.Count != 0) && model.Quantity != null))
                {
                    return ApiResponse<GiaiThuongViewModel>.Fail($"Either 'ChildrenGifts' must be provided and 'Quantity' should be null, or 'Quantity' must be provided and 'ChildrenGifts' should be empty.", StatusCodeEnum.Invalid);

                }
                var tl = await _giaiThuongRepository.GetByIdAsync(id);

                if (tl == null)
                {
                    return ApiResponse<GiaiThuongViewModel>.Fail("Gift not found", StatusCodeEnum.NotFound);
                }

                tl.IsShow = model.IsShow;
                tl.Quantity = model.Quantity;
                tl.Name = model.Name;
                tl.Time = model.Time;
                tl.ImageUrl = model.ImageUrl;
                if (model.ChildrenGifts != null && model.ChildrenGifts.Count > 0)
                {
                    foreach (var childGift in model.ChildrenGifts)
                    {
                        var childName = childGift.Name.Trim();
                        var childQuantity = childGift.Quantity;
                        var existingChildGift = tl.ChildrenGifts?.FirstOrDefault(cg => cg.Name == childName && cg.Quantity == childQuantity);
                        if (existingChildGift != null)
                        {
                            existingChildGift.Quantity = childGift.Quantity;
                            existingChildGift.Name = childGift.Name;
                        }
                        else
                        {

                            tl.ChildrenGifts.Add(new ChildrenGift
                            {
                                Name = childGift.Name,
                                Quantity = childGift.Quantity,
                                WinnersCount = 0 
                            });
                        }
                    }
                }
                else
                {
                    tl.ChildrenGifts = null;
                }

                await _giaiThuongRepository.UpdateAsync(id, tl);

                var tlModel = _mapper.Map<GiaiThuongViewModel>(tl);
                return ApiResponse<GiaiThuongViewModel>.Success(tlModel, "Gift updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<GiaiThuongViewModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }



        public async Task<ApiResponse<string>> DeleteAsync(ObjectId id)
        {
            try
            {
                var tl = await _giaiThuongRepository.GetByIdAsync(id);

                if (tl == null)
                {
                    return ApiResponse<string>.Fail("Gift not found", StatusCodeEnum.NotFound);
                }

                await _giaiThuongRepository.DeleteAsync(id);

                return ApiResponse<string>.Success("Gift deleted successfully", StatusCodeEnum.None);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }


    }

}
