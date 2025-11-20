using System.Security.Principal;

namespace BankCustomerAPI.Models
{
    public class Branch
    {
        public int BranchId { get; set; }
        public int BankId { get; set; }
        public string? BranchName { get; set; }
        public string? BranchCode { get; set; }
        public string? Address { get; set; }
        public int? ManagerUserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;

        public Bank? Bank { get; set; }
        public User? Manager { get; set; }
        public ICollection<Account> ?Accounts { get; set; }
    }
}