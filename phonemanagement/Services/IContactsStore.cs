using PhoneBookApp.Models;

namespace phonemanagement.Services;

// Abstraction gia CRUD Contacts (ylopoihsh: SqlContactsStore)
public interface IContactsStore
{
    Task<List<Contact>> GetAllAsync();
    Task<Contact?> GetByIdAsync(int id);
    Task<List<Contact>> SearchAsync(string query);
    Task<Contact> AddAsync(Contact contact);
    Task<bool> UpdateAsync(Contact contact);
    Task<bool> DeleteAsync(int id);
}
