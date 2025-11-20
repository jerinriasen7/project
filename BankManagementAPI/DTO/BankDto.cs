namespace BankCustomerAPI.DTO
{
    public class BankDto
    {
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string IfscCode { get; set; }

        public List<BranchDto>? Branches { get; set; }
    }
}
