using BankCustomerAPI.DTO;
using BankCustomerAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankCustomerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Extract userId from JWT claims
            int userId = int.Parse(User.FindFirst("UserId")!.Value);

            // Get the user from database
            var user = await _userService.GetByIdAsync(userId);

            if (user == null)
                return NotFound();

            // Return only safe info
            var userDto = new
            {
                user.UserId,
                FullName = $"{user.FirstName} {user.LastName}",
                user.Email,
                user.UserType
            };

            return Ok(userDto);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();

            var userDtos = users.Select(u => new
            {
                u.UserId,
                u.FirstName,
                u.LastName,
                u.Email,
                u.UserType,
                u.CreatedDate
            }).ToList();

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            var userDto = new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.UserType,
                user.CreatedDate
            };

            return Ok(userDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var updatedUser = await _userService.UpdateUserAsync(id, dto);
            if (updatedUser == null) return NotFound();

            var userDto = new
            {
                updatedUser.UserId,
                updatedUser.FirstName,
                updatedUser.LastName,
                updatedUser.Email,
                updatedUser.UserType,
                updatedUser.CreatedDate
            };

            return Ok(userDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound();

            return Ok(new { Message = "User deleted successfully" });
        }
    }
}
