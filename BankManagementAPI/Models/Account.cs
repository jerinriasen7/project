using System;

namespace BankCustomerAPI.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public int BranchId { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountType { get; set; }
        public string? CurrencyCode { get; set; } 
        public decimal Balance { get; set; }
        public bool IsMinor { get; set; }
        public int? PowerOfAttorneyUserId { get; set; }
        public DateTime? MaturityDate { get; set; }
        public decimal? InterestRate { get; set; }
        public bool IsClosed { get; set; }
        public DateTime CreatedDate { get; set; }

        public User? User { get; set; }
        public Branch? Branch { get; set; }
        public User? PowerOfAttorneyUser { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    }
}