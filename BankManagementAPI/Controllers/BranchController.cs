using BankCustomerAPI.DTO;
using BankCustomerAPI.Models;
using BankCustomerAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankCustomerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly BranchService _branchService;

        public BranchController(BranchService branchService)
        {
            _branchService = branchService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetBranches()
        {
            var branches = await _branchService.GetAllBranchesAsync();

            var branchDtos = branches.Select(b => new CreateBranchDto
            {
                BankId = b.BankId,
                BranchName = b.BranchName,
                BranchCode = b.BranchCode,
                Address = b.Address,
                ManagerUserId = b.ManagerUserId
            }).ToList();

            return Ok(branchDtos);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetBranch(int id)
        {
            var branch = await _branchService.GetBranchByIdAsync(id);
            if (branch == null) return NotFound();

            var branchDto = new CreateBranchDto
            {
                BankId = branch.BankId,
                BranchName = branch.BranchName,
                BranchCode = branch.BranchCode,
                Address = branch.Address,
                ManagerUserId = branch.ManagerUserId
            };

            return Ok(branchDto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBranch([FromBody] CreateBranchDto dto)
        {
            var branch = new Branch
            {
                BankId = dto.BankId,
                BranchName = dto.BranchName,
                BranchCode = dto.BranchCode,
                Address = dto.Address,
                ManagerUserId = dto.ManagerUserId,
                CreatedDate = DateTime.UtcNow
            };

            var createdBranch = await _branchService.CreateBranchAsync(branch);

            var branchDto = new CreateBranchDto
            {
                BankId = createdBranch.BankId,
                BranchName = createdBranch.BranchName,
                BranchCode = createdBranch.BranchCode,
                Address = createdBranch.Address,
                ManagerUserId = createdBranch.ManagerUserId
            };

            return Ok(branchDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBranch(int id, [FromBody] CreateBranchDto dto)
        {
            var branch = await _branchService.GetBranchByIdAsync(id);
            if (branch == null) return NotFound();

            branch.BankId = dto.BankId;
            branch.BranchName = dto.BranchName;
            branch.BranchCode = dto.BranchCode;
            branch.Address = dto.Address;
            branch.ManagerUserId = dto.ManagerUserId;

            var updated = await _branchService.UpdateBranchAsync(branch);
            if (!updated) return StatusCode(500, "Error updating branch.");

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
      
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var result = await _branchService.SoftDeleteBranchAsync(id);
            if (!result) return NotFound();

            return Ok("Branch soft-deleted successfully.");
        }

    }
}
