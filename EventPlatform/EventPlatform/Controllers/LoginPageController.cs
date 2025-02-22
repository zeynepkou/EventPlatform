using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Yazlab2.Data;
using Yazlab2.Models;

namespace Yazlab2.Controllers
{
    public class LoginPageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginPageController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Layout"] = null;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password, string role)
        {
            // Kullanıcı adı ile veritabanında kullanıcıyı bul
            var user = _context.Kullanıcılar
                .FirstOrDefault(u => u.KullaniciAdi == username);

            if (user != null)
            {
                bool isPasswordValid;

              
                
                    // Diğer kullanıcılar için hashlenmiş şifreyi doğrula
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Sifre);
                

                if (isPasswordValid && user.KullaniciTuru.ToLower() == role.ToLower())
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.KullaniciAdi),
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()), // Kullanıcı ID'sini saklayın
                new Claim(ClaimTypes.Role, user.KullaniciTuru)
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true // Oturumu kalıcı yap
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                    if (role.ToLower() == "admin")
                    {
                        return RedirectToAction("AdminIndex", "Admin");
                    }
                    else if (role.ToLower() == "kullanıcı")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            // Geçersiz giriş durumu
            Debug.WriteLine("Geçersiz kullanıcı adı veya şifre");
            ViewData["Layout"] = null;
            return View();

        }
    }
}
