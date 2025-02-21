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
    [Route("api/gift")]
    [Authorize]
    public class GiaiThuongController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IGiaiThuongService _giaiThuongService;

        public GiaiThuongController(IUserService userService, IAuthService authService, IGiaiThuongService giaiThuongService)
        {
            _userService = userService;
            _authService = authService;
            _giaiThuongService = giaiThuongService;
        }

        [HttpGet]
        [PermissionAuthorize(Permission.View)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var response = await _giaiThuongService.GetAll();
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
        public async Task<IActionResult> Create([FromBody] GiaiThuongRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(errors); // Trả về lỗi nếu có
                }

                var response = await _giaiThuongService.CreateAsync(model);
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
        public async Task<IActionResult> Update(string id, [FromBody] GiaiThuongRequest model)
        {
            try
            {
                var response = await _giaiThuongService.UpdateAsync(ObjectId.Parse(id), model);
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
                var response = await _giaiThuongService.DeleteAsync(ObjectId.Parse(id));
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
