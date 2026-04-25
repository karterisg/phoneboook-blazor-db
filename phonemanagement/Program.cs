using PhoneBookApp.Models;
using phonemanagement.Components;
using phonemanagement.Data;
using phonemanagement.Dtos.Auth;
using phonemanagement.Dtos.Directory;
using phonemanagement.Dtos.Me;
using phonemanagement.Dtos.Tasks;
using phonemanagement.Dtos.Users;
using phonemanagement.Models;
using phonemanagement.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server + interactive UI
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<ClientAuthState>();
builder.Services.AddScoped<ApiClient>();

// CORS (dev / Postman)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Swagger + JWT sto header
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT access token here."
    };

    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(doc =>
    {
        var req = new OpenApiSecurityRequirement();
        req.Add(new OpenApiSecuritySchemeReference("Bearer", doc, null), new List<string>());
        return req;
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                  ?? throw new InvalidOperationException("Missing Jwt configuration section.");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(o => !string.IsNullOrWhiteSpace(o.SigningKey) && o.SigningKey.Length >= 32,
        "Jwt:SigningKey must be at least 32 characters.")
    .ValidateOnStart();

builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.PasswordHasher<phonemanagement.Models.AppUser>>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddScoped<IContactsStore, SqlContactsStore>();
    Console.WriteLine("Using SQL Server store");
}
else
{
    throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection. Configure SQL Server connection string.");
}

var app = builder.Build();

// Schema Contacts (palies vasis), migrations, seed
if (!string.IsNullOrWhiteSpace(connectionString))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.PasswordHasher<AppUser>>();
    await ContactSchemaBootstrap.EnsureExtendedColumnsAsync(db);
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, hasher);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.UseSwagger();
app.UseSwaggerUI();

// /api/contacts: pliris lista/search/id mono Admin - o katalogos xristwn einai /api/directory
var contactsApi = app.MapGroup("/api/contacts");

contactsApi.RequireAuthorization();

contactsApi.MapGet("/", async (IContactsStore store) =>
{
    var contacts = await store.GetAllAsync();
    return Results.Ok(contacts);
}).RequireAuthorization("AdminOnly");

contactsApi.MapGet("/{id:int}", async (int id, IContactsStore store) =>
{
    var contact = await store.GetByIdAsync(id);
    return contact is null ? Results.NotFound() : Results.Ok(contact);
}).RequireAuthorization("AdminOnly");

contactsApi.MapGet("/search", async (string q, IContactsStore store) =>
{
    var results = await store.SearchAsync(q);
    return Results.Ok(results);
}).RequireAuthorization("AdminOnly");

// POST: koini epafi (oloi oi logged-in) - emfanizetai sto directory
contactsApi.MapPost("/", async (Contact body, IContactsStore store) =>
{
    if (string.IsNullOrWhiteSpace(body.Name))
        return Results.BadRequest(new { message = "Name is required." });

    var contact = new Contact
    {
        Name = body.Name.Trim(),
        Phone = (body.Phone ?? "").Trim(),
        Email = (body.Email ?? "").Trim(),
        Gender = string.IsNullOrWhiteSpace(body.Gender) ? "Male" : body.Gender.Trim(),
        IsUserContribution = true,
        DirectoryListingId = Guid.NewGuid(),
        CreatedAtUtc = DateTime.UtcNow
    };

    var created = await store.AddAsync(contact);
    return Results.Created($"/api/contacts/{created.Id}", created);
});

contactsApi.MapPut("/{id:int}", async (int id, Contact contact, IContactsStore store) =>
{
    if (id != contact.Id)
        contact.Id = id;

    var ok = await store.UpdateAsync(contact);
    return ok ? Results.Ok(contact) : Results.NotFound();
}).RequireAuthorization("AdminOnly");

contactsApi.MapDelete("/{id:int}", async (int id, IContactsStore store) =>
{
    var ok = await store.DeleteAsync(id);
    return ok ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization("AdminOnly");

static Guid GetUserId(ClaimsPrincipal user)
{
    var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
    return Guid.TryParse(raw, out var id) ? id : throw new InvalidOperationException("Missing/invalid user id claim.");
}

var authApi = app.MapGroup("/api/auth");

authApi.MapPost("/register", async (
    RegisterRequest req,
    AppDbContext db,
    Microsoft.AspNetCore.Identity.PasswordHasher<AppUser> hasher,
    IJwtTokenService jwt) =>
{
    var email = (req.Email ?? "").Trim().ToLowerInvariant();
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
        return Results.BadRequest(new { message = "Invalid email/password." });

    var name = (req.Name ?? "").Trim();
    var phone = (req.Phone ?? "").Trim();
    var gender = string.IsNullOrWhiteSpace(req.Gender) ? "Male" : req.Gender.Trim();
    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
        return Results.BadRequest(new { message = "Name/phone are required." });

    var exists = await db.Users.AnyAsync(u => u.Email == email);
    if (exists)
        return Results.Conflict(new { message = "Email already registered." });

    var user = new AppUser
    {
        Email = email,
        Name = name,
        Phone = phone,
        Gender = gender,
        PasswordHash = "TEMP"
    };
    user.PasswordHash = hasher.HashPassword(user, req.Password);

    db.Users.Add(user);

    // Neo row Contacts gia xristi (mirroring)
    db.Contacts.Add(new Contact
    {
        Name = user.Name,
        Phone = user.Phone,
        Email = user.Email,
        Gender = user.Gender,
        CreatedAtUtc = DateTime.UtcNow
    });

    await db.SaveChangesAsync();

    var (token, expiresAtUtc) = jwt.CreateAccessToken(user);
    return Results.Ok(new AuthResponse(token, expiresAtUtc, user.Email));
});

authApi.MapPost("/login", async (
    LoginRequest req,
    AppDbContext db,
    Microsoft.AspNetCore.Identity.PasswordHasher<AppUser> hasher,
    IJwtTokenService jwt) =>
{
    var identifier = (req.Identifier ?? req.Email ?? "").Trim();
    var email = identifier.ToLowerInvariant();
    if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { message = "Invalid credentials." });

    // Login me "admin" -> admin@test.com (idio me DbSeeder)
    if (string.Equals(identifier, "admin", StringComparison.OrdinalIgnoreCase))
        email = "admin@test.com";

    var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
    if (user is null)
        return Results.Unauthorized();

    // Kwdikos <6 mono gia rolo Admin (px. "admin")
    if (req.Password.Length < 6 && !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { message = "Invalid credentials." });

    var verified = hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
    if (verified == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
        return Results.Unauthorized();

    var (token, expiresAtUtc) = jwt.CreateAccessToken(user);
    return Results.Ok(new AuthResponse(token, expiresAtUtc, user.Email));
});

var meApi = app.MapGroup("/api/me").RequireAuthorization();

meApi.MapGet("/", async (ClaimsPrincipal principal, AppDbContext db) =>
{
    var userId = GetUserId(principal);
    var me = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId);
    if (me is null)
        return Results.NotFound();

    return Results.Ok(new MeResponse(me.Id, me.Email, me.Name, me.Phone, me.Gender, me.Role, me.CreatedAtUtc));
});

meApi.MapPut("/", async (ClaimsPrincipal principal, MeUpdateRequest req, AppDbContext db) =>
{
    var userId = GetUserId(principal);
    var me = await db.Users.SingleOrDefaultAsync(u => u.Id == userId);
    if (me is null)
        return Results.NotFound();

    var email = (req.Email ?? "").Trim().ToLowerInvariant();
    var name = (req.Name ?? "").Trim();
    var phone = (req.Phone ?? "").Trim();
    var gender = string.IsNullOrWhiteSpace(req.Gender) ? "Male" : req.Gender.Trim();

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(phone))
        return Results.BadRequest(new { message = "Invalid fields." });

    // Monadiko email
    var emailTaken = await db.Users.AnyAsync(u => u.Email == email && u.Id != userId);
    if (emailTaken)
        return Results.Conflict(new { message = "Email already exists." });

    var oldEmail = me.Email;
    me.Email = email;
    me.Name = name;
    me.Phone = phone;
    me.Gender = gender;

    // Enimerwsh Contacts otan allazei email (profile / admin)
    var contact = await db.Contacts.SingleOrDefaultAsync(c => c.Email == oldEmail);
    if (contact is null)
    {
        db.Contacts.Add(new Contact
        {
            Name = me.Name,
            Phone = me.Phone,
            Email = me.Email,
            Gender = me.Gender,
            CreatedAtUtc = DateTime.UtcNow
        });
    }
    else
    {
        contact.Name = me.Name;
        contact.Phone = me.Phone;
        contact.Email = me.Email;
        contact.Gender = me.Gender;
    }

    await db.SaveChangesAsync();
    return Results.Ok(new MeResponse(me.Id, me.Email, me.Name, me.Phone, me.Gender, me.Role, me.CreatedAtUtc));
});

meApi.MapPut("/password", async (
    ClaimsPrincipal principal,
    ChangePasswordRequest req,
    AppDbContext db,
    Microsoft.AspNetCore.Identity.PasswordHasher<AppUser> hasher) =>
{
    var userId = GetUserId(principal);
    var me = await db.Users.SingleOrDefaultAsync(u => u.Id == userId);
    if (me is null)
        return Results.NotFound();

    if (string.IsNullOrWhiteSpace(req.CurrentPassword) || string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 6)
        return Results.BadRequest(new { message = "Invalid password." });

    var verified = hasher.VerifyHashedPassword(me, me.PasswordHash, req.CurrentPassword);
    if (verified == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
        return Results.BadRequest(new { message = "Current password is incorrect." });

    me.PasswordHash = hasher.HashPassword(me, req.NewPassword);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Password updated." });
});

var tasksApi = app.MapGroup("/api/tasks").RequireAuthorization();

tasksApi.MapGet("/", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var userId = GetUserId(user);
    var isAdmin = user.IsInRole("Admin");

    if (isAdmin)
    {
        var all = await db.Tasks
            .Join(db.Users,
                t => t.UserId,
                u => u.Id,
                (t, u) => new { t, u })
            .OrderByDescending(x => x.t.CreatedAtUtc)
            .Select(x => new AdminTaskResponse(
                x.t.Id,
                x.u.Id,
                x.u.Email,
                x.u.Name,
                x.t.Title,
                x.t.Notes,
                x.t.IsCompleted,
                x.t.DueAtUtc,
                x.t.CreatedAtUtc))
            .ToListAsync();

        return Results.Ok(all);
    }

    var tasks = await db.Tasks
        .Where(t => t.UserId == userId)
        .OrderByDescending(t => t.CreatedAtUtc)
        .Select(t => new TaskResponse(t.Id, t.Title, t.Notes, t.IsCompleted, t.DueAtUtc, t.CreatedAtUtc))
        .ToListAsync();

    return Results.Ok(tasks);
});

tasksApi.MapPost("/", async (ClaimsPrincipal user, TaskCreateRequest req, AppDbContext db) =>
{
    var userId = GetUserId(user);
    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest(new { message = "Title is required." });

    var task = new TaskItem
    {
        Title = req.Title.Trim(),
        Notes = req.Notes,
        DueAtUtc = req.DueAtUtc,
        UserId = userId
    };

    db.Tasks.Add(task);
    await db.SaveChangesAsync();

    return Results.Created($"/api/tasks/{task.Id}",
        new TaskResponse(task.Id, task.Title, task.Notes, task.IsCompleted, task.DueAtUtc, task.CreatedAtUtc));
});

tasksApi.MapPut("/{id:int}", async (ClaimsPrincipal user, int id, TaskUpdateRequest req, AppDbContext db) =>
{
    var userId = GetUserId(user);
    var isAdmin = user.IsInRole("Admin");
    var task = await db.Tasks.SingleOrDefaultAsync(t => t.Id == id);
    if (task is null)
        return Results.NotFound();

    if (!isAdmin && task.UserId != userId)
        return Results.NotFound();

    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.BadRequest(new { message = "Title is required." });

    task.Title = req.Title.Trim();
    task.Notes = req.Notes;
    task.IsCompleted = req.IsCompleted;
    task.DueAtUtc = req.DueAtUtc;

    await db.SaveChangesAsync();
    return Results.Ok(new TaskResponse(task.Id, task.Title, task.Notes, task.IsCompleted, task.DueAtUtc, task.CreatedAtUtc));
});

tasksApi.MapDelete("/{id:int}", async (ClaimsPrincipal user, int id, AppDbContext db) =>
{
    var userId = GetUserId(user);
    var isAdmin = user.IsInRole("Admin");
    var task = await db.Tasks.SingleOrDefaultAsync(t => t.Id == id);
    if (task is null)
        return Results.NotFound();

    if (!isAdmin && task.UserId != userId)
        return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Tilefonikos katalogos: aloi Users (oxi ego, oxi Admin) + koina Contacts
var directoryApi = app.MapGroup("/api/directory").RequireAuthorization();

directoryApi.MapGet("/", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var currentUserId = GetUserId(user);
    var userRows = await db.Users.AsNoTracking()
        .Where(u => u.Id != currentUserId)
        .Where(u => (u.Role ?? "").ToUpper() != "ADMIN")
        .Select(u => new DirectoryContactResponse(u.Id, u.Name, u.Phone, u.Email, u.Gender, null, u.CreatedAtUtc))
        .ToListAsync();

    var sharedRows = await db.Contacts.AsNoTracking()
        .Where(c => c.IsUserContribution && c.DirectoryListingId != null)
        .Select(c => new DirectoryContactResponse(
            c.DirectoryListingId!.Value,
            c.Name ?? "",
            c.Phone ?? "",
            c.Email ?? "",
            c.Gender,
            c.Id,
            c.CreatedAtUtc))
        .ToListAsync();

    var rows = userRows.Concat(sharedRows)
        .OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();

    return Results.Ok(rows);
});

directoryApi.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var u = await db.Users.AsNoTracking()
        .Where(x => x.Id == id && (x.Role ?? "").ToUpper() != "ADMIN")
        .Select(x => new DirectoryContactResponse(x.Id, x.Name, x.Phone, x.Email, x.Gender, null, x.CreatedAtUtc))
        .SingleOrDefaultAsync();

    if (u is not null)
        return Results.Ok(u);

    var c = await db.Contacts.AsNoTracking()
        .Where(x => x.DirectoryListingId == id && x.IsUserContribution)
        .Select(x => new DirectoryContactResponse(
            x.DirectoryListingId!.Value,
            x.Name ?? "",
            x.Phone ?? "",
            x.Email ?? "",
            x.Gender,
            x.Id,
            x.CreatedAtUtc))
        .SingleOrDefaultAsync();

    return c is null ? Results.NotFound() : Results.Ok(c);
});

var usersApi = app.MapGroup("/api/users").RequireAuthorization("AdminOnly");

usersApi.MapGet("/", async (AppDbContext db) =>
{
    var rows = await db.Users.AsNoTracking()
        .OrderByDescending(u => u.CreatedAtUtc)
        .Select(u => new UserResponse(
            u.Id,
            u.Email,
            u.Role,
            u.CreatedAtUtc,
            db.Tasks.Count(t => t.UserId == u.Id)))
        .ToListAsync();

    return Results.Ok(rows);
});

usersApi.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var u = await db.Users.AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new DirectoryContactResponse(x.Id, x.Name, x.Phone, x.Email, x.Gender, null, x.CreatedAtUtc))
        .SingleOrDefaultAsync();

    return u is null ? Results.NotFound() : Results.Ok(u);
});

usersApi.MapPost("/", async (
    AdminCreateUserRequest req,
    AppDbContext db,
    Microsoft.AspNetCore.Identity.PasswordHasher<AppUser> hasher) =>
{
    var email = (req.Email ?? "").Trim().ToLowerInvariant();
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
        return Results.BadRequest(new { message = "Invalid email/password." });

    var exists = await db.Users.AnyAsync(u => u.Email == email);
    if (exists)
        return Results.Conflict(new { message = "Email already exists." });

    var role = string.IsNullOrWhiteSpace(req.Role) ? "User" : req.Role.Trim();
    if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        role = "User"; // den epitrepetai dhmiourgia Admin apo auto to endpoint

    var user = new AppUser
    {
        Email = email,
        Name = (req.Name ?? "").Trim(),
        Phone = (req.Phone ?? "").Trim(),
        Gender = (req.Gender ?? "Male").Trim(),
        Role = role,
        PasswordHash = "TEMP"
    };
    user.PasswordHash = hasher.HashPassword(user, req.Password);

    db.Users.Add(user);

    // Neo row Contacts gia xristi (mirroring)
    db.Contacts.Add(new Contact
    {
        Name = user.Name,
        Phone = user.Phone,
        Email = user.Email,
        Gender = user.Gender,
        CreatedAtUtc = DateTime.UtcNow
    });

    await db.SaveChangesAsync();

    return Results.Ok(new DirectoryContactResponse(user.Id, user.Name, user.Phone, user.Email, user.Gender, null, user.CreatedAtUtc));
});

usersApi.MapPut("/{id:guid}", async (
    Guid id,
    AdminUpdateUserRequest req,
    AppDbContext db,
    Microsoft.AspNetCore.Identity.PasswordHasher<AppUser> hasher) =>
{
    var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id);
    if (user is null) return Results.NotFound();
    if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { message = "Admin user cannot be edited here." });

    var oldEmail = user.Email;
    var email = (req.Email ?? "").Trim().ToLowerInvariant();
    if (string.IsNullOrWhiteSpace(email))
        return Results.BadRequest(new { message = "Invalid email." });

    var emailTaken = await db.Users.AnyAsync(u => u.Email == email && u.Id != id);
    if (emailTaken)
        return Results.Conflict(new { message = "Email already exists." });

    user.Name = (req.Name ?? "").Trim();
    user.Phone = (req.Phone ?? "").Trim();
    user.Gender = (req.Gender ?? user.Gender).Trim();
    user.Email = email;

    if (!string.IsNullOrWhiteSpace(req.NewPassword))
    {
        if (req.NewPassword.Trim().Length < 6)
            return Results.BadRequest(new { message = "Password must be at least 6 characters." });
        user.PasswordHash = hasher.HashPassword(user, req.NewPassword.Trim());
    }

    // Enimerwsh Contacts otan allazei email (profile / admin)
    var contact = await db.Contacts.SingleOrDefaultAsync(c => c.Email == oldEmail);
    if (contact is null)
    {
        db.Contacts.Add(new Contact
        {
            Name = user.Name,
            Phone = user.Phone,
            Email = user.Email,
            Gender = user.Gender,
            CreatedAtUtc = DateTime.UtcNow
        });
    }
    else
    {
        contact.Name = user.Name;
        contact.Phone = user.Phone;
        contact.Email = user.Email;
        contact.Gender = user.Gender;
    }

    await db.SaveChangesAsync();
    return Results.Ok(new DirectoryContactResponse(user.Id, user.Name, user.Phone, user.Email, user.Gender, null, user.CreatedAtUtc));
});

usersApi.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id);
    if (user is null) return Results.NotFound();
    if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { message = "Admin user cannot be deleted." });

    // Diagrafi Contact me idio email me ton xristi
    var contact = await db.Contacts.SingleOrDefaultAsync(c => c.Email == user.Email);
    if (contact is not null)
        db.Contacts.Remove(contact);

    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();