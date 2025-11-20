namespace BankCustomerAPI.DTO
{
    public class CreateAccountDto
    {
        public int BranchId { get; set; }
        public string AccountType { get; set; } = "Savings";
        public string CurrencyCode { get; set; } = "USD";
        public decimal InitialBalance { get; set; } = 0;
        public bool IsMinor { get; set; } = false;
        public int? PowerOfAttorneyUserId { get; set; }
        public DateTime? MaturityDate { get; set; }
        public decimal? InterestRate { get; set; }
    }

}
