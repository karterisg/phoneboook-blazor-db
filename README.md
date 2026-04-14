
 Σκοπός του project
Το project είναι ένα Phonebook (κατάλογος επαφών) που:
- Έχει Web UI (Blazor Server / Razor Components).
- Εκθέτει REST API endpoints (/api/contacts) για testing από Postman.
- Αποθηκεύει δεδομένα σε SQL Server (SSMS) μέσω Entity Framework Core (EF Core).

Ο στόχος των αλλαγών που κάναμε ήταν να φύγει τελείως το hardcoded/in-memory data
και να δουλεύουν όλα με “πραγματική” βάση (SQL Server).


 Δομή φακέλων / βασικά αρχεία

Solution:
- phonemanagement.sln

Project:
- phonemanagement/phonemanagement.csproj  (net9.0 project)

Κώδικας:
- phonemanagement/Program.cs
  Το entry-point. Εδώ γίνεται:
  - register services (DI)
  - setup EF Core / SQL connection
  - mapping API endpoints
  - middleware (Cors/Swagger/Static assets)
  - εκκίνηση Blazor UI

Models:
- phonemanagement/Models/Contact.cs
  Το domain model της επαφής: Id, Name, Phone, Email, Gender.

Data (EF Core):
- phonemanagement/Data/AppDbContext.cs
  DbContext του EF: καθορίζει mapping του Contact σε SQL table “Contacts”.

- phonemanagement/Migrations/*
  EF migrations: τι schema changes έγιναν στη βάση.
  Το αρχικό migration δημιουργεί table Contacts.

- phonemanagement/Data/DbSeeder.cs
  Seed demo data στη SQL, μόνο όταν ο πίνακας είναι άδειος.
  (δηλαδή “insert την demo λίστα” στη βάση, όχι σε κώδικα)

Services:
- phonemanagement/Services/IContactsStore.cs
  Interface/abstraction για CRUD operations (GetAll, GetById, Search, Add, Update, Delete).

- phonemanagement/Services/SqlContactsStore.cs
  Υλοποίηση του IContactsStore που μιλάει με SQL μέσω EF Core.

UI (Blazor pages):
- phonemanagement/Components/Pages/Contacts.razor
  Listing σελίδα με search/sort και κουμπιά View/Edit/Delete/Add.

- phonemanagement/Components/Pages/AddContact.razor
  Form για εισαγωγή νέας επαφής (Name, Phone, Email, Gender).

- phonemanagement/Components/Pages/EditContact.razor
  Form για αλλαγή υπάρχουσας επαφής (Name, Phone, Email, Gender).

- phonemanagement/Components/Pages/View.razor
  Σελίδα προβολής λεπτομερειών (Name, Phone, Email, Gender).

Config:
- phonemanagement/appsettings.json
- phonemanagement/appsettings.Development.json
  Εδώ έχουμε ConnectionStrings:DefaultConnection για SQL Server.

Run profile:
- phonemanagement/Properties/launchSettings.json
  Καθορίζει URLs/ports που ακούει το app (π.χ. http://localhost:5053).



Τι είναι το Entity Framework (και πού “μπαίνει”)
---------------------------------------------------
Το EF Core δεν είναι “ένα property” που το βάζεις κάπου.
Μπαίνει με 2 τρόπους:

Α) Program.cs: Register DbContext + provider SQL Server
   builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

Β) AppDbContext.cs: Κλάση που κληρονομεί από DbContext και έχει DbSet<Contact>
   public DbSet<Contact> Contacts => Set<Contact>();

Το EF Core “μεταφράζει” Linq queries σε SQL και κάνει SaveChanges για INSERT/UPDATE/DELETE.




 Πώς συνδέεται το Web UI με τη βάση (η λογική του store)

Χρησιμοποιούμε abstraction:
  IContactsStore

Το UI δεν ξέρει “SQL” ούτε “EF”. Ξέρει μόνο:
  Store.GetAllAsync()
  Store.AddAsync(contact)
  Store.UpdateAsync(contact)
  Store.DeleteAsync(id)

Έτσι το UI είναι καθαρό και “μιλάει” με service.
Η SQL υλοποίηση είναι στο SqlContactsStore.cs και χρησιμοποιεί AppDbContext.


Πώς δουλεύει ο SQL store (SqlContactsStore.cs)

Παραδείγματα λογικής:
- GetAllAsync:
  _db.Contacts.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
  (AsNoTracking = πιο γρήγορο για read-only)

- AddAsync:
  contact.Id = 0 για να αφήσουμε το identity column να βάλει Id.
  _db.Contacts.Add(contact);
  await _db.SaveChangesAsync();

- UpdateAsync:
  βρίσκουμε το entity, αλλάζουμε properties, SaveChangesAsync.

- DeleteAsync:
  βρίσκουμε το entity, Remove, SaveChangesAsync.

- SearchAsync:
  κάνει LIKE σε Name/Phone/Email ώστε να βρίσκει μερική αντιστοίχιση.


Τι άλλαξε στη βάση (Migrations)
Το migration δημιουργεί table:
  Contacts
  - Id int IDENTITY PK
  - Name nvarchar(200) NULL
  - Phone nvarchar(50) NULL
  - Email nvarchar(200) NULL
  - Gender nvarchar(20) NOT NULL

Αυτό είναι η αλλαγή στη βάση: schema definition.

Για να το εφαρμόσουμε σε νέο PC:
  dotnet tool restore
  dotnet tool run dotnet-ef database update


Seed data (γιατί/πώς)

Επειδή θέλαμε να υπάρχει η demo λίστα (Γιώργος, Κώστας, κλπ) αλλά:
- ΟΧΙ μέσα στον κώδικα σαν static list,
- ΑΛΛΑ μέσα στη βάση,

βάλαμε DbSeeder.SeedAsync(db):
- ελέγχει αν υπάρχουν ήδη Contacts,
- αν είναι άδεια η βάση, κάνει insert τα demo contacts.

Στο Program.cs κατά το startup:
  await db.Database.MigrateAsync();   // εφαρμόζει migrations
  await DbSeeder.SeedAsync(db);       // seed αν είναι άδειο


API endpoints για Postman (/api/contacts)

Minimal API endpoints στο Program.cs:
- GET    /api/contacts
- GET    /api/contacts/{id}
- GET    /api/contacts/search?q=...
- POST   /api/contacts
- PUT    /api/contacts/{id}
- DELETE /api/contacts/{id}

Όλα χρησιμοποιούν IContactsStore, άρα τελικά γράφουν/διαβάζουν από SQL.

Παράδειγμα JSON για POST στο Postman:
{
  "name": "Giorgos",
  "phone": "6912345678",
  "email": "giorgos@mail.com",
  "gender": "Male"
}

Σημείωση: στο POST δεν βάζουμε id, το βάζει η βάση (IDENTITY).


9) UI λειτουργίες (Contacts.razor)
----------------------------------
Η λίστα φορτώνεται από SQL:
  contacts = await Store.GetAllAsync();

Search:
  contacts = await Store.SearchAsync(searchText);

Sort:
Υλοποιήσαμε ένα κουμπί ανά στήλη με toggle ↑/↓ (asc/desc) και ένδειξη στο UI.
Το sort γίνεται client-side πάνω στη λίστα που επέστρεψε η SQL.

Add/Edit/View/Delete:
Κάθε ενέργεια καλεί Store.* και μετά refresh/navigate.


10) Ports / “address already in use”
------------------------------------
Αν βγάλει error ότι το port είναι ήδη πιασμένο, σημαίνει ότι τρέχει ήδη instance.
Αλλάξαμε το port σε:
  http://localhost:5053
στο Properties/launchSettings.json.


11) Τι χρειάζεται για να τρέξει σε άλλο PC (summary)
----------------------------------------------------
Prereqs:
- .NET SDK (για net9.0)
- SQL Server (LocalDB ή full instance) + SSMS προαιρετικά

Steps:
1) git clone
2) dotnet tool restore
3) dotnet restore
4) ρύθμιση ConnectionStrings:DefaultConnection στο appsettings.Development.json
5) cd phonemanagement
6) dotnet tool run dotnet-ef database update
7) dotnet run


