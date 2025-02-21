
using BotTrungThuong.Models;
using BotTrungThuong.Repositories;
using MongoDB.Bson;
using BotTrungThuong.Dtos;
using AutoMapper;
using Quartz;
using Telegram.Bot;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Telegram.Bot.Types;
using static MongoDB.Driver.WriteConcern;
using Microsoft.Extensions.Caching.Memory;


namespace BotTrungThuong.Services
{
    public interface IDanhSachTrungThuongOnlineService
    {

        Task<ApiResponse<List<DanhSachTrungThuongOnlineViewModel>>> GetAll(TrungThuongFilter model);
        Task<ApiResponse<string>> DeleteAsync(ObjectId id);
        Task<ApiResponse<string>> SendMessageAsync(string[] ids);

        Task<ApiResponse<DanhSachTrungThuongOnlineViewModel>> Spin(ObjectId giftId);
    }

    public class DanhSachTrungThuongOnlineService : IDanhSachTrungThuongOnlineService
    {
        private readonly IMapper _mapper;
        private readonly IGiaiThuongRepository _giaiThuongRepository;
        private readonly IDanhSachTrungThuongOnlineRepository _danhSachTrungThuongRepository;
        private readonly IThamGiaTrungThuongRepository _thamGiaTrungThuongRepository;
        private readonly IBotConfigurationRepository _botConfigurationRepository;
        private readonly IThietLapTrungThuongRepository _thietLapTrungThuongRepository;

        private readonly IMemoryCache _cache;
        public DanhSachTrungThuongOnlineService(
            IMapper mapper, 
            IDanhSachTrungThuongOnlineRepository danhSachTrungThuongRepository, 
            IGiaiThuongRepository giaiThuongRepository, 
            IBotConfigurationRepository botConfigurationRepository,
            IThamGiaTrungThuongRepository thamGiaTrungThuongRepository,
            IMemoryCache cache,
            IThietLapTrungThuongRepository thietLapTrungThuongRepository)
        {
            _mapper = mapper;
            _danhSachTrungThuongRepository = danhSachTrungThuongRepository;
            _giaiThuongRepository = giaiThuongRepository;
            _botConfigurationRepository = botConfigurationRepository;
            _thietLapTrungThuongRepository = thietLapTrungThuongRepository;
            _thamGiaTrungThuongRepository = thamGiaTrungThuongRepository;
            _cache = cache;
        }
        public async Task<ApiResponse<List<DanhSachTrungThuongOnlineViewModel>>> GetAll(TrungThuongFilter model)
        {
            try
            {
                var tlListDto = await _danhSachTrungThuongRepository.GetAllAsync();

                if (!string.IsNullOrEmpty(model.GiftId))
                {
                    tlListDto = tlListDto.Where(x => x.GiftId == ObjectId.Parse(model.GiftId)).ToList();
                }

                return ApiResponse<List<DanhSachTrungThuongOnlineViewModel>>.Success(_mapper.Map<List<DanhSachTrungThuongOnlineViewModel>>(tlListDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<DanhSachTrungThuongOnlineViewModel>>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
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

        public async  Task<ApiResponse<string>> SendMessageAsync(string[] ids)
        {
            try
            {
                var thietlap = await _thietLapTrungThuongRepository.GetSingleAsync();

                if (thietlap == null)
                { 
                    return ApiResponse<string>.Fail("Setting Not found. Cannot send Message.", StatusCodeEnum.Invalid);
                }

                var botClient = await GetBotClientAsync();
                var messageText = EscapeMarkdown(thietlap.Content) + "\n\n";


                foreach (var id in ids) {
                    var winner = await _danhSachTrungThuongRepository.GetByIdAsync(ObjectId.Parse(id));
                    string winnerMessage = $"[{EscapeMarkdown(winner.Fullname)}](https://t.me/{winner.Username}) nhận giải thưởng {EscapeMarkdown(winner.Gift)}";
                    messageText += string.Join("\n", winnerMessage) + "\n";
                }
                messageText += EscapeMarkdown("\n\n Liên hệ quản lý nhóm để nhận phần thưởng");

                using var client = new HttpClient();

                if (!string.IsNullOrEmpty(thietlap.ImageUrl) && Uri.IsWellFormedUriString(thietlap.ImageUrl, UriKind.Absolute))
                {
                    var responsec = await client.GetAsync(thietlap.ImageUrl);

                    if (responsec.IsSuccessStatusCode)
                    {
                        var contentType = responsec.Content.Headers.ContentType?.MediaType;
                        if (contentType.StartsWith("image/"))
                        {
                            var stream = await responsec.Content.ReadAsStreamAsync();
                            await botClient.SendPhoto(
                                thietlap.ChatId,
                                stream
                                //replyParameters: firstWinner.FromMessageId
                            );
                        }
                        else if (contentType.StartsWith("video/"))
                        {
                            var stream = await responsec.Content.ReadAsStreamAsync();
                            await botClient.SendVideo(
                                thietlap.ChatId,
                                stream
                                //replyParameters: firstWinner.FromMessageId
                            );
                        }
                    }
                }
                await botClient.SendMessage(
                    thietlap.ChatId,
                    messageText, 
                    Telegram.Bot.Types.Enums.ParseMode.MarkdownV2
                    //replyParameters: firstWinner.FromMessageId
                );


                return ApiResponse<string>.Success("Send successfully", StatusCodeEnum.None);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
        public async Task<ApiResponse<DanhSachTrungThuongOnlineViewModel>> Spin(ObjectId giftId)
        {
            try
            {
                var gift = await _giaiThuongRepository.GetByIdAsync(giftId);
                if (gift == null)
                    return ApiResponse<DanhSachTrungThuongOnlineViewModel>.Fail("Xin lỗi! Giải thưởng không tồn tại", StatusCodeEnum.Invalid);
                if (!_cache.TryGetValue("ParticipantsList", out List<ThamGiaTrungThuongDto> participants))
                {
                    return ApiResponse<DanhSachTrungThuongOnlineViewModel>.Fail("Danh sách tham gia không tìm thấy trong cache", StatusCodeEnum.InternalServerError);
                }
                var winners = await _danhSachTrungThuongRepository.GetAllAsync();

                var eligibleParticipants = GetEligibleParticipants(participants, winners);
                if (!eligibleParticipants.Any())
                    return ApiResponse<DanhSachTrungThuongOnlineViewModel>.Fail("Danh sách tham gia không tìm thấy", StatusCodeEnum.InternalServerError);

                if (gift.Quantity == null && gift.ChildrenGifts?.Any() == true)
                {
                    var result = await HandleChildrenGifts(gift, eligibleParticipants);
                    if (result.IsOk == false)
                    {
                        return ApiResponse<DanhSachTrungThuongOnlineViewModel>.Fail(result.Message, StatusCodeEnum.Invalid);
                    }
                    return ApiResponse<DanhSachTrungThuongOnlineViewModel>.Success(_mapper.Map<DanhSachTrungThuongOnlineViewModel>(result.Data),"Quay thưởng thành công");
                }
                else if (gift.Quantity != null && gift.WinnersCount != null)
                {
                   var result =  await HandleGiftWithQuantity(gift, eligibleParticipants);
                    if (result.IsOk == false)
                    {
                        return ApiResponse<DanhSachTrungThuongOnlineViewModel>.Fail(result.Message, StatusCodeEnum.Invalid);
                    }
                    return ApiResponse<DanhSachTrungThuongOnlineViewModel>.Success(_mapper.Map<DanhSachTrungThuongOnlineViewModel>(result.Data), "Quay thưởng thành công");
                }
                else
                {
                    return ApiResponse<DanhSachTrungThuongOnlineViewModel>.Fail("Không có danh sách trúng thưởng", StatusCodeEnum.InternalServerError);
                }

               
            }
            catch (Exception ex)
            {
                return ApiResponse<DanhSachTrungThuongOnlineViewModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }

        private List<ThamGiaTrungThuongDto> GetEligibleParticipants(IEnumerable<ThamGiaTrungThuongDto> participants, IEnumerable<DanhSachTrungThuongOnlineDto> winners)
        {
            var winnerIds = new HashSet<string>(winners.Select(w => w.UserId));
            return participants
                .Where(p => !winnerIds.Contains(p.UserId))
                .ToList();
        }

        private async Task<ApiResponse<DanhSachTrungThuongOnlineDto>>HandleChildrenGifts(GiaiThuongDto gift, List<ThamGiaTrungThuongDto> eligibleParticipants)
        {
            var random = new Random();

            var childrenGifts = gift.ChildrenGifts.OrderByDescending(x => x.Quantity).ToList();
            var giftName = "";
            bool hasWinner = false;
            var winner = new ThamGiaTrungThuongDto();
            foreach (var childGift in childrenGifts)
            {
                if (childGift.WinnersCount >= childGift.Quantity) continue;


                winner = SelectRandomWinner(eligibleParticipants, random);
                childGift.WinnersCount++;
                hasWinner = true;
                giftName = childGift.Name;
                break;
            }

            if (!hasWinner)
                return ApiResponse<DanhSachTrungThuongOnlineDto>.Fail("Số lượng phần quà đã hết!", StatusCodeEnum.Invalid);

            gift.ChildrenGifts = childrenGifts;
            var newWinner = await CreateWinnerRecord(winner, gift, giftName);
            if (newWinner.IsOk == false)
            {
                return newWinner;
            }
            await _giaiThuongRepository.UpdateAsync(gift.Id, gift);
            return ApiResponse<DanhSachTrungThuongOnlineDto>.Success(newWinner.Data, "Quay thưởng thành công");
        }

        private async Task<ApiResponse<DanhSachTrungThuongOnlineDto>> HandleGiftWithQuantity(GiaiThuongDto gift, List<ThamGiaTrungThuongDto> eligibleParticipants)
        {
            var random = new Random();

            if (gift.WinnersCount >= gift.Quantity)
            {
                return ApiResponse<DanhSachTrungThuongOnlineDto>.Fail("Số lượng phần quà đã hết!", StatusCodeEnum.Invalid);

            };

            var winner = SelectRandomWinner(eligibleParticipants, random);
            var newWinner = await CreateWinnerRecord(winner, gift, null);
            if(newWinner.IsOk == false)
            {
                return newWinner;
            }
            gift.WinnersCount++;
            await _giaiThuongRepository.UpdateAsync(gift.Id, gift);
            return ApiResponse<DanhSachTrungThuongOnlineDto>.Success(newWinner.Data, "Quay thưởng thành công");
        }



        private ThamGiaTrungThuongDto SelectRandomWinner(List<ThamGiaTrungThuongDto> participants, Random random)
        {
            return participants[random.Next(participants.Count)];
        }

        private async Task<ApiResponse<DanhSachTrungThuongOnlineDto>> CreateWinnerRecord(ThamGiaTrungThuongDto winner, GiaiThuongDto gift, string? giftName)
        {
            var setup = await _thietLapTrungThuongRepository.GetByIdAsync(winner.ThietLapId);

            if(setup == null)
            {
                return ApiResponse<DanhSachTrungThuongOnlineDto>.Fail("Setting not found",StatusCodeEnum.NotFound);

            }

            var newWinner = new DanhSachTrungThuongOnlineDto
            {
                Id = ObjectId.GenerateNewId(),
                Fullname = winner.Fullname,
                ThietLapId = setup.Id,
                Username = winner.Username,
                UserId = winner.UserId,
                FromChatId = winner.FromChatId,
                FromMessageId = winner.FromMessageId,
                Gift = giftName != null ? giftName : gift.Name,
                GiftId = gift.Id
            };

            await _danhSachTrungThuongRepository.AddAsync(newWinner);
            return ApiResponse<DanhSachTrungThuongOnlineDto>.Success(newWinner, "Tạo thành công");
        }

        private async Task<TelegramBotClient> GetBotClientAsync()
        {
            var botConfig = await _botConfigurationRepository.GetSingleAsync();
            if (botConfig == null || string.IsNullOrEmpty(botConfig.KeyValue))
            {
                throw new Exception("API Key is not configured in the database.");
            }
            return new TelegramBotClient(botConfig.KeyValue);
        }
        private string EscapeMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var specialChars = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
            foreach (var ch in specialChars)
            {
                text = text.Replace(ch, "\\" + ch);
            }
            return text;
        }
    }
}

