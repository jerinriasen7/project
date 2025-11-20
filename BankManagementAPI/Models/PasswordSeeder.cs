using BCrypt.Net;

namespace BankCustomerAPI.Data
{
    public static class PasswordSeeder
    {
        // Pre-hashed passwords for seeding (using BCrypt)
        public static readonly string AdminPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        public static readonly string UserPassword = BCrypt.Net.BCrypt.HashPassword("User@123");
        public static readonly string AdminUserPassword = BCrypt.Net.BCrypt.HashPassword("AdminUser@123");
        public static readonly string NoUserPassword = BCrypt.Net.BCrypt.HashPassword("NoUser@123");
    }
}