using WebPortalAPI.Models;
using WebPortalAPI.DTOs; // Make sure DTOs are under this namespace
using Microsoft.EntityFrameworkCore;

namespace WebPortalAPI.Services;

public class FeeTitleService
{
    private readonly PmfdatabaseContext _context;

    public FeeTitleService(PmfdatabaseContext context)
    {
        _context = context;
    }

    // GET ALL
    public List<FeeTitleDTO> GetAllFeeTitles()
    {
        return _context.FeeTitles
            .Select(f => new FeeTitleDTO
            {
                FeeTitleId = f.FeeTitleId,
                Title = f.Title,
                Amount = f.Amount,
                HasExpiry = f.HasExpiry,
                ExpiryDate = f.ExpiryDate
            })
            .ToList();
    }

    // CREATE
    public async Task<FeeTitleDTO> CreateFeeTitleAsync(FeeTitleDTO dto)
    {
        var entity = new FeeTitle
        {
            Title = dto.Title,
            Amount = dto.Amount,
            HasExpiry = dto.HasExpiry,
            ExpiryDate = dto.ExpiryDate
        };

        _context.FeeTitles.Add(entity);
        await _context.SaveChangesAsync();

        dto.FeeTitleId = entity.FeeTitleId;
        return dto;
    }

    // UPDATE
    public async Task<bool> UpdateFeeTitleAsync(int id, FeeTitleDTO dto)
    {
        var entity = await _context.FeeTitles.FindAsync(id);
        if (entity == null)
            return false;

        entity.Title = dto.Title;
        entity.Amount = dto.Amount;
        entity.HasExpiry = dto.HasExpiry;
        entity.ExpiryDate = dto.ExpiryDate;

        _context.FeeTitles.Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // DELETE
    public async Task<bool> DeleteFeeTitleAsync(int id)
    {
        var entity = await _context.FeeTitles.FindAsync(id);
        if (entity == null)
            return false;

        _context.FeeTitles.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    // GET BY ID
    public async Task<FeeTitleDTO?> GetFeeTitleByIdAsync(int id)
    {
        var entity = await _context.FeeTitles.FindAsync(id);
        if (entity == null)
            return null;

        return new FeeTitleDTO
        {
            FeeTitleId = entity.FeeTitleId,
            Title = entity.Title,
            Amount = entity.Amount,
            HasExpiry = entity.HasExpiry,
            ExpiryDate = entity.ExpiryDate
        };
    }
}
