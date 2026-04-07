 using PhoneBookApp.Models;

namespace phonemanagement.Services;

public sealed class InMemoryContactsStore : IContactsStore
{
    public Task<List<Contact>> GetAllAsync() =>
        Task.FromResult(ContactsRepository.GetContacts());

    public Task<Contact?> GetByIdAsync(int id) =>
        Task.FromResult(ContactsRepository.GetContactById(id));

    public Task<List<Contact>> SearchAsync(string query) =>
        Task.FromResult(ContactsRepository.SearchContacts(query));

    public Task<Contact> AddAsync(Contact contact)
    {
        ContactsRepository.AddContact(contact);
        return Task.FromResult(contact);
    }

    public Task<bool> UpdateAsync(Contact contact)
    {
        var existing = ContactsRepository.GetContactById(contact.Id);
        if (existing is null) return Task.FromResult(false);
        ContactsRepository.UpdateContact(contact);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(int id)
    {
        var existing = ContactsRepository.GetContactById(id);
        if (existing is null) return Task.FromResult(false);
        ContactsRepository.DeleteContact(id);
        return Task.FromResult(true);
    }
}

