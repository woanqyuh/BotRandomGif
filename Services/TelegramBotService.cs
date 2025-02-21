using BotTrungThuong.Dtos;
using BotTrungThuong.Models;
using BotTrungThuong.Repositories;
using MongoDB.Bson;
using Telegram.Bot;
using Newtonsoft.Json;
using MongoDB.Driver.Linq;
using AutoMapper;
using Telegram.Bot.Types.Enums;


namespace BotTrungThuong.Services
{
    public interface ITelegramBotService
    {
        Task<ApiResponse<string>> HandleWebHook(dynamic data);

        Task<ApiResponse<BotConfigurationViewModel>> GetApiKey();

        Task<ApiResponse<BotConfigurationViewModel>> CreateOrUpdateApiKey(string keyValue);

        Task<ApiResponse<TeleTextModel>> CreateOrUpdateTeleText(TeleTextRequest model);

        Task<ApiResponse<TeleTextModel>> GetTeleText();
    }
    public class TelegramBotService : ITelegramBotService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IThietLapTrungThuongRepository _thietLapTrungThuongRepository;
        private readonly IThamGiaTrungThuongRepository _thamGiaTrungThuongRepository;
        private readonly IDanhSachTrungThuongRepository _danhSachTrungThuongRepository;
        private readonly IBotConfigurationRepository _botConfigurationRepository;
        private readonly ITeleTextRepository _teleTextRepository;
        private readonly IMapper _mapper;

        public TelegramBotService(
            IUserRepository userRepository,
            IThietLapTrungThuongRepository thietLapTrungThuongRepository,
            IThamGiaTrungThuongRepository thamGiaTrungThuongRepository,
            IDanhSachTrungThuongRepository danhSachTrungThuongRepository,
            IBotConfigurationRepository botConfigurationRepository,
            IMapper mapper,
            IConfiguration configuration,
            ITeleTextRepository teleTextRepository
        )

        {
            _userRepository = userRepository;
            _thietLapTrungThuongRepository = thietLapTrungThuongRepository;
            _thamGiaTrungThuongRepository = thamGiaTrungThuongRepository;
            _danhSachTrungThuongRepository = danhSachTrungThuongRepository;
            _botConfigurationRepository = botConfigurationRepository;
            _mapper = mapper;
            _configuration = configuration;
            _teleTextRepository = teleTextRepository;
        }

        public async Task<ApiResponse<string>> HandleWebHook(dynamic data)
        {
            try
            {
                var botClient = await GetBotClientAsync();

                var thietlapList = await _thietLapTrungThuongRepository.GetAllAsync();
                var webhookData = JsonConvert.DeserializeObject<WebhookDataDto>(data.ToString());
                var message = webhookData.Message;
                long chatId = message.Chat.Id;
                int messageId = message.MessageId;
                int messParentId = message.MessageThreadId;

                if (message.Text != null && message.Text.ToLower() == "/getchatid")
                {
                   await botClient.SendMessage(chatId, $"Xin chào! ChatId của bạn là: {chatId}");
                    return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);
                }

                if (message.ReplyToMessage != null)
                {

                    var matchingThietLap = thietlapList
                        .Where(t => t.Command.Any(command => message.Text.Equals(command, StringComparison.OrdinalIgnoreCase)))
                        .Where(t => t.Status == (int)GiftSettingStatus.InProgress)
                        .Where(t => t.ChatId == chatId.ToString())
                        .FirstOrDefault();


                    if (matchingThietLap != null)
                    {
                        if(message.From.Username == null)
                        {
                            await botClient.SendMessage(chatId, $"❌ Tài khoản của bạn chưa cập nhật username! Vui lòng cập nhật để tham gia!");
                            return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);
                        }


                        var alreadyJoined = await _thamGiaTrungThuongRepository.CheckIfUserJoinedAsync(matchingThietLap.Id, message.From.Id.ToString(), chatId, messParentId);
                        var alreadyWinned = await _danhSachTrungThuongRepository.CheckIfUserWinnedAsync(matchingThietLap.ChatId, message.From.Id.ToString());
                        //var alreadyWinned = await _danhSachTrungThuongRepository.CheckIfUserWinnedAsync(matchingThietLap.ChatId, message.From.Id.ToString());
                        if (alreadyJoined)
                        {
                            var messJoined = $"BẠN ĐÃ THAM GIA SỰ KIỆN";
                            var teleText = await _teleTextRepository.GetSingleAsync();
                            if (teleText != null && !string.IsNullOrEmpty(teleText.JoinedReply))
                            {
                                messJoined = teleText.JoinedReply;
                            }

                            await botClient.SendMessage(chatId, messJoined, replyParameters: messageId);
                            return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);
                        }
                        if(!alreadyJoined && !alreadyWinned)
                        {
                            var thamgia = new ThamGiaTrungThuongDto
                            {
                                Id = ObjectId.GenerateNewId(),
                                Fullname = (message.From.FirstName ?? "") + " " + (message.From.LastName ?? ""),
                                ThietLapId = matchingThietLap.Id,
                                Username = message.From.Username,
                                UserId = message.From.Id.ToString(), 
                                FromChatId = chatId,
                                FromMessageId = messParentId,
                                ThietLapName = matchingThietLap.Name,
                            };

                           
                            var messJoinSuccess = $"✅ Bạn đã tham gia dự thưởng thành công";
                            var teleText = await _teleTextRepository.GetSingleAsync();
                            if (teleText != null && !string.IsNullOrEmpty(teleText.JoinSuccessReply))
                            {
                                messJoinSuccess = teleText.JoinSuccessReply.Replace("{0}", thamgia.Fullname);
                            }

                            await _thamGiaTrungThuongRepository.AddAsync(thamgia);
                            await botClient.SendMessage( chatId, messJoinSuccess, replyParameters: messageId);
                            return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);
                        }

                    }

                }

                return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);
            }
        }

        public async Task<ApiResponse<BotConfigurationViewModel>> GetApiKey()
        {
            try
            {
                var userListDto = await _botConfigurationRepository.GetSingleAsync();

                return ApiResponse<BotConfigurationViewModel>.Success(_mapper.Map<BotConfigurationViewModel>(userListDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<BotConfigurationViewModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }

        }
        public async Task<ApiResponse<BotConfigurationViewModel>> CreateOrUpdateApiKey(string keyValue)
        {
            try
            {
                var existingConfig = await _botConfigurationRepository.GetSingleAsync();

                if (existingConfig != null)
                {
                    try
                    {
                        await DeleteWebhook(existingConfig.KeyValue,keyValue);
                    }
                    catch (Exception ex)
                    {

                        return ApiResponse<BotConfigurationViewModel>.Fail("Failed to delete webhook: " + ex.Message, StatusCodeEnum.Invalid);
                    }

                    existingConfig.KeyValue = keyValue;
                   
                    try{
                        await UpdateWebhookWithNewApiKey(keyValue);
                    }
                    catch (Exception ex){
                        return ApiResponse<BotConfigurationViewModel>.Fail("Failed to Update webhook: " + ex.Message, StatusCodeEnum.Invalid);
                    }
                    await _botConfigurationRepository.UpdateAsync(existingConfig.Id, existingConfig);
                    return ApiResponse<BotConfigurationViewModel>.Success(_mapper.Map<BotConfigurationViewModel>(existingConfig));
                }
                var newConfig = new BotConfigurationDto
                {
                    Id = ObjectId.GenerateNewId(),
                    KeyValue = keyValue,

                };
                try
                {
                    await UpdateWebhookWithNewApiKey(keyValue);
                }
                catch (Exception ex)
                {
                    return ApiResponse<BotConfigurationViewModel>.Fail("Failed to Update webhook: " + ex.Message, StatusCodeEnum.Invalid);
                }
                await _botConfigurationRepository.AddAsync(newConfig);

                return ApiResponse<BotConfigurationViewModel>.Success(_mapper.Map<BotConfigurationViewModel>(newConfig));
            }
            catch (Exception ex)
            {
                return ApiResponse<BotConfigurationViewModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }

        public async Task<ApiResponse<TeleTextModel>> GetTeleText()
        {
            try
            {
                var userListDto = await _teleTextRepository.GetSingleAsync();

                return ApiResponse<TeleTextModel>.Success(_mapper.Map<TeleTextModel>(userListDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<TeleTextModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }

        }
        public async Task<ApiResponse<TeleTextModel>> CreateOrUpdateTeleText(TeleTextRequest model)
        {
            try
            {
                var existingConfig = await _teleTextRepository.GetSingleAsync();
                if (existingConfig != null)
                {

                    existingConfig.JoinedReply = model.JoinedReply;
                    existingConfig.JoinSuccessReply = model.JoinSuccessReply;


                    await _teleTextRepository.UpdateAsync(existingConfig.Id, existingConfig);
                    return ApiResponse<TeleTextModel>.Success(_mapper.Map<TeleTextModel>(existingConfig));
                }


                var newConfig = new TeleTextDto
                {
                    Id = ObjectId.GenerateNewId(),
                    JoinSuccessReply = model.JoinSuccessReply,
                    JoinedReply = model.JoinedReply,
                };
                await _teleTextRepository.AddAsync(newConfig);

                return ApiResponse<TeleTextModel>.Success(_mapper.Map<TeleTextModel>(newConfig));
            }
            catch (Exception ex)
            {
                return ApiResponse<TeleTextModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
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

        private async Task UpdateWebhookWithNewApiKey(string keyValue)
        {
            var botClient = new TelegramBotClient(keyValue);

            var Webhook = _configuration.GetValue<string>("AdminAccount:Webhook");

            await botClient.GetUpdates(offset: -1);
            await botClient.SetWebhook(
                url: Webhook,
                allowedUpdates: new[] { UpdateType.Message }
            );
        }
        private async Task DeleteWebhook(string oldKeyValue,string newKeyvalue)
        {
            var newBotClient = new TelegramBotClient(newKeyvalue);
            var me = await newBotClient.GetMe();

            var botClient = new TelegramBotClient(oldKeyValue);
            await botClient.DeleteWebhook();
        }


    }
}
