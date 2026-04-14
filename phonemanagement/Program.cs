using PhoneBookApp.Models; // fernei to Contact model pou xrisimopoioume se API/UI
using phonemanagement.Components; // fernei to Blazor App component (UI root)
using phonemanagement.Data; // fernei AppDbContext + DbSeeder gia SQL/EF
using phonemanagement.Services; // fernei IContactsStore + SqlContactsStore (CRUD layer)
using Microsoft.EntityFrameworkCore; // EF Core APIs (UseSqlServer, MigrateAsync, klt)

var builder = WebApplication.CreateBuilder(args); // ftiaxnei ton ASP.NET Core builder (config + DI)

//Razor Components // energopoiei Blazor Server (Razor Components) me interactivity
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); // epitrepei events (@onclick, forms) na doulevoun server-side

//gia Postman // CORS policy gia na mporoun external clients na kanoun requests
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()); // allow ola ta methods/headers/origins (dev-friendly)
});

//API testing // swagger/openapi gia dokimes tou API
builder.Services.AddEndpointsApiExplorer(); // paragei OpenAPI metadata apo minimal APIs
builder.Services.AddSwaggerGen(); // ftiaxnei swagger docs + swagger UI
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN"; // orizei poio header xrisimopoiei to antiforgery
});

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
    await db.Database.MigrateAsync(); // efarmozei migrations (schema up to date)
    await DbSeeder.SeedAsync(db); // vazei arxika data an den yparxoun contacts
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

//Static files
app.MapStaticAssets(); // servirei static assets (wwwroot)

//Swagger UI
app.UseSwagger(); // expose swagger json
app.UseSwaggerUI(); // expose swagger UI

//API endpoints
var contactsApi = app.MapGroup("/api/contacts"); // base route group gia contacts API



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
});

//UPDATE
contactsApi.MapPut("/{id:int}", async (int id, Contact contact, IContactsStore store) =>
{
    if (id != contact.Id) // an to route Id den tairiazei me body Id
        contact.Id = id; // kratame route Id

    var ok = await store.UpdateAsync(contact); // UPDATE stin SQL
    return ok ? Results.Ok(contact) : Results.NotFound(); // 200 h 404
});

//DELETE
contactsApi.MapDelete("/{id:int}", async (int id, IContactsStore store) =>
{
    var ok = await store.DeleteAsync(id); // DELETE apo SQL
    return ok ? Results.NoContent() : Results.NotFound(); // 204 h 404
});

//Blazor app
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); // energopoiei interactive render mode gia components


app.MapRazorComponents<App>() // deyteri mapping tou App
    .AddAdditionalAssemblies(); // den exei arguments, opote einai praktika peritto

app.Run(); // ksekinaei ton server kai akouei sta configured ports