using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankCustomerAPI.Service
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Generates a JWT access token for a user.
        /// </summary>
        public string GenerateAccessToken(int userId, string username, List<string> roles, List<string>? permissions = null)
        {
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.UniqueName, username),

        // ✅ ADD THIS (important)
        new Claim("UserId", userId.ToString())
    };

            // Add roles
            if (roles != null)
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Add permissions
            if (permissions != null)
                claims.AddRange(permissions.Select(p => new Claim("permission", p)));

            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(keyString) || keyString.Length < 32)
                throw new ArgumentException("JWT Key must be at least 256 bits (32 characters) long.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiryMinutes = int.TryParse(_configuration["Jwt:AccessTokenExpiryMinutes"], out var duration)
                ? duration
                : 15;

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Generates a secure random refresh token.
        /// </summary>
        public string GenerateRefreshToken()
        {
            // Use RNGCryptoServiceProvider or RandomNumberGenerator
            var randomBytes = new byte[64];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Validates a JWT access token and returns claims principal.
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
