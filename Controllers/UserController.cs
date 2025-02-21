﻿using Microsoft.AspNetCore.Authorization;
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
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UserController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        [HttpGet("users")]
        [PermissionAuthorize(Permission.View)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var response = await _userService.GetUsers();
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
        public async Task<IActionResult> Create([FromBody] RegisterModel model)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                var response = await _userService.CreateUserAsync(model, ObjectId.Parse(userIdClaim));
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
        public async Task<IActionResult> Update(string id, [FromBody] UpdateUserModel model)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.Name)?.Value;
                var response = await _userService.UpdateUserAsync(ObjectId.Parse(id), model);
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
                var response = await _userService.DeleteUserAsync(ObjectId.Parse(id));
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

        [HttpPut("change-password/{id}")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordModel model)
        {
            try
            {
                var response = await _userService.ChangePasswordAsync(ObjectId.Parse(id), model);
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
