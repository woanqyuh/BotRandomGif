using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BotTrungThuong.Models;
using BotTrungThuong.Services;

using System.Security.Claims;
using MongoDB.Bson;
using Telegram.Bot.Types;
using BotTrungThuong.Common;

namespace BotTrungThuong.Controllers
{
    [ApiController]
    [Route("api/list")]
    [Authorize]
    public class DanhSachTrungThuongController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IDanhSachTrungThuongService _danhSachTrungThuongService;
        private readonly IDanhSachTrungThuongOnlineService _danhSachTrungThuongOnlineService;
        private readonly IThamGiaTrungThuongService _thamGiaTrungThuongService;

        public DanhSachTrungThuongController(IUserService userService, IAuthService authService, IDanhSachTrungThuongService danhSachTrungThuongService, IThamGiaTrungThuongService thamGiaTrungThuongService, IDanhSachTrungThuongOnlineService danhSachTrungThuongOnlineService)
        {
            _userService = userService;
            _authService = authService;
            _danhSachTrungThuongService = danhSachTrungThuongService;
            _thamGiaTrungThuongService = thamGiaTrungThuongService;
            _danhSachTrungThuongOnlineService = danhSachTrungThuongOnlineService;
        }

        [HttpGet("winners")]
        [PermissionAuthorize(Permission.View)]
        public async Task<IActionResult> GetAllDanhSach()
        {
            try
            {
                var response = await _danhSachTrungThuongService.GetAll();
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
        [HttpGet("joiners")]
        [PermissionAuthorize(Permission.View)]
        public async Task<IActionResult> GetAllThamGia()
        {
            try
            {
                var response = await _thamGiaTrungThuongService.GetAll();
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
        [HttpDelete("winners/{id}")]
        [PermissionAuthorize(Permission.Delete)]
        public async Task<IActionResult> DeleteWinner(string id)
        {
            try
            {
                var response = await _danhSachTrungThuongService.DeleteAsync(ObjectId.Parse(id));
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

        [HttpDelete("joiners/{id}")]
        [PermissionAuthorize(Permission.Delete)]
        public async Task<IActionResult> DeleteJoiner(string id)
        {
            try
            {
                var response = await _thamGiaTrungThuongService.DeleteBySettingIdAsync(ObjectId.Parse(id));
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
        [HttpGet("joiners-online")]
        [PermissionAuthorize(Permission.View)]
        public async Task<IActionResult> GetAllThamGiaOnline()
        {
            try
            {
                var response = await _thamGiaTrungThuongService.GetNonWinners();
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
        [HttpPost("winners-online/spin/{id}")]
        [PermissionAuthorize(Permission.Create)]
        public async Task<IActionResult> SpinOnline(string id)
        {
            try
            {
                var response = await _danhSachTrungThuongOnlineService.Spin(ObjectId.Parse(id));
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

        [HttpDelete("winners-online/{id}")]
        [PermissionAuthorize(Permission.Delete)]
        public async Task<IActionResult> DeleteWinnerOnline(string id)
        {
            try
            {
                var response = await _danhSachTrungThuongOnlineService.DeleteAsync(ObjectId.Parse(id));
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

        [HttpPost("winners-online")]
        [PermissionAuthorize(Permission.View)]
        public async Task<IActionResult> GetAllDanhSachWinnerOnline([FromBody] TrungThuongFilter model)
        {
            try
            {
                var response = await _danhSachTrungThuongOnlineService.GetAll(model);
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

        [HttpPost("winners-online/send")]
        [PermissionAuthorize(Permission.Create)]
        public async Task<IActionResult> SendMessageToTele([FromBody]  SendMessageRequest model)
        {
            try
            {
                var response = await _danhSachTrungThuongOnlineService.SendMessageAsync(model.Ids);
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
