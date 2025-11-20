using BankCustomerAPI.DTO;
using BankCustomerAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace BankCustomerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly TransactionService _transactionService;

        public AccountsController(AccountService accountService, TransactionService transactionService)
        {
            _accountService = accountService;
            _transactionService = transactionService;
        }

        // Get accounts for logged-in user
        [Authorize]
        [HttpGet("my-accounts")]
        public async Task<IActionResult> GetMyAccounts()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized("Invalid User ID");

            var accounts = await _accountService.GetAccountsByUserAsync(userId);

            var accountDtos = accounts.Select(a => new AccountDto
            {
                AccountId = a.AccountId,
                BranchId = a.BranchId,
                AccountNumber = a.AccountNumber!,
                AccountType = a.AccountType!,
                CurrencyCode = a.CurrencyCode!,
                Balance = a.Balance,
                IsMinor = a.IsMinor,
                PowerOfAttorneyUserId = a.PowerOfAttorneyUserId,
                IsClosed = a.IsClosed,
                CreatedDate = a.CreatedDate,
                BranchName = a.Branch?.BranchName,
                BankName = a.Branch?.Bank?.BankName
            }).ToList();

            return Ok(accountDtos);
        }

        // Deposit
        [Authorize]
        [HttpPost("{accountId}/deposit")]
        public async Task<IActionResult> Deposit(int accountId, [FromBody] TransactionDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            int userId = int.Parse(userIdClaim.Value);

            var success = await _accountService.DepositAsync(accountId, dto.Amount);
            if (!success) return BadRequest("Deposit failed.");

            // Log transaction
            await _transactionService.DepositAsync(accountId, dto.Amount, userId);

            return Ok(new { Message = "Deposit successful", accountId, dto.Amount });
        }

        // Withdraw
        [Authorize]
        [HttpPost("{accountId}/withdraw")]
        public async Task<IActionResult> Withdraw(int accountId, [FromBody] TransactionDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            int userId = int.Parse(userIdClaim.Value);

            var success = await _accountService.WithdrawAsync(accountId, dto.Amount);
            if (!success) return BadRequest("Withdrawal failed. Check balance.");

            // Log transaction
            await _transactionService.WithdrawAsync(accountId, dto.Amount, userId);

            return Ok(new { Message = "Withdrawal successful", accountId, dto.Amount });
        }

        // Transfer
        [Authorize]
        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst(JwtRegisteredClaimNames.Sub);
            int userId = int.Parse(userIdClaim.Value);

            var success = await _accountService.TransferAsync(dto.FromAccountId, dto.ToAccountId, dto.Amount);
            if (!success) return BadRequest("Transfer failed. Check balances or account IDs.");

            // Log transaction (one transaction per transfer)
            await _transactionService.TransferAsync(dto.FromAccountId, dto.ToAccountId, dto.Amount, userId);

            return Ok(new { Message = "Transfer successful", dto.FromAccountId, dto.ToAccountId, dto.Amount });
        }
    }
}
