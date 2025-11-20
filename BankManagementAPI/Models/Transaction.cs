using BankCustomerAPI.Models;
using System.ComponentModel.DataAnnotations.Schema;

public class Transaction
{

   
    public int TransactionId { get; set; }
    public string? Type { get; set; }
    public decimal Amount { get; set; }
    public string? FromAccount { get; set; }
    public string? ToAccount { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public string? Status { get; set; } = "Completed";

    public int UserId { get; set; }
    public User? User { get; set; }

    public int AccountId { get; set; }
    public Account? Account { get; set; }   // Navigation property
}

