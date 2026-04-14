using PhoneBookApp.Models; // fernei to Contact model

namespace phonemanagement.Services; // namespace gia services layer

public interface IContactsStore // contract pou prepei na ylopoioun ola ta stores (SQL, klt)
{
    Task<List<Contact>> GetAllAsync(); // pare ola ta contacts
    Task<Contact?> GetByIdAsync(int id); // pare 1 contact me id (h null)
    Task<List<Contact>> SearchAsync(string query); // search se contacts (name/phone/email)
    Task<Contact> AddAsync(Contact contact); // insert neou contact
    Task<bool> UpdateAsync(Contact contact); // update yparxontos contact
    Task<bool> DeleteAsync(int id); // delete yparxontos contact
}

