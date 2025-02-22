using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Yazlab2;
using Yazlab2.Data;
using Yazlab2.Services;

var builder = WebApplication.CreateBuilder(args);

// Servisleri container'a ekleyin
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<EmailService>();

// ApplicationDbContext'i ekleyin
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Kimlik do�rulama ve �erez yap�land�rmas�
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/LoginPage/Index"; // Giri� sayfas� y�nlendirmesi
        options.AccessDeniedPath = "/Home/AccessDenied"; // Yetkisiz eri�im sayfas�
        options.SlidingExpiration = true; // �erez s�resi yenilensin
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // �erezlerin ge�erlilik s�resi

    });

// Uygulama olu�turuluyor
var app = builder.Build();

// HTTPS y�nlendirmeyi etkinle�tir
app.UseHttpsRedirection();

// Statik dosyalar�n eri�imi sa�lan�yor
app.UseStaticFiles();

// Routing (y�nlendirme) i�lemi
app.UseRouting();

// Authentication ve Authorization middleware'lerini etkinle�tir
app.UseAuthentication();
app.UseAuthorization();

// Varsay�lan controller ve action y�nlendirmesi
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LoginPage}/{action=Index}/{id?}");

// Uygulama �al��t�r�l�yor
app.Run();
