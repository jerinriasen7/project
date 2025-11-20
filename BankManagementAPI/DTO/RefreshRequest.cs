namespace BankCustomerAPI.DTO
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = null!;
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }
}

