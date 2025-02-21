using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BotTrungThuong.Models;
using BotTrungThuong.Dtos;
using BotTrungThuong.Repositories;
using BotTrungThuong.Services;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text;
using Quartz;
using Quartz.Impl;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using BotTrungThuong.Jobs;
using Quartz.Spi;


var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "wwwroot")
});

// SignalR
builder.Services.AddSignalR();

// MongoDB
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<IMongoClient>(s =>
{
    var mongoDbSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>();
    return new MongoClient(mongoDbSettings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(s =>
{
    var mongoClient = s.GetRequiredService<IMongoClient>();
    var mongoDbSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>();
    return mongoClient.GetDatabase(mongoDbSettings.DatabaseName);
});

// JWT
var jwtSettings = builder.Configuration.GetSection("JWT");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["AccessTokenSecret"])),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
});

//// Đăng ký IScheduler là Singleton
//builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
//builder.Services.AddSingleton<IScheduler>(provider =>
//{
//    var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
//    return schedulerFactory.GetScheduler().Result;  // Khởi tạo Scheduler
//});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IThietLapTrungThuongService, ThietLapTrungThuongService>();
builder.Services.AddScoped<IDanhSachTrungThuongService, DanhSachTrungThuongService>();
builder.Services.AddScoped<IDanhSachTrungThuongOnlineService, DanhSachTrungThuongOnlineService>();
builder.Services.AddScoped<IThamGiaTrungThuongService, ThamGiaTrungThuongService>();
builder.Services.AddScoped<IGiaiThuongService, GiaiThuongService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IThietLapTrungThuongRepository, ThietLapTrungThuongRepository>();
builder.Services.AddScoped<IThamGiaTrungThuongRepository, ThamGiaTrungThuongRepository>();
builder.Services.AddScoped<IDanhSachTrungThuongRepository, DanhSachTrungThuongRepository>();
builder.Services.AddScoped<IBotConfigurationRepository, BotConfigurationRepository>();
builder.Services.AddScoped<IDanhSachTrungThuongOnlineRepository, DanhSachTrungThuongOnlineRepository>();
builder.Services.AddScoped<ITeleTextRepository, TeleTextRepository>();
builder.Services.AddScoped<IGiaiThuongRepository, GiaiThuongRepository>();
builder.Services.AddAutoMapper(typeof(MappingProfile));


builder.Services.AddSingleton<IJobFactory, ScopedJobFactory>();
builder.Services.AddSingleton<IScheduler>(provider =>
{
    var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
    var scheduler = schedulerFactory.GetScheduler().Result;
    scheduler.JobFactory = provider.GetRequiredService<IJobFactory>();
    scheduler.Start().Wait();
    return scheduler;
});
builder.Services.AddScoped<JobScheduler>();
builder.Services.AddTransient<LuckyDrawJob>();
builder.Services.AddScoped<ThietLapTrungThuongScheduler>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token JWT vào dưới dạng: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});




builder.Services.AddMemoryCache();


builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});

builder.Services.AddControllers();

var app = builder.Build();
var adminAccountSettings = builder.Configuration.GetSection("AdminAccount").Get<AdminAccountSettings>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(x => x
  .AllowAnyOrigin()
  .AllowAnyMethod()
  .AllowAnyHeader());

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userRepository = services.GetRequiredService<IUserRepository>();
    var authService = services.GetRequiredService<IAuthService>();
    var scheduler = scope.ServiceProvider.GetRequiredService<IScheduler>();
    if (!scheduler.IsStarted)
    {
        await scheduler.Start();
    }
    var tlScheduler = services.GetRequiredService<ThietLapTrungThuongScheduler>();
    await tlScheduler.Start();
    var existingAdmin = await userRepository.GetByUsernameAsync(adminAccountSettings.Username);

    if (existingAdmin == null)
    {
        var hashedPassword = authService.HashPassword(adminAccountSettings.Password);

        var adminUser = new UserDto
        {
            Id = ObjectId.GenerateNewId(),
            Username = adminAccountSettings.Username,
            Password = hashedPassword,
            Fullname = adminAccountSettings.Fullname,
            Role = UserRole.Admin,
        };
        await userRepository.AddAsync(adminUser);

    }

    var botConfigurationRepository = scope.ServiceProvider.GetRequiredService<IBotConfigurationRepository>();
    var botConfig = await botConfigurationRepository.GetSingleAsync();

    if (botConfig == null || string.IsNullOrWhiteSpace(botConfig.KeyValue))
    {
        throw new Exception("API Key not found in the database.");
    }

    var botClient = new TelegramBotClient(botConfig.KeyValue);
  
    await botClient.DeleteWebhook();
    await botClient.GetUpdates(offset: -1);
    await botClient.SetWebhook(
    url: adminAccountSettings.Webhook,
    allowedUpdates: new[] { UpdateType.Message }
);
}


app.UseHttpsRedirection();
app.UseStaticFiles();

//  Authentication  Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map  controller
app.MapControllers();

app.Run();
