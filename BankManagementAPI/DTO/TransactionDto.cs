namespace BankCustomerAPI.DTO
{
    public class TransactionDto
    {
        public decimal Amount { get; set; }
    }

    public class TransferDto
    {
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}
