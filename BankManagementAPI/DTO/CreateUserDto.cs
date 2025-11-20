namespace BankCustomerAPI.DTO
{
    public class CreateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string UserType { get; set; } = "User"; // default to "User"
        public int RoleId { get; set; } // Role assignment
    }
}
