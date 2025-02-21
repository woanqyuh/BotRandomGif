﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel.DataAnnotations;


namespace BotTrungThuong.Models
{
    public class UserModel
    {

        public string Id { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
    }

    public class UserViewModel
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public UserRole Role { get; set; }

    }
    public class UpdateUserModel
    {
        [Required(ErrorMessage = "Fullname là bắt buộc.")]
        public string Fullname { get; set; }
        [Required(ErrorMessage = "TeleUser là bắt buộc.")]
        public UserRole Role { get; set; }

    }

    public class AuthResponse
    {
        public UserViewModel User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
    public class LoginResponse
    {
        public string ChatId { get; set; }
        public string UserId { get; set; }
    }

    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class VerifyCodeModel
    {
        [Required(ErrorMessage = "ChatId là bắt buộc.")]
        public string ChatId { get; set; }

        [Required(ErrorMessage = "Mã xác thực là bắt buộc.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "UserId là bắt buộc.")]
        public string UserId { get; set; }
    }

    public class ChangePasswordModel
    {

        [Required(ErrorMessage = "Password là bắt buộc.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password là bắt buộc.")]
        [MinLength(6, ErrorMessage = "New Password phải có ít nhất 6 ký tự.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "ConfirmNewPassword là bắt buộc.")]
        [Compare("NewPassword", ErrorMessage = "New Password và Confirm New Password phải khớp.")]
        public string ConfirmNewPassword { get; set; }
    }
}
