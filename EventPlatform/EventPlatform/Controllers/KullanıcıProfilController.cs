using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Yazlab2.Data;
using Yazlab2.Models;

namespace Yazlab2.Controllers
{
    public class KullanıcıProfilController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KullanıcıProfilController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Giriş yapan kullanıcının kimliğini al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcı bilgilerini ve ilgi alanlarını al
            var user = _context.Kullanıcılar
                .Include(u => u.KatildigiEtkinlikler)
                .ThenInclude(k => k.Etkinlik)
                .FirstOrDefault(u => u.ID == int.Parse(userId));

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Şu anki tarih
            var currentDate = DateTime.Now;

            // Geçmiş ve gelecek etkinlikler
            var katildigiEtkinlikler = user.KatildigiEtkinlikler
                .Where(k => k.Etkinlik.Tarih < currentDate) // Tarihi geçmiş etkinlikler
                .Select(k => k.Etkinlik) // Sadece etkinlik detaylarını al
                .ToList();

            var katilacagiEtkinlikler = user.KatildigiEtkinlikler
                .Where(k => k.Etkinlik.Tarih >= currentDate) // Tarihi gelmemiş etkinlikler
                .Select(k => k.Etkinlik)
                .ToList();
            // Kullanıcının ilgi alanlarını al
            var userIlgiAlanlari = _context.KullaniciIlgiAlanlari
                .Where(ka => ka.KullanıcıID == int.Parse(userId))
                .Select(ka => ka.IlgiAlaniID)
                .ToList();

            // Tüm ilgi alanlarını al
            var tumIlgiAlanlari = _context.IlgiAlanlari.ToList();

            // ViewBag üzerinden ilgi alanlarını ve kullanıcının seçili ilgi alanlarını geçiyoruz
            ViewBag.TumIlgiAlanlari = tumIlgiAlanlari;
            ViewBag.SeciliIlgiAlanlari = userIlgiAlanlari;

            // Etkinlikleri ViewBag ile görünümde paylaş
            ViewBag.KatildigiEtkinlikler = katildigiEtkinlikler;
            ViewBag.KatilacagiEtkinlikler = katilacagiEtkinlikler;

            return View(user);
        }
        public async Task<IActionResult> UpdateProfile(Kullanıcı model, IFormFile ProfilFotografi, [FromForm] List<int> selectedIlgiAlanlari)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Kullanıcılar.FirstOrDefault(u => u.ID == int.Parse(userId));
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Kullanıcı bilgilerini güncelle
            user.KullaniciAdi = model.KullaniciAdi ?? user.KullaniciAdi;
            user.Eposta = model.Eposta ?? user.Eposta;
            user.Konum = model.Konum ?? user.Konum;
            user.Ad = model.Ad ?? user.Ad;
            user.Soyad = model.Soyad ?? user.Soyad;
            user.DogumTarihi = model.DogumTarihi;
            user.Cinsiyet = model.Cinsiyet ?? user.Cinsiyet;
            user.TelefonNumarasi = model.TelefonNumarasi ?? user.TelefonNumarasi;

            // Profil fotoğrafını güncelle
            if (ProfilFotografi != null && ProfilFotografi.Length > 0)
            {
                var uploadsFolder = Path.Combine("wwwroot", "resim");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ProfilFotografi.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilFotografi.CopyToAsync(stream);
                }

                user.ProfilFotografiYolu = $"/resim/{uniqueFileName}";
            }

            // Ilgi alanlarını güncelle
            if (selectedIlgiAlanlari != null && selectedIlgiAlanlari.Any())
            {
                var currentIlgiAlanlari = _context.KullaniciIlgiAlanlari
                    .Where(ka => ka.KullanıcıID == int.Parse(userId))
                    .Select(ka => ka.IlgiAlaniID)
                    .ToList();

                foreach (var ilgiAlaniId in selectedIlgiAlanlari)
                {
                    if (!currentIlgiAlanlari.Contains(ilgiAlaniId))
                    {
                        var yeniIlgiAlani = new KullaniciIlgiAlani
                        {
                            KullanıcıID = int.Parse(userId),
                            IlgiAlaniID = ilgiAlaniId
                        };
                        _context.KullaniciIlgiAlanlari.Add(yeniIlgiAlani);
                    }
                }

                await _context.SaveChangesAsync();
            }

            // Kullanıcı profilini güncelle
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "KullanıcıProfil");
        }


        public IActionResult AdminProfili()
        {
            // Giriş yapan kullanıcının kimliğini al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcı bilgilerini veritabanından çek
            var user = _context.Kullanıcılar.FirstOrDefault(u => u.ID == int.Parse(userId));

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user); // AdminProfil.cshtml görünümüne kullanıcı verilerini gönder
        }


        [HttpPost]
        public async Task<IActionResult> AdminUpdateProfile(Kullanıcı model, IFormFile ProfilFotografi)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Kullanıcılar.FirstOrDefault(u => u.ID == int.Parse(userId));
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Kullanıcı bilgilerini güncelle
            user.KullaniciAdi = model.KullaniciAdi ?? user.KullaniciAdi;
            user.Eposta = model.Eposta ?? user.Eposta;
            user.Konum = model.Konum ?? user.Konum;
            user.Ad = model.Ad ?? user.Ad;
            user.Soyad = model.Soyad ?? user.Soyad;
            user.DogumTarihi = model.DogumTarihi;
            user.Cinsiyet = model.Cinsiyet ?? user.Cinsiyet;
            user.TelefonNumarasi = model.TelefonNumarasi ?? user.TelefonNumarasi;


            // Profil fotoğrafını güncelle
            if (ProfilFotografi != null && ProfilFotografi.Length > 0)
            {
                // Fotoğrafı kaydet
                var uploadsFolder = Path.Combine("wwwroot", "resim");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ProfilFotografi.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilFotografi.CopyToAsync(stream);
                }

                user.ProfilFotografiYolu = $"/resim/{uniqueFileName}";
            }

            // Veritabanına kaydet
            _context.SaveChanges();

            return RedirectToAction("AdminProfili", "KullanıcıProfil");
        }








    }
}