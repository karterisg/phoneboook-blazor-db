using PhoneBookApp.Models;
using phonemanagement.Components;
using phonemanagement.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Razor Components (Blazor Server)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 🔹 CORS (για Postman / external calls)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// 🔹 Swagger (API testing)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔹 Decide which store to use (Firebase ή InMemory)
var firebaseUrl = builder.Configuration["Firebase:DatabaseUrl"];

if (!string.IsNullOrWhiteSpace(firebaseUrl))
{
    builder.Services.AddHttpClient<IContactsStore, RealtimeDbContactsStore>();
    Console.WriteLine("Using Firebase Realtime DB");
}
else
{
    builder.Services.AddSingleton<IContactsStore, InMemoryContactsStore>();
    Console.WriteLine("Using InMemory store");
}

var app = builder.Build();

// 🔹 Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAntiforgery();

// 🔹 Static files
app.MapStaticAssets();

// 🔹 Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// 🔹 API endpoints
var contactsApi = app.MapGroup("/api/contacts");

// GET all
contactsApi.MapGet("/", async (IContactsStore store) =>
{
    var contacts = await store.GetAllAsync();
    return Results.Ok(contacts);
});

// GET by id
contactsApi.MapGet("/{id:int}", async (int id, IContactsStore store) =>
{
    var contact = await store.GetByIdAsync(id);
    return contact is null ? Results.NotFound() : Results.Ok(contact);
});

// SEARCH
contactsApi.MapGet("/search", async (string q, IContactsStore store) =>
{
    var results = await store.SearchAsync(q);
    return Results.Ok(results);
});

// CREATE
contactsApi.MapPost("/", async (Contact contact, IContactsStore store) =>
{
    var created = await store.AddAsync(contact);
    return Results.Created($"/api/contacts/{created.Id}", created);
});

// UPDATE
contactsApi.MapPut("/{id:int}", async (int id, Contact contact, IContactsStore store) =>
{
    if (id != contact.Id)
        contact.Id = id;

    var ok = await store.UpdateAsync(contact);
    return ok ? Results.Ok(contact) : Results.NotFound();
});

// DELETE
contactsApi.MapDelete("/{id:int}", async (int id, IContactsStore store) =>
{
    var ok = await store.DeleteAsync(id);
    return ok ? Results.NoContent() : Results.NotFound();
});

// 🔹 Blazor app
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();