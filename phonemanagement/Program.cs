using PhoneBookApp.Models;
using phonemanagement.Components;
using phonemanagement.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (!string.IsNullOrWhiteSpace(builder.Configuration["Firebase:DatabaseUrl"]))
{
    builder.Services.AddHttpClient<IContactsStore, RealtimeDbContactsStore>();
}
else
{
    builder.Services.AddSingleton<IContactsStore, InMemoryContactsStore>();
}

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAntiforgery();

app.MapStaticAssets();

app.UseSwagger();
app.UseSwaggerUI();

var contactsApi = app.MapGroup("/api/contacts");

contactsApi.MapGet("/", async (IContactsStore store) =>
    Results.Ok(await store.GetAllAsync()));

contactsApi.MapGet("/{id:int}", async (int id, IContactsStore store) =>
{
    var contact = await store.GetByIdAsync(id);
    return contact is null ? Results.NotFound() : Results.Ok(contact);
});

contactsApi.MapGet("/search", async (string q, IContactsStore store) =>
    Results.Ok(await store.SearchAsync(q)));

contactsApi.MapPost("/", async (Contact contact, IContactsStore store) =>
{
    var created = await store.AddAsync(contact);
    return Results.Created($"/api/contacts/{created.Id}", created);
});

contactsApi.MapPut("/{id:int}", async (int id, Contact contact, IContactsStore store) =>
{
    contact.Id = id;
    var ok = await store.UpdateAsync(contact);
    return ok ? Results.Ok(contact) : Results.NotFound();
});

contactsApi.MapDelete("/{id:int}", async (int id, IContactsStore store) =>
{
    var ok = await store.DeleteAsync(id);
    return ok ? Results.NoContent() : Results.NotFound();
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();