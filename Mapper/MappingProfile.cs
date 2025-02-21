using AutoMapper;
using BotTrungThuong.Models;
using BotTrungThuong.Dtos;
using static BotTrungThuong.Dtos.ThietLapTrungThuongDto;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserDto, UserModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))  
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.ToString()));
        CreateMap<ThietLapTrungThuongDto, ThietLapTrungThuongViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy.ToString()))
            .ForMember(dest => dest.RewardSchedules, opt => opt.MapFrom(src => src.RewardSchedules));

        CreateMap<RewardSchedule, RewardScheduleViewModel>();
        CreateMap<RewardScheduleRequest, RewardSchedule>();
        CreateMap<RewardRequest, RewardItem>();
        CreateMap<RewardItem, RewardViewModel>();
        CreateMap<ChildrenGiftRequest, ChildrenGift>();
        CreateMap<ChildrenGift, ChildrenGiftViewModel>();

        CreateMap<ThamGiaTrungThuongDto, ThamGiaTrungThuongViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
             .ForMember(dest => dest.ThietLapId, opt => opt.MapFrom(src => src.ThietLapId.ToString()));
        CreateMap<DanhSachTrungThuongDto, DanhSachTrungThuongViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        CreateMap<DanhSachTrungThuongOnlineDto, DanhSachTrungThuongOnlineViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.GiftId, opt => opt.MapFrom(src => src.GiftId.ToString()));
        CreateMap<BotConfigurationDto, BotConfigurationViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        CreateMap<TeleTextDto, TeleTextModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        CreateMap<GiaiThuongDto, GiaiThuongViewModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ChildrenGifts, opt => opt.MapFrom(src => src.ChildrenGifts));
    }
}