using BankCustomerAPI.DTO;
using BankCustomerAPI.Service;
using Microsoft.AspNetCore.Mvc;

namespace BankCustomerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionService _transactionService;

        public TransactionsController(TransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // GET: api/Transactions/account/{accountId}
        // Retrieve all transactions for a specific account
        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetByAccount(int accountId)
        {
            var transactions = await _transactionService.GetTransactionsByAccountAsync(accountId);

            if (transactions == null || !transactions.Any())
                return NotFound(new { Message = "No transactions found for this account." });

            return Ok(transactions);
        }
    }
}
