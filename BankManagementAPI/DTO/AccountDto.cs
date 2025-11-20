namespace BankCustomerAPI.DTO
{
    public class AccountDto
    {
        public int AccountId { get; set; }
        public int BranchId { get; set; }
        public string AccountNumber { get; set; } = null!;
        public string AccountType { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public decimal Balance { get; set; }
        public bool IsMinor { get; set; }
        public int? PowerOfAttorneyUserId { get; set; }
        public bool IsClosed { get; set; }
        public DateTime CreatedDate { get; set; }

        // Added
        public string? BranchName { get; set; }
        public string? BankName { get; set; }
    }
}
