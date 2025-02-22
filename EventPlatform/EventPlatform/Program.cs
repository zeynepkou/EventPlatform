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

// Kimlik doðrulama ve çerez yapýlandýrmasý
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/LoginPage/Index"; // Giriþ sayfasý yönlendirmesi
        options.AccessDeniedPath = "/Home/AccessDenied"; // Yetkisiz eriþim sayfasý
        options.SlidingExpiration = true; // Çerez süresi yenilensin
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Çerezlerin geçerlilik süresi

    });

// Uygulama oluþturuluyor
var app = builder.Build();

// HTTPS yönlendirmeyi etkinleþtir
app.UseHttpsRedirection();

// Statik dosyalarýn eriþimi saðlanýyor
app.UseStaticFiles();

// Routing (yönlendirme) iþlemi
app.UseRouting();

// Authentication ve Authorization middleware'lerini etkinleþtir
app.UseAuthentication();
app.UseAuthorization();

// Varsayýlan controller ve action yönlendirmesi
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LoginPage}/{action=Index}/{id?}");

// Uygulama çalýþtýrýlýyor
app.Run();
