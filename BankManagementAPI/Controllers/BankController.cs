using BankCustomerAPI.DTO;
using BankCustomerAPI.Models;
using BankCustomerAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankCustomerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankController : ControllerBase
    {
        private readonly BankService _bankService;

        public BankController(BankService bankService)
        {
            _bankService = bankService;
        }

        [HttpGet]
        [Authorize] // any authenticated user
        public async Task<IActionResult> GetBanks()
        {
            var banks = await _bankService.GetAllBanksAsync();

            var bankDtos = banks.Select(b => new
            {
                BankId = b.BankId,
                BankName = b.BankName,
                HeadOfficeAddress = b.HeadOfficeAddress,
                IFSCCode = b.IFSCCode,
                CreatedDate = b.CreatedDate,
                Branches = b.Branches?.Select(br => new CreateBranchDto
                {
                    BankId = br.BankId,
                    BranchName = br.BranchName,
                    BranchCode = br.BranchCode,
                    Address = br.Address,
                    ManagerUserId = br.ManagerUserId
                }).ToList() ?? new List<CreateBranchDto>()
            }).ToList();

            return Ok(bankDtos);
        }


        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetBank(int id)
        {
            var bank = await _bankService.GetBankByIdAsync(id);
            if (bank == null)
                return NotFound();

            var dto = new BankDto
            {
                BankId = bank.BankId,
                BankName = bank.BankName,
                HeadOfficeAddress = bank.HeadOfficeAddress,
                IfscCode = bank.IFSCCode,
                Branches = bank.Branches?.Select(br => new BranchDto
                {
                    BranchId = br.BranchId,
                    BranchName = br.BranchName,
                    BranchCode = br.BranchCode
                }).ToList() ?? new List<BranchDto>()
            };

            return Ok(dto);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBank([FromBody] Bank bank)
        {
            bank.CreatedDate = DateTime.UtcNow;
            var createdBank = await _bankService.CreateBankAsync(bank);

            var bankDto = new
            {
                BankId = createdBank.BankId,
                BankName = createdBank.BankName,
                HeadOfficeAddress = createdBank.HeadOfficeAddress,
                IFSCCode = createdBank.IFSCCode,
                CreatedDate = createdBank.CreatedDate
            };

            return Ok(bankDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBank(int id, [FromBody] Bank bank)
        {
            if (id != bank.BankId) return BadRequest();

            var updated = await _bankService.UpdateBankAsync(bank);
            if (!updated) return NotFound();

            var bankDto = new
            {
                BankId = bank.BankId,
                BankName = bank.BankName,
                HeadOfficeAddress = bank.HeadOfficeAddress,
                IFSCCode = bank.IFSCCode,
                CreatedDate = bank.CreatedDate
            };

            return Ok(bankDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> DeleteBank(int id)
        {
            var result = await _bankService.SoftDeleteBankAsync(id);
            if (!result) return NotFound();

            return Ok("Bank soft-deleted successfully.");
        }

    }
}
