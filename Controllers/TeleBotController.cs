using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BotTrungThuong.Models;
using BotTrungThuong.Services;
using MongoDB.Bson;
using System.Security.Claims;

using System.Text.Json;
using Telegram.Bot;

using Quartz;
using BotTrungThuong.Common;

using Newtonsoft.Json;


namespace BotTrungThuong.Controllers
{
    [ApiController]
    [Route("api/telebot")]
    [Authorize]
    public class TeleBotController : ControllerBase
    {
        private readonly ITelegramBotService _telegramBotService;
        private readonly ISchedulerFactory _schedulerFactory;

        public TeleBotController(
            ITelegramBotService telegramBotService,
            ISchedulerFactory schedulerFactory)
        {
            _telegramBotService = telegramBotService;
        }




        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Post([FromBody] dynamic payload)
        {
            try
            {
                var response = await _telegramBotService.HandleWebHook(payload);
                if (!response.IsOk)
                {
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)StatusCodeEnum.InternalServerError, new { message = ex.Message });
            }
        }
        [HttpGet("get-api")]
        [PermissionAuthorize(Permission.View)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var response = await _telegramBotService.GetApiKey();
                if (!response.IsOk)
                {
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)StatusCodeEnum.InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost("upsert-api")]
        [PermissionAuthorize(Permission.Create)]
        public async Task<IActionResult> Create([FromBody] BotConfigurationRequest model)
        {
            try
            {
                var response = await _telegramBotService.CreateOrUpdateApiKey(model.KeyValue);
                if (!response.IsOk)
                {
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)StatusCodeEnum.InternalServerError, new { message = ex.Message });
            }
        }

        [HttpGet("get-text")]
        [PermissionAuthorize(Permission.View)]
        public async Task<IActionResult> GetText()
        {
            try
            {
                var response = await _telegramBotService.GetTeleText();
                if (!response.IsOk)
                {
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)StatusCodeEnum.InternalServerError, new { message = ex.Message });
            }
        }

        [HttpPost("upsert-text")]
        [PermissionAuthorize(Permission.Create)]
        public async Task<IActionResult> CreateText([FromBody] TeleTextRequest model)
        {
            try
            {
                var response = await _telegramBotService.CreateOrUpdateTeleText(model);
                if (!response.IsOk)
                {
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode((int)StatusCodeEnum.InternalServerError, new { message = ex.Message });
            }
        }
    }
}
