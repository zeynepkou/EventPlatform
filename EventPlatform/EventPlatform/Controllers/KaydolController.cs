using Microsoft.AspNetCore.Mvc;
using Yazlab2.Models;
using Yazlab2.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;

public class KaydolController : Controller
{
    private readonly ApplicationDbContext _context;

    public KaydolController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        ViewBag.IlgiAlanlari = _context.IlgiAlanlari.ToList();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(Kullanıcı kullanici, IFormFile ProfilFotografi, int[] IlgiAlanlari)
    {
        try
        {
            if (ModelState.IsValid)
            {
                Debug.WriteLine("Model geçerli, kullanıcı kaydı işleme başlıyor...");

                // Şifreyi hashleyelim
                kullanici.Sifre = HashPassword(kullanici.Sifre);
                Debug.WriteLine("Şifre başarıyla hash edildi.");

                // Profil fotoğrafı yüklendiyse işlemleri başlat
                if (ProfilFotografi != null && ProfilFotografi.Length > 0)
                {
                    string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resim");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + ProfilFotografi.FileName;
                    string filePath = Path.Combine(uploadPath, uniqueFileName);

                    Debug.WriteLine("Profil fotoğrafı yükleniyor...");
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfilFotografi.CopyToAsync(stream);
                    }

                    kullanici.ProfilFotografiYolu = "/resim/" + uniqueFileName;
                    Debug.WriteLine($"Profil fotoğrafı kaydedildi: {kullanici.ProfilFotografiYolu}");
                }

                // Kullanıcıyı veritabanına ekle
                _context.Kullanıcılar.Add(kullanici);
                await _context.SaveChangesAsync();
                Debug.WriteLine("Kullanıcı başarıyla veritabanına kaydedildi.");

                // Kullanıcıya 20 puan ekle
                var puan = new Puan
                {
                    KullanıcıID = kullanici.ID,
                    Puanlar = 20, // 20 puan ekleniyor
                    KazanilanTarih = DateTime.Now
                };

                _context.Puanlar.Add(puan);
                await _context.SaveChangesAsync();
                Debug.WriteLine("20 puan başarıyla Puanlar tablosuna eklendi.");

                // İlgi alanlarını işleyelim
                if (IlgiAlanlari != null && IlgiAlanlari.Any())
                {
                    List<KullaniciIlgiAlani> kullaniciIlgiAlanlari = IlgiAlanlari
                        .Select(ilgiAlaniId => new KullaniciIlgiAlani
                        {
                            KullanıcıID = kullanici.ID,
                            IlgiAlaniID = ilgiAlaniId
                        })
                        .ToList();

                    _context.KullaniciIlgiAlanlari.AddRange(kullaniciIlgiAlanlari);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("İlgi alanları başarıyla kaydedildi.");
                }

                return RedirectToAction("Index", "LoginPage");
            }
            else
            {
                Debug.WriteLine("ModelState geçersiz.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Debug.WriteLine("Hata: " + error.ErrorMessage);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Hata: " + ex.Message);
        }

        Debug.WriteLine("Kayıt işlemi başarısız oldu.");
        return View(kullanici);
    }



    private string HashPassword(string password)
    {
        Debug.WriteLine("Şifre hash işlemi başladı.");
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}
