using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Yazlab2.Data;
using Yazlab2.Migrations;
using Yazlab2.Models;

namespace Yazlab2.Controllers
{
    public class CreateController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CreateController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public IActionResult Create()
        {
            
            ViewBag.IlgiAlanlari = _context.IlgiAlanlari.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Etkinlik etkinlik, IFormFile EtkinlikResmi)
        {

            
            Debug.WriteLine("KategoriID: " + etkinlik.KategoriID);

            if (etkinlik.KategoriID == 0)
            {
                TempData["ErrorMessage"] = "Bir kategori seçmeniz gerekiyor.";
                return RedirectToAction("Create");
            }

            
            var kullanıcıIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (kullanıcıIdClaim == null)
            {
                Debug.WriteLine("Kullanıcı ID'si alınamadı.");
                return View(etkinlik); 
            }

            var kullanıcıId = int.Parse(kullanıcıIdClaim.Value);

            
            var katildigiEtkinlikler = _context.Katılımcılar
                .Where(k => k.KullanıcıID == kullanıcıId)
                .Select(k => k.Etkinlik)
                .ToList();

            // Yeni etkinliğin başlangıç ve bitiş zamanını hesaplıyoruz
            var yeniEtkinlikBaslangic = etkinlik.Tarih.Add(etkinlik.Saat);
            var yeniEtkinlikBitis = yeniEtkinlikBaslangic.Add(etkinlik.EtkinlikSuresi);

            // Çakışma kontrolü
            foreach (var katilinanEtkinlik in katildigiEtkinlikler)
            {
                var katilinanEtkinlikBaslangic = katilinanEtkinlik.Tarih.Add(katilinanEtkinlik.Saat);
                var katilinanEtkinlikBitis = katilinanEtkinlikBaslangic.Add(katilinanEtkinlik.EtkinlikSuresi);

                if ((yeniEtkinlikBaslangic < katilinanEtkinlikBitis) && (yeniEtkinlikBitis > katilinanEtkinlikBaslangic))
                {
                    TempData["ErrorMessage"] = "Bu etkinlik, katıldığınız başka bir etkinlikle çakışmaktadır!";
                    Debug.WriteLine("Bu etkinlik, katıldığınız başka bir etkinlikle çakışmaktadır!");
                    return RedirectToAction("Create"); // Çakışma varsa tekrar Create sayfasına yönlendir
                }
            }

            
            if (EtkinlikResmi != null && EtkinlikResmi.Length > 0)
            {
                
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resim");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + EtkinlikResmi.FileName;
                string filePath = Path.Combine(uploadPath, uniqueFileName);

                Debug.WriteLine("Resim yükleniyor...");

               
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await EtkinlikResmi.CopyToAsync(stream);
                }

                
                etkinlik.EtkinlikResmi = "/resim/" + uniqueFileName;
                Debug.WriteLine($"Resim kaydedildi: {etkinlik.EtkinlikResmi}");
            }

            
            etkinlik.KullanıcıID = kullanıcıId;
            etkinlik.EtkinlikDurum = 2;
            _context.Etkinlikler.Add(etkinlik);

            try
            {
                await _context.SaveChangesAsync();
                // Bildirim oluştur
                var bildirim = new Bildirim
                {
                    KullanıcıID = kullanıcıId,
                    Mesaj = $"Yeni bir etkinlik oluşturuldu: {etkinlik.EtkinlikAdi}",
                    Tarih = DateTime.Now,
                    OkunduMu = false
                };

                _context.Bildirimler.Add(bildirim);
                
                var puan = new Puan
                {
                    KullanıcıID = kullanıcıId,
                    Puanlar = 15,
                    KazanilanTarih = DateTime.Now
                };

                _context.Puanlar.Add(puan);
                await _context.SaveChangesAsync();

                Debug.WriteLine("Etkinlik ve puan başarıyla kaydedildi!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Veritabanına kaydetme hatası: " + ex.Message);
            }


            return RedirectToAction("Index", "Home");
        }


       
        public IActionResult AdminCreate()
        {
            
            ViewBag.IlgiAlanlari = _context.IlgiAlanlari.ToList();
            return View();
        }


        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminCreate(Etkinlik etkinlik, IFormFile EtkinlikResmi)
        {

            
            Debug.WriteLine("KategoriID: " + etkinlik.KategoriID);

            if (etkinlik.KategoriID == 0)
            {
                TempData["ErrorMessage"] = "Bir kategori seçmeniz gerekiyor.";
                return RedirectToAction("Create");
            }

            
            var kullanıcıIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (kullanıcıIdClaim == null)
            {
                Debug.WriteLine("Kullanıcı ID'si alınamadı.");
                return View(etkinlik); 
            }

            var kullanıcıId = int.Parse(kullanıcıIdClaim.Value); 



            // Profil fotoğrafı yüklendiyse işlemleri başlat
            if (EtkinlikResmi != null && EtkinlikResmi.Length > 0)
            {
                // Klasörün varlığını kontrol edin, yoksa oluşturun
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resim");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Dosya adını benzersiz hale getirin (örneğin, GUID ekleyerek)
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + EtkinlikResmi.FileName;
                string filePath = Path.Combine(uploadPath, uniqueFileName);

                Debug.WriteLine("Resim yükleniyor...");

                // Dosyayı wwwroot/images dizinine kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await EtkinlikResmi.CopyToAsync(stream);
                }

                // Veritabanına kaydedilecek dosya yolu
                etkinlik.EtkinlikResmi = "/resim/" + uniqueFileName;
                Debug.WriteLine($"Resim kaydedildi: {etkinlik.EtkinlikResmi}");
            }

            // Çakışma yoksa etkinliği ekle
            etkinlik.KullanıcıID = kullanıcıId;
            etkinlik.EtkinlikDurum = 1;
            _context.Etkinlikler.Add(etkinlik);

            try
            {
                await _context.SaveChangesAsync();
                // Bildirim oluştur
                var bildirim = new Bildirim
                {
                    KullanıcıID = kullanıcıId,
                    Mesaj = $"Yeni bir etkinlik oluşturuldu: {etkinlik.EtkinlikAdi}",
                    Tarih = DateTime.Now,
                    OkunduMu = false
                };

                _context.Bildirimler.Add(bildirim);
                // Kullanıcıya 15 puan ekle
                var puan = new Puan
                {
                    KullanıcıID = kullanıcıId,
                    Puanlar = 15,
                    KazanilanTarih = DateTime.Now
                };

                _context.Puanlar.Add(puan);
                await _context.SaveChangesAsync();

                Debug.WriteLine("Etkinlik ve puan başarıyla kaydedildi!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Veritabanına kaydetme hatası: " + ex.Message);
            }


            return RedirectToAction("AdminIndex", "Admin");
        }


        [HttpGet]
        public IActionResult GetKategoriler(int ilgiAlaniId)
        {
            try
            {
                // İlgi alanına ait etkinlik kategorilerini çek
                var kategoriler = _context.Kategoriler
                    .Where(k => k.IlgiAlaniID == ilgiAlaniId)
                    .Select(k => new { k.ID, k.Ad })
                    .ToList();

                return Json(kategoriler);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Kategoriler alınırken hata: " + ex.Message);
                return Json(new { success = false, message = "Bir hata oluştu." });
            }
        }
        [HttpGet]
        public IActionResult GetIlgiAlanlari()
        {
            var ilgiAlanlari = _context.IlgiAlanlari
                .Select(ia => new { ia.ID, ia.Ad })
                .ToList();
            return Json(ilgiAlanlari);
        }

        [HttpPost]
        public IActionResult AddIlgiAlani(IlgiAlani ilgiAlani)
        {
            if (string.IsNullOrWhiteSpace(ilgiAlani.Ad))
            {
                TempData["Message"] = "İlgi alanı adı boş olamaz.";
                TempData["MessageType"] = "danger"; // Hata mesajı
                return RedirectToAction("CreateIlgiAlani");
            }

            if (_context.IlgiAlanlari.Any(ia => ia.Ad.ToLower() == ilgiAlani.Ad.ToLower()))
            {
                TempData["Message"] = "Bu ilgi alanı zaten mevcut.";
                TempData["MessageType"] = "warning"; // Uyarı mesajı
                return RedirectToAction("CreateIlgiAlani");
            }

            _context.IlgiAlanlari.Add(ilgiAlani);
            _context.SaveChanges();

            TempData["Message"] = "İlgi alanı başarıyla eklendi.";
            TempData["MessageType"] = "success"; // Başarı mesajı
            return RedirectToAction("CreateIlgiAlani");
        }

        [HttpPost]
        public IActionResult AddKategori(Kategori kategori)
        {
            if (string.IsNullOrWhiteSpace(kategori.Ad))
            {
                TempData["Message"] = "Kategori adı boş olamaz.";
                TempData["MessageType"] = "danger"; // Hata mesajı
                return RedirectToAction("CreateIlgiAlani");
            }

            if (!_context.IlgiAlanlari.Any(ia => ia.ID == kategori.IlgiAlaniID))
            {
                TempData["Message"] = "Geçersiz ilgi alanı ID.";
                TempData["MessageType"] = "danger"; // Hata mesajı
                return RedirectToAction("CreateIlgiAlani");
            }

            if (_context.Kategoriler.Any(k => k.Ad.ToLower() == kategori.Ad.ToLower() && k.IlgiAlaniID == kategori.IlgiAlaniID))
            {
                TempData["Message"] = "Bu kategori zaten mevcut.";
                TempData["MessageType"] = "warning"; // Uyarı mesajı
                return RedirectToAction("CreateIlgiAlani");
            }

            _context.Kategoriler.Add(kategori);
            _context.SaveChanges();

            TempData["Message"] = "Kategori başarıyla eklendi.";
            TempData["MessageType"] = "success"; // Başarı mesajı
            return RedirectToAction("CreateIlgiAlani");
        }



        [HttpPost]
        public IActionResult AddIlgiAlani2(IlgiAlani ilgiAlani)
        {
            if (string.IsNullOrWhiteSpace(ilgiAlani.Ad))
            {
                TempData["Message"] = "İlgi alanı adı boş olamaz.";
                TempData["MessageType"] = "danger"; // Hata mesajı
                return RedirectToAction("AdminCreateIlgiAlani", "Create");
            }

            if (_context.IlgiAlanlari.Any(ia => ia.Ad.ToLower() == ilgiAlani.Ad.ToLower()))
            {
                TempData["Message"] = "Bu ilgi alanı zaten mevcut.";
                TempData["MessageType"] = "warning"; // Uyarı mesajı
                return RedirectToAction("AdminCreateIlgiAlani", "Create");
            }

            _context.IlgiAlanlari.Add(ilgiAlani);
            _context.SaveChanges();

            TempData["Message"] = "İlgi alanı başarıyla eklendi.";
            TempData["MessageType"] = "success"; // Başarı mesajı
            return RedirectToAction("AdminCreateIlgiAlani", "Create");
        }

        [HttpPost]
        public IActionResult AddKategori2(Kategori kategori)
        {
            if (string.IsNullOrWhiteSpace(kategori.Ad))
            {
                TempData["Message"] = "Kategori adı boş olamaz.";
                TempData["MessageType"] = "danger"; // Hata mesajı
                return RedirectToAction("AdminCreateIlgiAlani", "Create");
            }

            if (!_context.IlgiAlanlari.Any(ia => ia.ID == kategori.IlgiAlaniID))
            {
                TempData["Message"] = "Geçersiz ilgi alanı ID.";
                TempData["MessageType"] = "danger"; // Hata mesajı
                return RedirectToAction("AdminCreateIlgiAlani", "Create");
            }

            if (_context.Kategoriler.Any(k => k.Ad.ToLower() == kategori.Ad.ToLower() && k.IlgiAlaniID == kategori.IlgiAlaniID))
            {
                TempData["Message"] = "Bu kategori zaten mevcut.";
                TempData["MessageType"] = "warning"; // Uyarı mesajı
                return RedirectToAction("AdminCreateIlgiAlani", "Create");
            }

            _context.Kategoriler.Add(kategori);
            _context.SaveChanges();

            TempData["Message"] = "Kategori başarıyla eklendi.";
            TempData["MessageType"] = "success"; // Başarı mesajı
            return RedirectToAction("AdminCreateIlgiAlani", "Create");
        }




        public IActionResult CreateIlgiAlani()
        {
            var ilgiAlanlari = _context.IlgiAlanlari.ToList();

            if (ilgiAlanlari == null || !ilgiAlanlari.Any())
            {
                Debug.WriteLine("İlgi alanları boş veya null!");
            }
            else
            {
                Debug.WriteLine($"İlgi alanları yüklendi: {ilgiAlanlari.Count} kayıt bulundu.");
            }

            ViewBag.IlgiAlanlari = ilgiAlanlari ?? new List<IlgiAlani>();

            return View();
        }


        public IActionResult AdminCreateIlgiAlani()
        {
            var ilgiAlanlari = _context.IlgiAlanlari.ToList();

            if (ilgiAlanlari == null || !ilgiAlanlari.Any())
            {
                Debug.WriteLine("İlgi alanları boş veya null!");
            }
            else
            {
                Debug.WriteLine($"İlgi alanları yüklendi: {ilgiAlanlari.Count} kayıt bulundu.");
            }

            ViewBag.IlgiAlanlari = ilgiAlanlari ?? new List<IlgiAlani>();

            return View();
        }





    }
}