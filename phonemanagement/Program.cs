using PhoneBookApp.Models;
using phonemanagement.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();

// Razor Components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


// DELETE CONTACT

app.MapPost("/delete-contact", (HttpContext context) =>
{
    if (int.TryParse(context.Request.Form["id"], out int id))
    {
        ContactsRepository.DeleteContact(id);
    }

    return Results.Redirect("/contacts");
})
.DisableAntiforgery();



// SAVE (EDIT CONTACT)
app.MapPost("/edit-contact-save/{id:int}", (HttpContext context, int id) =>
{
    var name = context.Request.Form["Name"].ToString();
    var phone = context.Request.Form["Phone"].ToString();

    var contact = new Contact
    {
        Id = id,
        Name = name,
        Phone = phone
    };

    ContactsRepository.UpdateContact(contact);

    return Results.Redirect("/contacts");
})
.DisableAntiforgery();


// ADD CONTACT
app.MapPost("/add-contact-save", (HttpContext context) =>
{
    var name = context.Request.Form["Name"].ToString();
    var phone = context.Request.Form["Phone"].ToString();

    var contact = new Contact
    {
        Name = name,
        Phone = phone
    };

    ContactsRepository.AddContact(contact);

    return Results.Redirect("/contacts");
})
.DisableAntiforgery(); 


app.Run();