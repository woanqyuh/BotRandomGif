
using BotTrungThuong.Models;
using BotTrungThuong.Repositories;
using MongoDB.Bson;
using BotTrungThuong.Dtos;
using AutoMapper;


namespace BotTrungThuong.Services
{
    public interface IUserService
    {

        Task<ApiResponse<List<UserModel>>> GetUsers();
        Task<ApiResponse<UserModel>> GetUserByIdAsync(ObjectId id);
        Task<ApiResponse<UserModel>> CreateUserAsync(RegisterModel model, ObjectId userId);   
        Task<ApiResponse<UserModel>> UpdateUserAsync(ObjectId id, UpdateUserModel model);  
        Task<ApiResponse<string>> DeleteUserAsync(ObjectId id);

        Task<ApiResponse<string>> ChangePasswordAsync(ObjectId userId, ChangePasswordModel model);
    }

    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;


        public UserService(IUserRepository userRepository, IAuthService authService, IMapper mapper)
        {
            _userRepository = userRepository;
            _authService = authService;
            _mapper = mapper;
        }
        public async Task<ApiResponse<List<UserModel>>> GetUsers()
        {
            try
            {
                var userListDto = await _userRepository.GetAllAsync();

                return ApiResponse<List<UserModel>>.Success(_mapper.Map<List<UserModel>>(userListDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserModel>>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }

        }
        public async Task<ApiResponse<UserModel>> GetUserByIdAsync(ObjectId id) 
        {
            try
            {
                var userDto = await _userRepository.GetByIdAsync(id);
                if (userDto == null)
                {
                    return ApiResponse<UserModel>.Fail("User not found", StatusCodeEnum.NotFound);
                }

                return ApiResponse<UserModel>.Success(_mapper.Map<UserModel>(userDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<UserModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }


        }
        public async Task<ApiResponse<UserModel>> CreateUserAsync(RegisterModel model, ObjectId userId)
        {
            try
            {
                model.Username = model.Username.Trim().ToLower();
                var existingUser = await _userRepository.GetByUsernameAsync(model.Username);
                if (existingUser != null)
                {
                    return ApiResponse<UserModel>.Fail($"User with username '{model.Username}' already exists.", StatusCodeEnum.Invalid);
                }
                var hashedPassword = _authService.HashPassword(model.Password);
                var user = new UserDto
                {
                    Id = ObjectId.GenerateNewId(),
                    Username = model.Username,
                    Password = hashedPassword,
                    Fullname = model.Fullname,
                    Role = model.Role,
                    CreatedBy = userId
                };
                await _userRepository.AddAsync(user);

                var userModel = _mapper.Map<UserModel>(user);
                return ApiResponse<UserModel>.Success(userModel, "User created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }



        public async Task<ApiResponse<UserModel>> UpdateUserAsync(ObjectId id, UpdateUserModel model)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return ApiResponse<UserModel>.Fail("User not found", StatusCodeEnum.NotFound);
                }

                user.Fullname = model.Fullname;
                user.Role = model.Role;

                await _userRepository.UpdateAsync(id, user);

                var userModel = _mapper.Map<UserModel>(user);
                return ApiResponse<UserModel>.Success(userModel, "User updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }


        public async Task<ApiResponse<string>> DeleteUserAsync(ObjectId id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return ApiResponse<string>.Fail("User not found", StatusCodeEnum.NotFound);
                }

                await _userRepository.DeleteAsync(id);
                return ApiResponse<string>.Success(null, "User deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
        public async Task<ApiResponse<string>> ChangePasswordAsync(ObjectId userId, ChangePasswordModel model)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return ApiResponse<string>.Fail("User not found", StatusCodeEnum.NotFound);
                }


                if (!_authService.VerifyPassword(model.OldPassword, user.Password))
                {
                    return ApiResponse<string>.Fail("Sai mật khẩu.", StatusCodeEnum.Invalid);
                }
                var hashedPassword = _authService.HashPassword(model.NewPassword);

                user.Password = hashedPassword;
                await _userRepository.UpdateAsync(userId, user);

                return ApiResponse<string>.Success(null, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }

    }

}
