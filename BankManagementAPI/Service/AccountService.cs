using BankCustomerAPI.Data;
using BankCustomerAPI.DTO;
using BankCustomerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BankCustomerAPI.Service
{
    public class AccountService
    {
        private readonly BankingDbContext _context;

        public AccountService(BankingDbContext context)
        {
            _context = context;
        }

        // List user accounts
        public async Task<List<Account>> GetAccountsByUserAsync(int userId)
        {
            return await _context.Accounts
                .Where(a => a.UserId == userId && !a.IsClosed)
                .Include(a => a.Branch)
                    .ThenInclude(b => b.Bank)
                .ToListAsync();
        }

        // Admin: list all accounts
        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _context.Accounts
                .Where(a => !a.IsClosed)
                .Include(a => a.Branch)
                    .ThenInclude(b => b.Bank)
                .ToListAsync();
        }

        // Create account
        public async Task<Account> CreateAccountAsync(int userId, CreateAccountDto dto)
        {
            var account = new Account
            {
                UserId = userId,
                BranchId = dto.BranchId,
                AccountType = dto.AccountType,
                CurrencyCode = dto.CurrencyCode,
                Balance = dto.InitialBalance,
                IsMinor = dto.IsMinor,
                PowerOfAttorneyUserId = dto.PowerOfAttorneyUserId,
                MaturityDate = dto.MaturityDate,
                InterestRate = dto.InterestRate,
                AccountNumber = GenerateAccountNumber(),
                IsClosed = false,
                CreatedDate = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        // Deposit
        public async Task<bool> DepositAsync(int accountId, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null || account.IsClosed) return false;

            account.Balance += amount;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return true;
        }

        // Withdraw
        public async Task<bool> WithdrawAsync(int accountId, decimal amount)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null || account.IsClosed || account.Balance < amount) return false;

            account.Balance -= amount;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return true;
        }

        // Transfer
        public async Task<bool> TransferAsync(int fromAccountId, int toAccountId, decimal amount)
        {
            if (fromAccountId == toAccountId) return false;

            var fromAccount = await _context.Accounts.FindAsync(fromAccountId);
            var toAccount = await _context.Accounts.FindAsync(toAccountId);

            if (fromAccount == null || toAccount == null) return false;
            if (fromAccount.IsClosed || toAccount.IsClosed) return false;
            if (fromAccount.Balance < amount) return false;

            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            _context.Accounts.Update(fromAccount);
            _context.Accounts.Update(toAccount);
            await _context.SaveChangesAsync();
            return true;
        }

        // Close account
        public async Task<bool> CloseAccountAsync(int accountId, int userId, bool isAdmin)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null || account.IsClosed) return false;
            if (!isAdmin && account.UserId != userId) return false;

            account.IsClosed = true;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GenerateAccountNumber()
        {
            var random = new Random();
            return random.Next(10000000, 99999999).ToString();
        }
    }
}
