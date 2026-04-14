using Microsoft.EntityFrameworkCore;
using PhoneBookApp.Models;
using phonemanagement.Data;

namespace phonemanagement.Services;

public sealed class SqlContactsStore : IContactsStore
{
    private readonly AppDbContext _db;

    public SqlContactsStore(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Contact>> GetAllAsync() =>
        _db.Contacts.AsNoTracking().OrderBy(c => c.Id).ToListAsync();

    public Task<Contact?> GetByIdAsync(int id) =>
        _db.Contacts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

    public Task<List<Contact>> SearchAsync(string query)
    {
        query = query?.Trim() ?? "";
        if (query.Length == 0) return GetAllAsync();

        return _db.Contacts.AsNoTracking()
            .Where(c =>
                (c.Name != null && EF.Functions.Like(c.Name, $"%{query}%")) ||
                (c.Phone != null && EF.Functions.Like(c.Phone, $"%{query}%")) ||
                (c.Email != null && EF.Functions.Like(c.Email, $"%{query}%")))
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<Contact> AddAsync(Contact contact)
    {
        contact.Id = 0; // ensure identity generates it
        _db.Contacts.Add(contact);
        await _db.SaveChangesAsync();
        return contact;
    }

    public async Task<bool> UpdateAsync(Contact contact)
    {
        var existing = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == contact.Id);
        if (existing is null) return false;

        existing.Name = contact.Name;
        existing.Phone = contact.Phone;
        existing.Email = contact.Email;
        existing.Gender = contact.Gender;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null) return false;

        _db.Contacts.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}

