# Phonebook (Blazor Server + SQL Server + EF Core)

Κατάλογος επαφών / τηλεφωνικός κατάλογος με Blazor Server, REST API, JWT σύνδεση και αποθήκευση σε SQL Server μέσω Entity Framework Core. Ο **κύριος κατάλογος** που βλέπουν οι χρήστες προέρχεται από **`/api/directory`** (άλλοι χρήστες + κοινές επαφές)· ο πίνακας **`Contacts`** χρησιμοποιείται και για συγχρονισμό με χρήστες και για τις κοινές κάρτες καταλόγου.

**Repository:** [https://github.com/karterisg/phoneboook-blazor-db](https://github.com/karterisg/phoneboook-blazor-db)

```bash
git clone https://github.com/karterisg/phoneboook-blazor-db.git
cd phoneboook-blazor-db
```

---

## 1) Σκοπός του project

Το project είναι ένα **Phonebook** που:

- Έχει **Web UI** (Blazor Server / Razor Components).
- **Εκθέτει REST API** (επαφές, κατάλογος, auth, χρήστες, tasks) και Swagger (`/swagger`).
- **Αποθηκεύει δεδομένα** σε **SQL Server** (π.χ. LocalDB) μέσω **EF Core**.
- Χρησιμοποιεί **JWT** για προστατευμένα endpoints· οι σελίδες UI περνάνε token μέσω `ApiClient`.

Ο στόχος παραμένει να δουλεύουν όλα με **πραγματική** βάση, όχι in-memory δεδομένα.

---

## 2) Δομή φακέλων / βασικά αρχεία

**Solution**

- `phonemanagement.sln`

**Project**

- `phonemanagement/phonemanagement.csproj` (net9.0)

**Κώδικας**

- `phonemanagement/Program.cs` — DI, EF Core, JWT, minimal API (`/api/contacts`, `/api/directory`, `/api/auth`, `/api/users`, `/api/tasks`, …), εκκίνηση Blazor. Στο startup: **`ContactSchemaBootstrap`** (idempotent στήλες στο `Contacts` αν λείπουν), **`MigrateAsync`**, **`DbSeeder`**.

**Models**

- `phonemanagement/Models/Contact.cs` — επαφή SQL: Id, Name, Phone, Email, Gender, IsUserContribution, DirectoryListingId, CreatedAtUtc.
- `phonemanagement/Models/AppUser.cs` — χρήστες (email, όνομα, τηλέφωνο, ρόλος, κ.λπ.).
- `phonemanagement/Models/TaskItem.cs` — tasks ανά χρήστη.

**Data (EF Core)**

- `phonemanagement/Data/AppDbContext.cs` — `Contacts`, `Users`, `Tasks`.
- `phonemanagement/Data/DbSeeder.cs` — admin + δοκιμαστικοί χρήστες.
- `phonemanagement/Data/ContactSchemaBootstrap.cs` — ADO.NET βήμα πριν το migrate ώστε να υπάρχουν οι στήλες `Contacts` που περιμένει το μοντέλο.
- `phonemanagement/Migrations/*` — ιστορικό schema (αρχικό `Contacts`, auth/tasks, ρόλοι, πεδία directory, κοινές επαφές, `CreatedAtUtc`, κ.λπ.).

**Services**

- `phonemanagement/Services/IContactsStore.cs` / `SqlContactsStore.cs` — CRUD στον πίνακα `Contacts`.
- `phonemanagement/Services/ApiClient.cs`, `ClientAuthState.cs` — κλήσεις API με JWT από το Blazor UI.

**UI (Blazor)**

- `phonemanagement/Components/Pages/Contacts.razor` — λίστα από **`GET /api/directory`** (όχι απευθείας `IContactsStore`).
- `phonemanagement/Components/Pages/Home.razor` — στατιστικά / πρόσφατα από **`/api/directory`** όταν ο χρήστης είναι συνδεδεμένος.
- `phonemanagement/Components/Pages/AddContact.razor` — **`POST /api/contacts`** (κοινή επαφή, ορατή σε όλους στο directory).
- `phonemanagement/Components/Pages/View.razor`, `EditContact.razor` — επαφές `Contacts` (int id) μέσω `IContactsStore` όπου ισχύει.
- `phonemanagement/Components/Pages/ViewUser.razor`, `Users.razor`, `Tasks.razor`, `Login.razor`, κ.λπ.

**Config**

- `phonemanagement/appsettings.json` — `ConnectionStrings:DefaultConnection` (π.χ. LocalDB), `Jwt`.
- `phonemanagement/appsettings.Development.json` — παρόμοια, συχνά με μεγαλύτερο token lifetime για dev.

**Run profile**

- `phonemanagement/Properties/launchSettings.json` — `applicationUrl` (ports).

---

## 3) Κατάλογος (`/api/directory`) vs πίνακας `Contacts`

- **`GET /api/directory`** (με JWT): επιστρέφει **άλλους μη-admin χρήστες** (εκτός του τρέχοντος) **και** γραμμές από `Contacts` με `IsUserContribution = true` (κοινές επαφές). Αυτό είναι το **phone directory** στο UI (`/contacts`, Home).
- **`GET /api/contacts`** (λίστα / id / search): **μόνο Admin** — raw περιεχόμενο πίνακα `Contacts` (χρήσιμο για διαχείριση / Swagger).
- **`POST /api/contacts`**: **οποιοσδήποτε συνδεδεμένος** — δημιουργεί κοινή κάρτα (`IsUserContribution`, νέο `DirectoryListingId`).

---

## 4) Πώς συνδέεται το UI με τη βάση

- Οι σελίδες **καταλόγου** χρησιμοποιούν **`ApiClient`** + **`/api/directory`** (και auth).
- Οι σελίδες **επεξεργασίας legacy επαφής** (int id) μπορούν να χρησιμοποιούν **`IContactsStore`** (SQL μέσω `AppDbContext`).
- Το **`SqlContactsStore`** κάνει CRUD στο `DbSet<Contact>`.

---

## 5) Πώς δουλεύει ο SQL store (`SqlContactsStore.cs`)

- **GetAllAsync:** `Contacts` read-only, ταξινόμηση κατά Id.
- **AddAsync:** insert· αν λείπει `CreatedAtUtc`, ορίζεται `DateTime.UtcNow`.
- **UpdateAsync / DeleteAsync:** ενημέρωση / διαγραφή με Id.

---

## 6) Migrations / σχήμα `Contacts`

Μετά το αρχικό migration, έχουν προστεθεί μεταξύ άλλων: πίνακες `Users` / `Tasks`, ρόλοι, πεδία directory, **`IsUserContribution`**, **`DirectoryListingId`** (μοναδικό filtered index), **`CreatedAtUtc`**.

Ενδεικτικά migrations στο φάκελο:

- `20260414084441_InitialCreate` — πίνακας `Contacts` (βασικά πεδία).
- `20260420102205_AddAuthAndTasks` — `Users`, `Tasks`.
- `20260421120000_AddUserRole`, `20260421124500_AddUserDirectoryFields`
- `20260425120000_AddSharedDirectoryContacts` — στήλες κοινών επαφών στο `Contacts`.
- `20260425140000_AddContactCreatedAtUtc` — `CreatedAtUtc`.

Στο **startup** καλείται πρώτα το **`ContactSchemaBootstrap`** (για παλιές βάσεις που δεν είχαν εφαρμοστεί σωστά migrations) και μετά **`Database.MigrateAsync()`**.

Από τη **ρίζα** solution (αν υπάρχει `dotnet-tools.json`):

```bash
dotnet tool restore
cd phonemanagement
dotnet tool run dotnet-ef database update
```

(Σε κανονική εκτέλεση η εφαρμογή καλεί ήδη `MigrateAsync()`· το `database update` είναι χρήσιμο για CI ή χειροκίνητο συγχρονισμό.)

---

## 7) Seed data (`DbSeeder.cs`)

- Δημιουργεί **admin** αν λείπει: email **`admin@test.com`**, κωδικός **`admin`** (ή σύνδεση με identifier **`admin`** στο login — χαρτογραφείται στο ίδιο email).
- Αν υπάρχει παλιός λογαριασμός **`admin@admin.com`**, στο seed μεταφέρεται σε **`admin@test.com`** με κωδικό **`admin`** και ενημερώνεται το αντίστοιχο `Contacts.Email` αν υπάρχει.
- Προστίθενται **δοκιμαστικοί χρήστες** (π.χ. Γιώργος, Μαρία, …) με κωδικό **`123456`** αν δεν υπάρχουν ήδη.

---

## 8) API (επιλεγμένα endpoints)

**Επαφές (`/api/contacts`)** — απαιτείται JWT· GET λίστα/id/search **AdminOnly**· POST **οποιοσδήποτε συνδεδεμένος** (κοινή επαφή)· PUT/DELETE **AdminOnly**.

**Κατάλογος (`/api/directory`)** — JWT· `GET /` λίστα, `GET /{guid}` λεπτομέρεια.

**Auth:** `POST /api/auth/register`, `POST /api/auth/login`.

**Χρήστες / tasks / profile:** `/api/users` (admin), `/api/tasks`, `/api/me`, κ.λπ.

**Swagger:** `/swagger` — ορισμένα endpoints θέλουν επικεφαλίδα `Authorization: Bearer <token>`.

**Παράδειγμα JSON για `POST /api/contacts`:**

```json
{
  "name": "Giorgos",
  "phone": "6912345678",
  "email": "giorgos@mail.com",
  "gender": "Male"
}
```

Ο server ορίζει `IsUserContribution`, `DirectoryListingId`, `CreatedAtUtc`· το `id` το δίνει η βάση.

---

## 9) UI (`Contacts.razor` και Home)

- **Contacts:** φόρτωση από **`/api/directory`**, αναζήτηση/ταξινόμηση client-side, σύνδεση με προβολή χρήστη (Guid) ή κοινής επαφής (`/view-contact/{int}`). Admin: διαχείριση χρηστών / κοινών επαφών όπου ορίζεται στο UI.
- **Home:** αν είσαι συνδεδεμένος, τα νούμερα και οι «πρόσφατες» προέρχονται από το **ίδιο** directory API.

---

## 10) Ports / «address already in use»

Αν το port είναι πιασμένο, δες `Properties/launchSettings.json` — π.χ.:

- `http://localhost:5055`
- `https://localhost:7247;http://localhost:5055` (profile `https`)

Άλλαξε `applicationUrl` αν χρειάζεται.

---

## 11) Τι χρειάζεται για να τρέξει σε άλλο PC

**Prereqs**

- .NET **SDK 9.0**
- **SQL Server** (LocalDB, Express `.\SQLEXPRESS`, κ.λπ.) — προαιρετικά SSMS

**Βήματα**

1. `git clone` και `cd` στο repo.
2. `dotnet tool restore` (από τη ρίζα, αν χρησιμοποιείς `dotnet-ef` από `dotnet-tools.json`).
3. `dotnet restore`
4. **`ConnectionStrings:DefaultConnection`** στο `phonemanagement/appsettings.json` ή `appsettings.Development.json` ώστε να ταιριάζει με το SQL instance και το όνομα βάσης (π.χ. `PhonebookDb` σε LocalDB).

   Παράδειγμα LocalDB:

   `Server=(localdb)\MSSQLLocalDB;Database=PhonebookDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=Optional`

5. **`Jwt:SigningKey`** τουλάχιστον **32 χαρακτήρες** (υπάρχει προεπιλογή στα appsettings για dev).
6. `cd phonemanagement` και `dotnet run` — migrations + seed τρέχουν στην εκκίνηση. Swagger: `/swagger`.

---

## 12) Σύνδεση αυτού του folder με το GitHub repo

```bash
cd /path/to/phone
git remote remove origin
git remote add origin https://github.com/karterisg/phoneboook-blazor-db.git
git add .
git commit -m "Your message"
git branch -M main
git push -u origin main
```

Για **HTTPS** με token, στο password prompt χρησιμοποίησε **Personal Access Token**.
