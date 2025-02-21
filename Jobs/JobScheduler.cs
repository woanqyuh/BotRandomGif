using BotTrungThuong.Dtos;
using BotTrungThuong.Repositories;
using MongoDB.Bson;
using Quartz;
using Quartz.Impl.Matchers;

namespace BotTrungThuong.Jobs
{
    public class JobScheduler
    {
        private readonly IScheduler _scheduler;
        private readonly IThietLapTrungThuongRepository _thietLapTrungThuongRepository;

        public JobScheduler(IScheduler scheduler, IThietLapTrungThuongRepository thietLapTrungThuongRepository)
        {
            _scheduler = scheduler;
            _thietLapTrungThuongRepository = thietLapTrungThuongRepository;
        }


        public async Task ScheduleJobs(ThietLapTrungThuongDto thietlap)
        {

            
            foreach (var schedule in thietlap.RewardSchedules)
            {

                if (DateTime.UtcNow > schedule.ResultTime)
                {
                    Console.WriteLine($"The scheduled time {schedule.ResultTime} has already passed.");
                    continue;
                }

                var jobId = $"{thietlap.Id.ToString()}-{schedule.ResultTime}";
                var jobKey = new JobKey(jobId, "ThietLapTrungThuong");

                var jobDataMap = new JobDataMap
                    {
                        { "ThietLapId", thietlap.Id.ToString() },
                        { "RewardSchedule", schedule }
                    };

                IJobDetail job = JobBuilder.Create<LuckyDrawJob>()
                    .WithIdentity(jobKey)
                    .UsingJobData(jobDataMap)
                    .Build();
                var trigger = TriggerBuilder.Create()
                    .WithIdentity($"{jobId}-trigger", "ThietLapTrungThuong")
                    .StartAt(schedule.ResultTime) 
                    .Build();

                try
                {
                    await _scheduler.ScheduleJob(job, trigger);
                    var tl = await _thietLapTrungThuongRepository.GetByIdAsync(thietlap.Id);
                    tl.Status = (int)GiftSettingStatus.InProgress;
                    await _thietLapTrungThuongRepository.UpdateAsync(thietlap.Id, tl);
                    Console.WriteLine($"Scheduled job for RewardSchedule at {schedule.ResultTime}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error scheduling job: {ex.Message}");
                }
            }
        }
        public async Task RescheduleJobs(ThietLapTrungThuongDto thietlap)
        {
            await DeleteScheduledJobs(thietlap.Id);
            await ScheduleJobs(thietlap);
        }
        public async Task DeleteScheduledJobs(ObjectId thietlapId)
        {
            var group = "ThietLapTrungThuong";
            var jobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));

            foreach (var jobKey in jobKeys)
            {
                if (jobKey.Name.StartsWith(thietlapId.ToString()))
                {
                    if (await _scheduler.CheckExists(jobKey))
                    {
                        await _scheduler.DeleteJob(jobKey);
                        Console.WriteLine($"Deleted job: {jobKey.Name}");
                    }
                }
            }
        }
    }
}
