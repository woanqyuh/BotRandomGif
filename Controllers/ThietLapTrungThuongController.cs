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
    [Route("api/settings")]
    [Authorize]
    public class ThietLapTrungThuongController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IThietLapTrungThuongService _thietLapTrungThuongService;

        public ThietLapTrungThuongController(IUserService userService, IAuthService authService, IThietLapTrungThuongService thietLapTrungThuongService)
        {
            _userService = userService;
            _authService = authService;
            _thietLapTrungThuongService = thietLapTrungThuongService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission.View)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var response = await _thietLapTrungThuongService.GetAll();
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

        [HttpPost("create")]
        [PermissionAuthorize(Permission.Create)]
        public async Task<IActionResult> Create([FromBody] CreateThietLapTrungThuongRequest model)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                var response = await _thietLapTrungThuongService.CreateAsync(model, ObjectId.Parse(userIdClaim));
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


        [HttpPut("update/{id}")]
        [PermissionAuthorize(Permission.Edit)]
        public async Task<IActionResult> Update(string id, [FromBody] CreateThietLapTrungThuongRequest model)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                var response = await _thietLapTrungThuongService.UpdateAsync(ObjectId.Parse(id), model);
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
        [HttpDelete("delete/{id}")]
        [PermissionAuthorize(Permission.Delete)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                var response = await _thietLapTrungThuongService.DeleteAsync(ObjectId.Parse(id));
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

        //[HttpPut("start/{id}")]
        //[PermissionAuthorize(Permission.Create)]
        //public async Task<IActionResult> Start(string id)
        //{
        //    try
        //    {

        //        var response = await _thietLapTrungThuongService.StartJobAsync(ObjectId.Parse(id));
        //        if (!response.IsOk)
        //        {
        //            return StatusCode(response.StatusCode, response);
        //        }
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)StatusCodeEnum.InternalServerError, new { message = ex.Message });
        //    }
        //}

        //[HttpDelete("stop/{id}")]
        //[PermissionAuthorize(Permission.Create)]
        //public async Task<IActionResult> Stop(string id)
        //{
        //    try
        //    {

        //        var response = await _thietLapTrungThuongService.StopJobAsync(ObjectId.Parse(id));
        //        if (!response.IsOk)
        //        {
        //            return StatusCode(response.StatusCode, response);
        //        }
        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode((int)StatusCodeEnum.InternalServerError, new { message = ex.Message });
        //    }
        //}
    }
}
