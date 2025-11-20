namespace BankCustomerAPI.DTO
{
    public class CreateBranchDto
    {
        public int BankId { get; set; } // FK to Bank table
        public string BranchName { get; set; } = null!;
        public string BranchCode { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int? ManagerUserId { get; set; } // Optional FK to User
    }

}
