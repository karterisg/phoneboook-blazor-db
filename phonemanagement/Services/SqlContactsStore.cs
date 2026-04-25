using Microsoft.EntityFrameworkCore; // EF Core APIs (AsNoTracking, ToListAsync, Like, klt)
using PhoneBookApp.Models; // Contact model
using phonemanagement.Data; // AppDbContext

namespace phonemanagement.Services; // namespace gia services layer (store)

public sealed class SqlContactsStore : IContactsStore // IContactsStore implementation pou doulevei me SQL/EF
{
    private readonly AppDbContext _db; // DbContext gia provasi stin SQL vasi

    public SqlContactsStore(AppDbContext db) // constructor injection tou DbContext
    {
        _db = db; // apothikeuei to DbContext sto field
    }

    public Task<List<Contact>> GetAllAsync() => // fernei ola ta contacts
        _db.Contacts.AsNoTracking().OrderBy(c => c.Id).ToListAsync(); // SELECT * ORDER BY Id (read-only)

    public Task<Contact?> GetByIdAsync(int id) => // fernei 1 contact me id
        _db.Contacts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id); // SELECT TOP 1 WHERE Id=id

    public Task<List<Contact>> SearchAsync(string query) // search se name/phone/email
    {
        query = query?.Trim() ?? ""; // katharizei null + trim
        if (query.Length == 0) return GetAllAsync(); // an den exei query, epistrefei ola

        return _db.Contacts.AsNoTracking() // read-only query
            .Where(c =>
                (c.Name != null && EF.Functions.Like(c.Name, $"%{query}%")) || // LIKE se Name
                (c.Phone != null && EF.Functions.Like(c.Phone, $"%{query}%")) || // LIKE se Phone
                (c.Email != null && EF.Functions.Like(c.Email, $"%{query}%"))) // LIKE se Email
            .OrderBy(c => c.Id) // stable ordering
            .ToListAsync(); // EXEC query kai fernei lista
    }

    public async Task<Contact> AddAsync(Contact contact) // insert neou contact
    {
        contact.Id = 0; // identity column tha valei to Id (to 0 agnoeitai)
        if (contact.CreatedAtUtc == default)
            contact.CreatedAtUtc = DateTime.UtcNow;
        _db.Contacts.Add(contact); // mark entity as Added
        await _db.SaveChangesAsync(); // INSERT stin SQL
        return contact; // epistrefei to contact me gemato Id
    }

    public async Task<bool> UpdateAsync(Contact contact) // update existing contact
    {
        var existing = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == contact.Id); // fetch tracked entity
        if (existing is null) return false; // an den vrethei, den kanei update

        existing.Name = contact.Name; // update name
        existing.Phone = contact.Phone; // update phone
        existing.Email = contact.Email; // update email
        existing.Gender = contact.Gender; // update gender

        await _db.SaveChangesAsync(); // UPDATE stin SQL
        return true; // success
    }

    public async Task<bool> DeleteAsync(int id) // delete existing contact
    {
        var existing = await _db.Contacts.FirstOrDefaultAsync(c => c.Id == id); // fetch tracked entity
        if (existing is null) return false; // an den yparxei, den kanei delete

        _db.Contacts.Remove(existing); // mark entity as Deleted
        await _db.SaveChangesAsync(); // DELETE stin SQL
        return true; // success
    }
}

