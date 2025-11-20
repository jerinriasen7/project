using BankCustomerAPI.Data;
using BankCustomerAPI.Models;
using Microsoft.EntityFrameworkCore;
public class BankService
{
    private readonly BankingDbContext _context;

    public BankService(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<List<Bank>> GetAllBanksAsync()
    {
        return await _context.Banks
            .Include(b => b.Branches)
            .ToListAsync();
    }

    public async Task<Bank?> GetBankByIdAsync(int bankId)
    {
        return await _context.Banks
            .Include(b => b.Branches)
            .FirstOrDefaultAsync(b => b.BankId == bankId);
    }

    public async Task<Bank> CreateBankAsync(Bank bank)
    {
        bank.CreatedDate = DateTime.UtcNow;
        _context.Banks.Add(bank);
        await _context.SaveChangesAsync();
        return bank;
    }

    public async Task<bool> UpdateBankAsync(Bank bank)
    {
        var existingBank = await _context.Banks.FindAsync(bank.BankId);
        if (existingBank == null) return false;

        existingBank.BankName = bank.BankName;
        existingBank.HeadOfficeAddress = bank.HeadOfficeAddress;
        existingBank.IFSCCode = bank.IFSCCode;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SoftDeleteBankAsync(int bankId)
    {
        var bank = await _context.Banks
            .Include(b => b.Branches)
            .FirstOrDefaultAsync(b => b.BankId == bankId);

        if (bank == null) return false;

        bank.IsDeleted = true;

        foreach (var branch in bank.Branches)
        {
            branch.IsDeleted = true;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
