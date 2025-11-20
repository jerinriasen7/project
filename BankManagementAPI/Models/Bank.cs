namespace BankCustomerAPI.Models
{
    public class Bank
    {
        public int BankId { get; set; }
        public string? BankName { get; set; }
        public string? HeadOfficeAddress { get; set; }
        public string? IFSCCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<Branch>? Branches { get; set; }
    }
}
