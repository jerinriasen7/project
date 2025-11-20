using BankCustomerAPI.Data;
using BankCustomerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BankCustomerAPI.Service
{
    public class TransactionService
    {
        private readonly BankingDbContext _context;

        public TransactionService(BankingDbContext context)
        {
            _context = context;
        }

        // Get all transactions for an account
        public async Task<List<Transaction>> GetTransactionsByAccountAsync(int accountId)
        {
            return await _context.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        // Deposit
        public async Task<Transaction> DepositAsync(int accountId, decimal amount, int userId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) throw new Exception("Account not found");

            account.Balance += amount;

            var txn = new Transaction
            {
                AccountId = accountId,
                UserId = userId,
                Amount = amount,
                Type = "Deposit",
                Status = "Completed",
                TransactionDate = DateTime.UtcNow,
                FromAccount = null,
                ToAccount = account.AccountNumber
            };

            _context.Transactions.Add(txn);
            await _context.SaveChangesAsync();

            return txn;
        }

        // Withdraw
        public async Task<Transaction> WithdrawAsync(int accountId, decimal amount, int userId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) throw new Exception("Account not found");
            if (account.Balance < amount) throw new Exception("Insufficient balance");

            account.Balance -= amount;

            var txn = new Transaction
            {
                AccountId = accountId,
                UserId = userId,
                Amount = amount,
                Type = "Withdraw",
                Status = "Completed",
                TransactionDate = DateTime.UtcNow,
                FromAccount = account.AccountNumber,
                ToAccount = null
            };

            _context.Transactions.Add(txn);
            await _context.SaveChangesAsync();

            return txn;
        }

        // Transfer (one transaction per account)
        public async Task<Transaction> TransferAsync(int fromAccountId, int toAccountId, decimal amount, int userId)
        {
            using var dbTx = await _context.Database.BeginTransactionAsync();

            var fromAcc = await _context.Accounts.FindAsync(fromAccountId);
            var toAcc = await _context.Accounts.FindAsync(toAccountId);

            if (fromAcc == null || toAcc == null)
                throw new Exception("Account not found");
            if (fromAcc.Balance < amount)
                throw new Exception("Insufficient balance");

            fromAcc.Balance -= amount;
            toAcc.Balance += amount;

            // Log only one transaction (from source account perspective)
            var txn = new Transaction
            {
                AccountId = fromAccountId,
                UserId = userId,
                Amount = amount,
                Type = "Transfer",
                Status = "Completed",
                TransactionDate = DateTime.UtcNow,
                FromAccount = fromAcc.AccountNumber,
                ToAccount = toAcc.AccountNumber
            };

            _context.Transactions.Add(txn);
            await _context.SaveChangesAsync();

            await dbTx.CommitAsync();
            return txn;
        }
    }
}
