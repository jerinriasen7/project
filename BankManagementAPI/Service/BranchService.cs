using BankCustomerAPI.Data;
using BankCustomerAPI.Models;
using Microsoft.EntityFrameworkCore;
public class BranchService
{
    private readonly BankingDbContext _context;

    public BranchService(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Branch>> GetAllBranchesAsync()
    {
        return await _context.Branches
            .Include(b => b.Bank)
            .ToListAsync();
    }

    public async Task<Branch?> GetBranchByIdAsync(int branchId)
    {
        return await _context.Branches
            .Include(b => b.Bank)
            .FirstOrDefaultAsync(b => b.BranchId == branchId);
    }

    public async Task<Branch> CreateBranchAsync(Branch branch)
    {
        branch.CreatedDate = DateTime.UtcNow;
        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();
        return branch;
    }

    public async Task<bool> UpdateBranchAsync(Branch branch)
    {
        var existing = await _context.Branches.FindAsync(branch.BranchId);
        if (existing == null) return false;

        existing.BranchName = branch.BranchName;
        existing.BranchCode = branch.BranchCode;
        existing.Address = branch.Address;
        existing.ManagerUserId = branch.ManagerUserId;
        existing.BankId = branch.BankId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteBranchAsync(int branchId)
    {
        var branch = await _context.Branches
            .Include(b => b.Accounts)
            .FirstOrDefaultAsync(b => b.BranchId == branchId);

        if (branch == null) return false;

        branch.IsDeleted = true;

        await _context.SaveChangesAsync();
        return true;
    }
}
