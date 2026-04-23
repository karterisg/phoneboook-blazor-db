# Phonebook (Blazor Server + SQL Server + EF Core)

Κατάλογος επαφών με Blazor Server UI, REST API και αποθήκευση σε SQL Server μέσω Entity Framework Core.

**Repository:** [https://github.com/karterisg/phoneboook-blazor-db](https://github.com/karterisg/phoneboook-blazor-db)

```bash
git clone https://github.com/karterisg/phoneboook-blazor-db.git
cd phoneboook-blazor-db
```

---

## 1) Σκοπός του project

Το project είναι ένα **Phonebook** (κατάλογος επαφών) που:

- Έχει **Web UI** (Blazor Server / Razor Components).
- **Εκθέτει REST API endpoints** (`/api/contacts`) για testing από Postman.
- **Αποθηκεύει δεδομένα** σε **SQL Server** (SSMS) μέσω **Entity Framework Core (EF Core)**.

Ο στόχος των αλλαγών που έγιναν ήταν να φύγει τελείως το **hardcoded / in-memory** data και να δουλεύουν όλα με **“πραγματική”** βάση (SQL Server).

---

## 2) Δομή φακέλων / βασικά αρχεία

**Solution**

- `phonemanagement.sln`

**Project**

- `phonemanagement/phonemanagement.csproj` (net9.0)

**Κώδικας**

- `phonemanagement/Program.cs` — entry-point: register services (DI), EF Core / SQL, mapping API, middleware (CORS, Swagger, static), εκκίνηση Blazor UI.

**Models**

- `phonemanagement/Models/Contact.cs` — domain model: Id, Name, Phone, Email, Gender.

**Data (EF Core)**

- `phonemanagement/Data/AppDbContext.cs` — `DbContext`, mapping `Contact` → πίνακας `Contacts`.
- `phonemanagement/Migrations/*` — migrations / schema. Το αρχικό migration δημιουργεί πίνακα `Contacts`.
- `phonemanagement/Data/DbSeeder.cs` — seed demo data **μόνο** όταν ο πίνακας είναι άδειος (όχι static list στον κώδικα).

**Services**

- `phonemanagement/Services/IContactsStore.cs` — abstraction CRUD: GetAll, GetById, Search, Add, Update, Delete.
- `phonemanagement/Services/SqlContactsStore.cs` — υλοποίηση με SQL μέσω EF Core.

**UI (Blazor pages)**

- `phonemanagement/Components/Pages/Contacts.razor` — listing, search/sort, View / Edit / Delete / Add.
- `phonemanagement/Components/Pages/AddContact.razor` — φόρμα νέας επαφής.
- `phonemanagement/Components/Pages/EditContact.razor` — φόρμα επεξεργασίας.
- `phonemanagement/Components/Pages/View.razor` — λεπτομέρειες επαφής.

**Config**

- `phonemanagement/appsettings.json`
- `phonemanagement/appsettings.Development.json` — `ConnectionStrings:DefaultConnection` για SQL Server

**Run profile**

- `phonemanagement/Properties/launchSettings.json` — `applicationUrl` (ports που ακούει το app).

---

## 3) Πώς συνδέεται το Web UI με τη βάση (store)

Χρησιμοποιούμε το abstraction **`IContactsStore`**. Το UI **δεν** ξέρει λεπτομέρειες της βάσης· καλεί μόνο:

- `Store.GetAllAsync()`
- `Store.AddAsync(contact)`
- `Store.UpdateAsync(contact)`
- `Store.DeleteAsync(id)`

Η SQL υλοποίηση είναι στο **`SqlContactsStore.cs`** με **`AppDbContext`**.

---

## 4) Πώς δουλεύει ο SQL store (`SqlContactsStore.cs`)

Παραδείγματα λογικής:

- **GetAllAsync:** `_db.Contacts.AsNoTracking().OrderBy(c => c.Id).ToListAsync();` (AsNoTracking για read-only)
- **AddAsync:** `contact.Id = 0`, `_db.Contacts.Add(contact);`, `SaveChangesAsync`
- **UpdateAsync:** find entity, αλλαγή properties, `SaveChangesAsync`
- **DeleteAsync:** find, `Remove`, `SaveChangesAsync`
- **SearchAsync:** `LIKE` σε Name / Phone / Email (μερική αντιστοίχιση)

---

## 5) Τι αλλάζει στη βάση (Migrations)

Το migration δημιουργεί πίνακα **`Contacts`**:

| Στήλη   | Τύπος            |
|--------|------------------|
| Id     | int, IDENTITY PK |
| Name   | nvarchar(200) NULL |
| Phone  | nvarchar(50) NULL  |
| Email  | nvarchar(200) NULL |
| Gender | nvarchar(20) NOT NULL |

Εφαρμογή schema σε **νέο** PC (από **ρίζα** solution, υπάρχει `dotnet-tools.json`):

```bash
dotnet tool restore
cd phonemanagement
dotnet tool run dotnet-ef database update
```

---

## 6) Seed data

Ζητούμενο: demo λίστα (π.χ. Γιώργος, Κώστας, …) **όχι** ως static list στον κώδικα, αλλά **στη βάση**.

- `DbSeeder.SeedAsync(db)`: αν **δεν** υπάρχουν `Contacts`, κάνει insert τα demo.
- Στο `Program.cs` κατά το startup: `await db.Database.MigrateAsync();` και μετά `await DbSeeder.SeedAsync(db);`

---

## 7) API για Postman (`/api/contacts`)

Minimal API στο `Program.cs`:

- `GET    /api/contacts`
- `GET    /api/contacts/{id}`
- `GET    /api/contacts/search?q=...`
- `POST   /api/contacts`
- `PUT    /api/contacts/{id}`
- `DELETE /api/contacts/{id}`

Όλα χρησιμοποιούν **`IContactsStore`** (άρα read/write από SQL).

**Παράδειγμα JSON (POST / Postman):**

```json
{
  "name": "Giorgos",
  "phone": "6912345678",
  "email": "giorgos@mail.com",
  "gender": "Male"
}
```

Στο POST **δεν** στέλνουμε `id` — το βάζει η βάση (IDENTITY).

---

## 8) UI (`Contacts.razor`)

- Λίστα από SQL: `contacts = await Store.GetAllAsync();`
- **Search:** `contacts = await Store.SearchAsync(searchText);`
- **Sort:** κουμπιά ανά στήλη, toggle ↑/↓ (asc/desc)· το sort γίνεται **client-side** πάνω στη λίστα που επέστρεψε η SQL.
- **Add / Edit / View / Delete:** `Store.*` και μετά refresh / navigate.

---

## 9) Ports / «address already in use»

Αν το port είναι ήδη πιασμένο, τρέχει άλλο instance ή άλλη εφαρμογή.

Η **τρέχουσα** ρύθμιση (δες `Properties/launchSettings.json`) περιλαμβάνει URLs όπως:

- `http://localhost:5055`
- `https://localhost:7247;http://localhost:5055` (profile `https`)

Για άλλο port, άλλαξε `applicationUrl` εκεί.

---

## 10) Τι χρειάζεται για να τρέξει σε άλλο PC

**Prereqs**

- .NET **SDK 9.0** (σύμφωνα με `TargetFramework` του project)
- **SQL Server** (LocalDB, **Express** `.\SQLEXPRESS`, default instance, κ.λπ.) — προαιρετικά **SSMS**

**Βήματα**

1. `git clone https://github.com/karterisg/phoneboook-blazor-db.git` και `cd` στο φάκελο του repo.
2. `dotnet tool restore` (από τη ρίζα, για `dotnet-ef` από `dotnet-tools.json`)
3. `dotnet restore`
4. Ρύθμιση **`ConnectionStrings:DefaultConnection`** στο `phonemanagement/appsettings.Development.json` (ή User Secrets) ώστε να ταιριάζει με **το** SQL instance και **το** όνομα βάσης.

   Παράδειγμα (SQL Server Express, named instance, βάση `PhonebookDb`):

   `Server=localhost\SQLEXPRESS;Database=PhonebookDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=Optional`

5. `cd phonemanagement`
6. `dotnet tool run dotnet-ef database update`
7. `dotnet run` (από `phonemanagement`)  
   - Swagger: συνήθως `/swagger` στο ίδιο base URL.

---

## 11) Σύνδεση αυτού του folder με το GitHub repo

Αν ξεκινάς από **τοπικό** git repo και θες το **νέο** `origin` στο GitHub:

```bash
cd /path/to/phone
git remote remove origin
git remote add origin https://github.com/karterisg/phoneboook-blazor-db.git
git add .
git commit -m "Your message"
git branch -M main
git push -u origin main
```

Δημιούργησε πρώτα το **άδειο** repository στο GitHub (`phoneboook-blazor-db`) χωρίς README αν θέλεις “καθαρό” first push, ή ακολούθισε οδηγίες merge αν το GitHub σου έκανε ήδη initial commit.

Αν χρησιμοποιείς **token** (HTTPS), στο password prompt του Git βάζεις **Personal Access Token**, όχι τον κωδικό λογαριασμού.
