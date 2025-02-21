using BotTrungThuong.Dtos;
using BotTrungThuong.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Bson;
using Quartz;
using System.Linq;
using System.Reflection;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace BotTrungThuong.Jobs
{
    public class LuckyDrawJob : IJob
    {
        private readonly IThietLapTrungThuongRepository _thietLapTrungThuongRepository;
        private readonly IBotConfigurationRepository _botConfigurationRepository;
        private readonly IThamGiaTrungThuongRepository _thamGiaTrungThuongRepository;
        private readonly IDanhSachTrungThuongRepository _danhSachTrungThuongRepository;

        public LuckyDrawJob(
            IThietLapTrungThuongRepository thietLapTrungThuongRepository,
            IBotConfigurationRepository botConfigurationRepository,
            IThamGiaTrungThuongRepository thamGiaTrungThuongRepository,
            IDanhSachTrungThuongRepository danhSachTrungThuongRepository)
        {
            _thietLapTrungThuongRepository = thietLapTrungThuongRepository;
            _botConfigurationRepository = botConfigurationRepository;
            _thamGiaTrungThuongRepository = thamGiaTrungThuongRepository;
            _danhSachTrungThuongRepository = danhSachTrungThuongRepository;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"LuckyDrawJob started at {DateTime.UtcNow}");

            var jobDataMap = context.JobDetail.JobDataMap;

            var id = jobDataMap.GetString("ThietLapId");
            var thietlap = await _thietLapTrungThuongRepository.GetByIdAsync(ObjectId.Parse(id));
            if (thietlap == null)
            {
                return;
            }
            var rewardSchedule = jobDataMap.Get("RewardSchedule") as RewardSchedule;
            if (rewardSchedule == null)
            {
                return;
            }

            //var thamgiaList = await _thamGiaTrungThuongRepository.GetAllBySettingIdAsync(ObjectId.Parse(id));
            var thamgiaAllList = await  _thamGiaTrungThuongRepository.GetAllBySettingIdAsync(ObjectId.Parse(id));

            var trungthuongAllList = await _danhSachTrungThuongRepository.GetAllAsync() ?? new List<DanhSachTrungThuongDto>();
            var trungThuongMemberIds = trungthuongAllList.Select(x => x.UserId).ToList();
            var thamgiaList = thamgiaAllList.Where(x => !trungThuongMemberIds.Contains(x.UserId)).ToList();
            var botClient = await GetBotClientAsync();

            var allWinnersList = new List<ThamGiaTrungThuongDto>();

            if (thamgiaList.Any())
            {
                var random = new Random();
                var potentialWinners = thamgiaList.ToList();
                var winners = new HashSet<string>();
                var messageText = EscapeMarkdown(thietlap.Content) + "\n\n";

                foreach (var reward in rewardSchedule.Rewards)
                {
                    int rewardQuantity = reward.Quantity;
                    var rewardWinners = new List<string>();
                    for (int i = 0; i < rewardQuantity; i++)
                    {
                        if (potentialWinners.Any())
                        {
                            var availableWinners = potentialWinners.Where(w => !winners.Contains(w.UserId)).ToList();
                            if (!availableWinners.Any())
                            {
                                Console.WriteLine("No more participants available for this reward.");
                                break;
                            }
                            var winner = availableWinners[random.Next(availableWinners.Count)];

                            allWinnersList.Add(winner);
                            winners.Add(winner.UserId);
                            string winnerMessage = $"[{EscapeMarkdown(winner.Fullname)}](https://t.me/{winner.Username}) nhận giải thưởng {EscapeMarkdown(reward.Gift)}";
                            rewardWinners.Add(winnerMessage);
                            await SaveWinnerToDatabaseAsync(winner, thietlap, reward, rewardSchedule);
                        }

                    }
                    if (rewardWinners.Any())
                    {
                        messageText += string.Join("\n", rewardWinners) + "\n";
                    }
                }

                messageText += EscapeMarkdown("\n\n Liên hệ quản lý nhóm để nhận phần thưởng");

                using var client = new HttpClient();
              
                if (allWinnersList.Any()) 
                {
                    var firstWinner = allWinnersList.First();
                    if (!string.IsNullOrEmpty(thietlap.ImageUrl) && Uri.IsWellFormedUriString(thietlap.ImageUrl, UriKind.Absolute))
                    {
                        var responsec = await client.GetAsync(thietlap.ImageUrl);

                        if (responsec.IsSuccessStatusCode)
                        {
                            var contentType = responsec.Content.Headers.ContentType?.MediaType;
                            if (contentType.StartsWith("image/"))
                            {
                                var stream = await responsec.Content.ReadAsStreamAsync();
                                await botClient.SendPhoto(firstWinner.FromChatId, stream, messageText, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2, replyParameters: firstWinner.FromMessageId);

                            }
                            else if (contentType.StartsWith("video/"))
                            {
                                var stream = await responsec.Content.ReadAsStreamAsync();
                                await botClient.SendVideo(firstWinner.FromChatId, stream, caption: messageText, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2, replyParameters: firstWinner.FromMessageId);

                            }
                        }
                    }
                    else
                    {
                        await botClient.SendMessage(firstWinner.FromChatId, messageText, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2, replyParameters: firstWinner.FromMessageId);
                    }
                }
                else
                {
                    Console.WriteLine("No winners found. Skipping message sending.");
                }




            }
            else
            {
                Console.WriteLine("No participants found for this gift setup.");
            }


        }
        private int ExtractNumberFromGift(string gift)
        {

            var numberPart = new string(gift.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());


            return int.TryParse(numberPart, out int result) ? result : 0;
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

        private async Task SaveWinnerToDatabaseAsync(ThamGiaTrungThuongDto winner, ThietLapTrungThuongDto thietlap,RewardItem reward, RewardSchedule rewardSchedule)
        {
            var trungThuong = new DanhSachTrungThuongDto
            {
                Id = ObjectId.GenerateNewId(),
                Fullname = winner.Fullname,
                ThietLapId = thietlap.Id,
                Username = winner.Username,
                UserId = winner.UserId,
                ThietLapName = thietlap.Name,
                ChatId = thietlap.ChatId,
                Gift = reward.Gift,
                ScheduleName = rewardSchedule.Name

            };
            await _danhSachTrungThuongRepository.AddAsync(trungThuong);
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
    }
}
