using Google.Cloud.Firestore;
using PhoneBookApp.Models;

namespace phonemanagement.Services;

public sealed class FirestoreContactsStore : IContactsStore
{
    private readonly CollectionReference _contacts;

    public FirestoreContactsStore(FirestoreDb db)
    {
        _contacts = db.Collection("contacts");
    }

    public async Task<List<Contact>> GetAllAsync()
    {
        var snap = await _contacts.GetSnapshotAsync();
        return snap.Documents
            .Select(d => MapFromDoc(d))
            .OrderBy(c => c.Id)
            .ToList();
    }

    public async Task<Contact?> GetByIdAsync(int id)
    {
        var doc = await _contacts.Document(id.ToString()).GetSnapshotAsync();
        return doc.Exists ? MapFromDoc(doc) : null;
    }

    public async Task<List<Contact>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return await GetAllAsync();

        // Simple client-side filtering to avoid requiring composite indexes.
        var all = await GetAllAsync();
        return all.Where(c =>
                (!string.IsNullOrWhiteSpace(c.Name) && c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(c.Phone) && c.Phone.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(c.Email) && c.Email.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task<Contact> AddAsync(Contact contact)
    {
        var id = await NextIdAsync();
        contact.Id = id;

        await _contacts.Document(id.ToString()).SetAsync(new Dictionary<string, object?>
        {
            ["id"] = contact.Id,
            ["name"] = contact.Name,
            ["phone"] = contact.Phone,
            ["email"] = contact.Email,
            ["gender"] = contact.Gender
        });

        return contact;
    }

    public async Task<bool> UpdateAsync(Contact contact)
    {
        var docRef = _contacts.Document(contact.Id.ToString());
        var existing = await docRef.GetSnapshotAsync();
        if (!existing.Exists) return false;

        await docRef.SetAsync(new Dictionary<string, object?>
        {
            ["id"] = contact.Id,
            ["name"] = contact.Name,
            ["phone"] = contact.Phone,
            ["email"] = contact.Email,
            ["gender"] = contact.Gender
        }, SetOptions.MergeAll);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var docRef = _contacts.Document(id.ToString());
        var existing = await docRef.GetSnapshotAsync();
        if (!existing.Exists) return false;

        await docRef.DeleteAsync();
        return true;
    }

    private async Task<int> NextIdAsync()
    {
        // Generates a numeric Id without needing a server-side counter.
        // Works fine for small apps; for high concurrency you'd use a transaction/counter doc.
        var snap = await _contacts.GetSnapshotAsync();
        var max = 0;
        foreach (var d in snap.Documents)
        {
            if (int.TryParse(d.Id, out var id) && id > max) max = id;
        }
        return max + 1;
    }

    private static Contact MapFromDoc(DocumentSnapshot doc)
    {
        var dict = doc.ToDictionary();
        var id = dict.TryGetValue("id", out var idObj) && idObj is long l ? (int)l
            : int.TryParse(doc.Id, out var parsed) ? parsed
            : 0;

        return new Contact
        {
            Id = id,
            Name = dict.TryGetValue("name", out var name) ? name as string : null,
            Phone = dict.TryGetValue("phone", out var phone) ? phone as string : null,
            Email = dict.TryGetValue("email", out var email) ? email as string : null,
            Gender = dict.TryGetValue("gender", out var gender) && gender is string g && !string.IsNullOrWhiteSpace(g)
                ? g
                : "Male"
        };
    }
}

