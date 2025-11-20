using BankCustomerAPI.DTO;
using BankCustomerAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BankCustomerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtService _jwtService;

        public AuthController(UserService userService, JwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        // ---------------- REGISTER ----------------
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { Message = "Email and Password are required." });

            bool isFirstUser = !await _userService.AnyUsersExistAsync();
            string role = isFirstUser ? "Admin" : (string.IsNullOrWhiteSpace(request.UserType) ? "User" : request.UserType);

            bool created = await _userService.RegisterAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                role
            );

            if (!created)
                return Conflict(new { Message = "User with this email already exists." });

            return Ok(new { Message = $"User registered successfully as {role}." });
        }

        // ---------------- LOGIN ----------------
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { Message = "Email and Password are required." });

            var authResult = await _userService.LoginWithRefreshTokenAsync(request.Email, request.Password);

            if (authResult == null)
                return Unauthorized(new { Message = "Invalid email or password." });

            return Ok(new TokenResponse
            {
                AccessToken = authResult.AccessToken,
                RefreshToken = authResult.RefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15) // or get from config
            });
        }

        // ---------------- REFRESH TOKEN ----------------
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new { Message = "Refresh token is required." });

            var newTokens = await _userService.RefreshTokenAsync(request.RefreshToken);

            if (newTokens == null)
                return Unauthorized(new { Message = "Invalid or expired refresh token." });

            return Ok(new TokenResponse
            {
                AccessToken = newTokens.AccessToken,
                RefreshToken = newTokens.RefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15)
            });
        }
        // ---------------------------------------
        // VERIFY TOKEN
        // ---------------------------------------
        [AllowAnonymous]
        [HttpGet("verify")]
        public IActionResult Verify()
        {
            return Ok(new
            {
                message = "Token is valid",
                userId = User.FindFirst("userId")?.Value,
                username = User.Identity?.Name,
                roles = User.Claims.Where(c => c.Type == "role").Select(c => c.Value)
            });
        }


        [HttpPost("logout")]
        [Authorize]  // user must be logged in
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> Logout()
        {
            // Get userId from JWT claims
            var userId = int.Parse(User.FindFirst("sub")!.Value);

            var result = await _userService.LogoutAsync(userId);

            if (!result)
                return BadRequest("Logout failed.");

            return Ok("Logged out successfully.");
        }


    }
}
