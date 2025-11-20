using BankCustomerAPI.Data;
using BankCustomerAPI.DTO;
using BankCustomerAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BankCustomerAPI.Service
{
    public class UserService
    {
        private readonly BankingDbContext _context;
        private readonly JwtService _jwtService;

        public UserService(BankingDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // -------------------------------------------------------
        // LOGIN WITH REFRESH TOKEN
        // -------------------------------------------------------
        public async Task<TokenResponse?> LoginWithRefreshTokenAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == email && !u.IsDeleted);

            if (user == null) return null;

            using var sha256 = SHA256.Create();
            var passwordHash = Convert.ToBase64String(
                sha256.ComputeHash(Encoding.UTF8.GetBytes(password))
            );

            if (user.PasswordHash != passwordHash)
                return null;

            var roles = GetRoles(user.UserType);

            string accessToken = _jwtService.GenerateAccessToken(
                user.UserId,
                $"{user.FirstName} {user.LastName}",
                roles
            );

            string refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15)
            };
        }

        // -------------------------------------------------------
        // REFRESH TOKEN
        // -------------------------------------------------------
        public async Task<TokenResponse?> RefreshTokenAsync(string refreshToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                return null;

            var roles = GetRoles(user.UserType);

            string newAccessToken = _jwtService.GenerateAccessToken(
                user.UserId,
                $"{user.FirstName} {user.LastName}",
                roles
            );

            string newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15)
            };
        }

        // -------------------------------------------------------
        // BASIC LOGIN
        // -------------------------------------------------------
        public async Task<string?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == email && !u.IsDeleted);

            if (user == null)
                return null;

            using var sha256 = SHA256.Create();
            var passwordHash = Convert.ToBase64String(
                sha256.ComputeHash(Encoding.UTF8.GetBytes(password))
            );

            if (user.PasswordHash != passwordHash)
                return null;

            var roles = GetRoles(user.UserType);

            return _jwtService.GenerateAccessToken(
                user.UserId,
                $"{user.FirstName} {user.LastName}",
                roles
            );
        }

        // -------------------------------------------------------
        // REGISTER
        // -------------------------------------------------------
        public async Task<bool> RegisterAsync(string firstName, string lastName, string email, string password, string role = "User")
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return false;

            using var sha256 = SHA256.Create();
            var passwordHash = Convert.ToBase64String(
                sha256.ComputeHash(Encoding.UTF8.GetBytes(password))
            );

            var newUser = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = passwordHash,
                UserType = role,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return true;
        }

        // -------------------------------------------------------
        // GET ALL USERS
        // -------------------------------------------------------
        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .Where(u => !u.IsDeleted)
                .ToListAsync();
        }

        // -------------------------------------------------------
        // UPDATE USER (Matches UsersController.UpdateUser)
        // -------------------------------------------------------
        public async Task<User?> UpdateUserAsync(int id, UpdateUserDto request)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.IsDeleted)
                return null;

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                using var sha256 = SHA256.Create();
                user.PasswordHash = Convert.ToBase64String(
                    sha256.ComputeHash(Encoding.UTF8.GetBytes(request.Password))
                );
            }

            if (!string.IsNullOrWhiteSpace(request.Role))
                user.UserType = request.Role;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return user;
        }

        // -------------------------------------------------------
        // DELETE USER (Soft Delete) – Matches UsersController.DeleteUser
        // -------------------------------------------------------
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || user.IsDeleted)
                return false;

            user.IsDeleted = true;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        // -------------------------------------------------------
        // CHECK IF ANY USERS EXIST
        // -------------------------------------------------------
        public async Task<bool> AnyUsersExistAsync()
        {
            return await _context.Users.AnyAsync(u => !u.IsDeleted);
        }

        // -------------------------------------------------------
        // ROLE MAPPING
        // -------------------------------------------------------
        private List<string> GetRoles(string userType)
        {
            var roles = new List<string>();

            switch (userType)
            {
                case "Admin":
                    roles.Add("Admin");
                    break;

                case "User":
                    roles.Add("User");
                    break;

                case "UserAndAdmin":
                    roles.Add("Admin");
                    roles.Add("User");
                    break;

                default:
                    roles.Add("NonUser");
                    break;
            }

            return roles;
        }
        public async Task<bool> LogoutAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return false;

            // Invalidate refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1); // Already expired

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id && !u.IsDeleted);
        }


    }
}
