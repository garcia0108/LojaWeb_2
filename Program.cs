using LojaWeb_2.Data;
using LojaWeb_2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddScoped<EmailService>();

builder.Services.AddControllersWithViews()
    .AddSessionStateTempDataProvider();

// Banco de dados
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;//O usuário poderá entrar sem precisar confirmar o e-mail.

    options.Password.RequireDigit = true;//A senha precisa ter pelo menos um número.
    options.Password.RequireLowercase = true;//Precisa ter pelo menos uma letra minúscula.
    options.Password.RequireUppercase = false;//Não é obrigatório usar letra maiúscula.
    options.Password.RequireNonAlphanumeric = false;//Não é obrigatório usar símbolos como @, #, !.
    options.Password.RequiredLength = 6;//A senha precisa ter no mínimo 6 caracteres.
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configuração do cookie de autenticação
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";

    options.AccessDeniedPath = "/Home/AccessDenied";
});

// Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages();

QuestPDF.Settings.License =
    QuestPDF.Infrastructure.LicenseType.Community;

builder.Services.AddScoped<FreteService>();

var app = builder.Build();


var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine(
    $"CONNECTION STRING EXISTE: {!string.IsNullOrWhiteSpace(connectionString)}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await IdentitySeed.SeedAsync(services);
}

// Tratamento de erros
app.UseExceptionHandler("/Home/Error");

app.UseStatusCodePagesWithReExecute(
    "/Home/NotFoundPage");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Session
app.UseSession();

// Autenticação
app.UseAuthentication();

// Autorização
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();// <-- Necessário para renderizar as telas do Identity UI

app.Run();