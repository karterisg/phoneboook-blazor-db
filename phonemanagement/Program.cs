using PhoneBookApp.Models; // fernei to Contact model pou xrisimoipoioume se API/UI
using phonemanagement.Components; // fernei to Blazor App component (UI root)
using phonemanagement.Data; // fernei AppDbContext + DbSeeder gia SQL/EF
using phonemanagement.Dtos.Auth;
using phonemanagement.Dtos.Directory;
using phonemanagement.Dtos.Tasks;
using phonemanagement.Dtos.Users;
using phonemanagement.Models;
using phonemanagement.Services; // fernei IContactsStore + SqlContactsStore (CRUD layer)
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using System.Security.Claims;
using Microsoft.EntityFrameworkCore; // EF Core APIs (UseSqlServer, MigrateAsync, klt)
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

var builder = WebApplication.CreateBuilder(args); // ftiaxnei ton ASP.NET Core builder (config + DI)

//Razor Components // energopoiei Blazor Server (Razor Components) me interactivity
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); // epitrepei events (@onclick, forms) na doulevoun server-side

builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<ClientAuthState>();
builder.Services.AddScoped<ApiClient>();

//gia Postman // CORS policy gia na mporoun external clients na kanoun requests
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()); // allow ola ta methods/headers/origins (dev-friendly)
});

//API testing // swagger/openapi gia dokimes tou API
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
}); // ftiaxnei swagger docs + swagger UI (me JWT)

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

builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>(); // DI gia JWT token creation (JwtTokenService implementei ego, IJwtTokenService to interface tou)
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.PasswordHasher<phonemanagement.Models.AppUser>>(); // DI gia password hashing (xrisimopoioume to built-in PasswordHasher apo ASP.NET Core Identity, an kai den xrisimopoioume olokliro Identity)


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // connection string (LocalDB/LOCALHOST) apo appsettings
if (!string.IsNullOrWhiteSpace(connectionString)) // an yparxei, doulevoume me SQL Server
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString)); // EF Core provider = SQL Server
    builder.Services.AddScoped<IContactsStore, SqlContactsStore>(); // IContactsStore -> SqlContactsStore (CRUD stin SQL)
    Console.WriteLine("Using SQL Server store"); // debug log
}
else
{
    throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection. Configure SQL Server connection string."); // xoris SQL config den trexei
}

var app = builder.Build(); // xtizei to app (middleware + endpoints)

// if all deleted put demo seed again // seed demo data mono an einai adeia i vasi
if (!string.IsNullOrWhiteSpace(connectionString)) // extra check
{
    using var scope = app.Services.CreateScope(); // ftiaxnei scope gia na parei DbContext ektos request
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // pairnei DbContext
    var hasher = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.PasswordHasher<AppUser>>();
    await db.Database.MigrateAsync(); // efarmozei migrations (schema up to date)
    await DbSeeder.SeedAsync(db, hasher); // vazei arxika data + admin user
}

//Middleware pipeline // seira middlewares pou trexoun se kathe request
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true); // production error page
    app.UseHsts(); // HSTS
}

app.UseHttpsRedirection(); // redirect se https (opou yparxei)

app.UseCors(); // energopoiei CORS policy

app.UseAntiforgery(); // energopoiei CSRF protection gia UI

app.UseAuthentication();
app.UseAuthorization();

var meApi = app.MapGroup("/api").RequireAuthorization();
meApi.MapGet("/me", (ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
    var email = user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email") ?? user.Identity?.Name;
    return Results.Ok(new
    {
        userId,
        email
    });
});


//Static files
app.MapStaticAssets(); // servirei static assets (wwwroot)

//Swagger UI
app.UseSwagger(); // expose swagger json
app.UseSwaggerUI(); // expose swagger UI

//API endpoints
var contactsApi = app.MapGroup("/api/contacts"); // base route group gia contacts API


contactsApi.RequireAuthorization();


//GET all
contactsApi.MapGet("/", async (IContactsStore store) =>
{
    var contacts = await store.GetAllAsync(); // SELECT ola ta contacts apo SQL
    return Results.Ok(contacts); // 200 + JSON
});

//GET by id
contactsApi.MapGet("/{id:int}", async (int id, IContactsStore store) =>
{
    var contact = await store.GetByIdAsync(id); // SELECT 1 me Id
    return contact is null ? Results.NotFound() : Results.Ok(contact); // 404 an den vrethei, alliws 200
});

//SEARCH
contactsApi.MapGet("/search", async (string q, IContactsStore store) =>
{
    var results = await store.SearchAsync(q); // search se Name/Phone/Email
    return Results.Ok(results); // 200 + lista
});

//CREATE
contactsApi.MapPost("/", async (Contact contact, IContactsStore store) =>
{
    var created = await store.AddAsync(contact); // INSERT stin SQL (Id identity)
    return Results.Created($"/api/contacts/{created.Id}", created); // 201 Created
}).RequireAuthorization("AdminOnly");

//UPDATE
contactsApi.MapPut("/{id:int}", async (int id, Contact contact, IContactsStore store) =>
{
    if (id != contact.Id) // an to route Id den tairiazei me body Id
        contact.Id = id; // kratame route Id

    var ok = await store.UpdateAsync(contact); // UPDATE stin SQL
    return ok ? Results.Ok(contact) : Results.NotFound(); // 200 h 404
}).RequireAuthorization("AdminOnly");

//DELETE
contactsApi.MapDelete("/{id:int}", async (int id, IContactsStore store) =>
{
    var ok = await store.DeleteAsync(id); // DELETE apo SQL
    return ok ? Results.NoContent() : Results.NotFound(); // 204 h 404
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

    var exists = await db.Users.AnyAsync(u => u.Email == email);
    if (exists)
        return Results.Conflict(new { message = "Email already registered." });

    var user = new AppUser
    {
        Email = email,
        PasswordHash = "TEMP"
    };
    user.PasswordHash = hasher.HashPassword(user, req.Password);

    db.Users.Add(user);
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

    // Allow admin to login with "admin" as identifier
    if (string.Equals(identifier, "admin", StringComparison.OrdinalIgnoreCase))
        email = "admin@test.com";

    var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
    if (user is null)
        return Results.Unauthorized();

    // Password length rule exception: allow 5 chars only for admin ("admin")
    if (req.Password.Length < 6 && !string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { message = "Invalid credentials." });

    var verified = hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
    if (verified == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
        return Results.Unauthorized();

    var (token, expiresAtUtc) = jwt.CreateAccessToken(user);
    return Results.Ok(new AuthResponse(token, expiresAtUtc, user.Email));
});

var tasksApi = app.MapGroup("/api/tasks").RequireAuthorization();

tasksApi.MapGet("/", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var userId = GetUserId(user);
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
    var task = await db.Tasks.SingleOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    if (task is null)
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
    var task = await db.Tasks.SingleOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    if (task is null)
        return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

var directoryApi = app.MapGroup("/api/directory").RequireAuthorization();

directoryApi.MapGet("/", async (ClaimsPrincipal user, AppDbContext db) =>
{
    var currentUserId = GetUserId(user);
    var rows = await db.Users.AsNoTracking()
        .Where(u => u.Id != currentUserId)
        .Where(u => (u.Role ?? "").ToUpper() != "ADMIN")
        .OrderBy(u => u.Name)
        .Select(u => new DirectoryContactResponse(u.Id, u.Name, u.Phone, u.Email, u.Gender))
        .ToListAsync();

    return Results.Ok(rows);
});

directoryApi.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var u = await db.Users.AsNoTracking()
        .Where(x => x.Id == id && (x.Role ?? "").ToUpper() != "ADMIN")
        .Select(x => new DirectoryContactResponse(x.Id, x.Name, x.Phone, x.Email, x.Gender))
        .SingleOrDefaultAsync();

    return u is null ? Results.NotFound() : Results.Ok(u);
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
        .Select(x => new DirectoryContactResponse(x.Id, x.Name, x.Phone, x.Email, x.Gender))
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
        role = "User"; // do not allow creating admins via this endpoint

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
    await db.SaveChangesAsync();

    return Results.Ok(new DirectoryContactResponse(user.Id, user.Name, user.Phone, user.Email, user.Gender));
});

usersApi.MapPut("/{id:guid}", async (Guid id, AdminUpdateUserRequest req, AppDbContext db) =>
{
    var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id);
    if (user is null) return Results.NotFound();
    if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { message = "Admin user cannot be edited here." });

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

    await db.SaveChangesAsync();
    return Results.Ok(new DirectoryContactResponse(user.Id, user.Name, user.Phone, user.Email, user.Gender));
});

usersApi.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id);
    if (user is null) return Results.NotFound();
    if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest(new { message = "Admin user cannot be deleted." });

    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

//Blazor app
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); // energopoiei interactive render mode gia components


app.MapRazorComponents<App>() // deyteri mapping tou App
    .AddAdditionalAssemblies(); // den exei arguments, opote einai praktika peritto

app.Run(); // ksekinaei ton server kai akouei sta configured ports