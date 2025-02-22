using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Yazlab2.Data;
using Yazlab2.Models;

namespace Yazlab2.Controllers
{

    public class EtkinlikDetayController : Controller
    {
        private readonly ApplicationDbContext _context;


        public EtkinlikDetayController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Etkinlik Detaylarını Göster
        public IActionResult Index(int id)
        {
            // Kullanıcının kimliğini al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var etkinlik = _context.Etkinlikler
     .Include(e => e.Mesajlar)
         .ThenInclude(m => m.Gonderici) // Mesajların Gonderici bilgisini dahil et
     .Include(e => e.Favoriler) // Favoriler dahil
     .Include(e => e.Kategori) // Kategori bilgisi dahil
     .FirstOrDefault(e => e.ID == id);

            // Kategori Adını almak için
            var kategoriAdi = etkinlik?.Kategori?.Ad;


            if (etkinlik == null)
            {
                return NotFound();
            }

            // Kullanıcının favorilerde olup olmadığını kontrol et
            var isFavori = etkinlik.Favoriler.Any(f => f.KullanıcıID == int.Parse(userId));
            ViewBag.IsFavori = isFavori; // Favori durumu ViewBag ile gönder

            // Kullanıcının katıldığı etkinlikler
            var isUserRegistered = _context.Katılımcılar
                .Any(k => k.KullanıcıID == int.Parse(userId) && k.EtkinlikID == id);

            // Etkinlik geçmişte mi kontrol et
            var isEventInPast = etkinlik.Tarih.Add(etkinlik.Saat) < DateTime.Now;

            // Eğer etkinlik geçmişteyse ve kullanıcı kayıtlıysa geri bildirimler ve yıldız puan bilgileri için ek verileri çek
            if (isUserRegistered && isEventInPast)
            {
                etkinlik = _context.Etkinlikler
                    .Include(e => e.Mesajlar)
                    .Include(e => e.GeriBildirimler)
                    .ThenInclude(gb => gb.Gonderici)
                    .FirstOrDefault(e => e.ID == id);

                if (etkinlik != null)
                {
                    // Kullanıcının yıldız puanını sorgula
                    var userIdInt = int.Parse(userId);
                    var kullaniciPuani = _context.YildizPuanlar
                        .FirstOrDefault(yp => yp.EtkinlikID == id && yp.KullanıcıID == userIdInt)?.Puan ?? 0;

                    // Kullanıcı puanını ViewBag'e ekle
                    ViewBag.KullaniciPuani = kullaniciPuani;
                }

                return View("EtkinlikBildirim", etkinlik);
            }

            // ViewBag ile bilgileri gönder
            ViewBag.IsUserRegistered = isUserRegistered;
            ViewBag.IsEventInPast = isEventInPast;

            // Aksi takdirde standart görünümü döndür
            return View("Index", etkinlik);
        }

        public IActionResult AdminEtkinlikDetay(int id)
        {
            // Etkinliği ID'ye göre al
            var etkinlik = _context.Etkinlikler
                  .Include(e => e.Mesajlar)
         .ThenInclude(m => m.Gonderici) // Mesajların Gonderici bilgisini dahil et
                    .Include(e => e.GeriBildirimler)
                    .ThenInclude(gb => gb.Gonderici) // Mesajlar gibi ilişkili verileri ekleyin
                .Include(e => e.Kategori)
                .FirstOrDefault(e => e.ID == id);

            if (etkinlik == null)
            {
                return NotFound("Etkinlik bulunamadı.");
            }

            // Eğer etkinlik tarihi geçmişse
            if (etkinlik.Tarih < DateTime.Now)
            {
                // AdminGecmisEtkinlik görünümüne yönlendir
                return View("AdminGecmisEtkinlik", etkinlik);
            }

            // Aksi halde AdminEtkinlikDetay görünümünü döndür
            return View("AdminEtkinlikDetay", etkinlik);
        }



        [HttpGet]
        public IActionResult GetUserLocation()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Json(new { success = false, message = "Kullanıcı oturum açmamış veya kimlik bilgisi eksik." });
            }

            var user = _context.Kullanıcılar.FirstOrDefault(k => k.ID == int.Parse(userId));
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            if (string.IsNullOrEmpty(user.Konum) || !user.Konum.Contains(",") || user.Konum.Split(',').Length != 2)
            {
                return Json(new { success = false, message = "Geçersiz konum formatı veya konum eksik." });
            }

            return Json(new { success = true, location = user.Konum });
        }



        public IActionResult Register(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = int.Parse(userIdClaim.Value);

            // Etkinliği al
            var etkinlik = _context.Etkinlikler
                .Include(e => e.Katılımcılar)
                .FirstOrDefault(e => e.ID == id);

            if (etkinlik == null)
            {
                return NotFound("Etkinlik bulunamadı.");
            }

            // Kullanıcının katılımcı olup olmadığını kontrol et
            var mevcutKatilimci = etkinlik.Katılımcılar.FirstOrDefault(k => k.KullanıcıID == userId);

            if (mevcutKatilimci != null)
            {
                // Kullanıcı zaten katılımcıysa kaydı iptal et
                _context.Katılımcılar.Remove(mevcutKatilimci);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Etkinlik kaydınız başarıyla iptal edildi.";
                return RedirectToAction("Index", new { id = id });
            }

            // Etkinlik başlangıç ve bitiş saatlerini hesapla
            var etkinlikBaslangic = etkinlik.Tarih.Add(etkinlik.Saat);
            var etkinlikBitis = etkinlikBaslangic.AddMinutes(etkinlik.EtkinlikSuresi.TotalMinutes);

            // Kullanıcının katıldığı etkinlikleri kontrol et
            var mevcutKatilimciEtkinlikler = _context.Katılımcılar
                .Where(k => k.KullanıcıID == userId)
                .Select(k => k.EtkinlikID)
            .ToList();

            foreach (var etkinlikID in mevcutKatilimciEtkinlikler)
            {
                var mevcutEtkinlik = _context.Etkinlikler.FirstOrDefault(e => e.ID == etkinlikID);

                if (mevcutEtkinlik != null)
                {
                    var mevcutEtkinlikBaslangic = mevcutEtkinlik.Tarih.Add(mevcutEtkinlik.Saat);
                    var mevcutEtkinlikBitis = mevcutEtkinlikBaslangic.AddMinutes(mevcutEtkinlik.EtkinlikSuresi.TotalMinutes);

                    // Çakışma kontrolü
                    if ((etkinlikBaslangic < mevcutEtkinlikBitis) && (etkinlikBitis > mevcutEtkinlikBaslangic))
                    {
                        // Çakışmayan alternatif etkinlikleri sorgula
                        var alternatifEtkinlikler = _context.Etkinlikler
                            .Where(e => e.Tarih == etkinlik.Tarih && e.ID != etkinlik.ID) // Aynı gün ve farklı etkinlikler
                            .AsEnumerable()
                            .Where(e =>
                            {
                                var alternatifBaslangic = e.Tarih.Add(e.Saat);
                                var alternatifBitis = alternatifBaslangic.AddMinutes(e.EtkinlikSuresi.TotalMinutes);
                                return alternatifBitis <= etkinlikBaslangic || alternatifBaslangic >= etkinlikBitis;
                            })
                            .ToList();

                        // Kullanıcının katılmadığı alternatif etkinlikleri filtrele
                        var katilimciEtkinlikler = _context.Katılımcılar
                            .Where(k => k.KullanıcıID == userId)
                            .Select(k => k.EtkinlikID)
                            .ToList();

                        // Kullanıcının katılmadığı etkinlikleri filtrele
                        var alternatifKatilimciEtkinlikler = alternatifEtkinlikler
                            .Where(e => !katilimciEtkinlikler.Contains(e.ID))
                            .ToList();

                        if (alternatifKatilimciEtkinlikler.Any())
                        {
                            TempData["ConflictMessage"] = "Bu etkinlik bir başka etkinlik ile çakışıyor. Alternatif etkinlikler aşağıda listelenmiştir.";
                            return View("Conflict", alternatifKatilimciEtkinlikler); // Alternatif etkinlikleri görünüme gönder
                        }
                        else
                        {
                            TempData["ConflictMessage"] = "Bu etkinlik bir başka etkinlik ile çakışıyor, fakat uygun alternatif etkinlik bulunmamaktadır.";
                            return View("Conflict");
                        }


                    }
                }
            }

            // Çakışma yoksa katılım eklenir
            var katilimci = new Katılımcı
            {
                EtkinlikID = etkinlik.ID,
                KullanıcıID = userId
            };

            _context.Katılımcılar.Add(katilimci);

            // Kullanıcının daha önce etkinliğe katılıp katılmadığını kontrol et
            var dahaOnceKatilimYapmisMi = _context.Puanlar.Any(p => p.KullanıcıID == userId);

            // Puanı belirle (ilk kez katılım için 20, aksi takdirde 10 puan)
            var yeniPuan = new Puan
            {
                KullanıcıID = userId,
                Puanlar = dahaOnceKatilimYapmisMi ? 10 : 20, // "Puanlar" sütununa göre
                KazanilanTarih = DateTime.Now
            };

            _context.Puanlar.Add(yeniPuan);

            // Bildirim oluştur
            var bildirim = new Bildirim
            {
                KullanıcıID = userId,
                Mesaj = $"'{etkinlik.EtkinlikAdi}' adlı etkinliğe kaydınız başarıyla tamamlandı.",
                Tarih = DateTime.Now,
                OkunduMu = false
            };

            _context.Bildirimler.Add(bildirim);

            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult AddMesaj(int etkinlikId, string mesajIcerik)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Kullanıcıyı veritabanından al
            var kullanıcı = _context.Kullanıcılar.FirstOrDefault(k => k.ID == userId);
            if (!_context.Kullanıcılar.Any(u => u.ID == userId))
            {
                return RedirectToAction("Index", new { id = etkinlikId });
            }

            var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.ID == etkinlikId);
            if (etkinlik == null)
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı.";
                return RedirectToAction("Index");
            }

            var mesaj = new Mesaj
            {
                GondericiID = userId,
                EtkinlikID = etkinlikId,
                MesajMetni = mesajIcerik,
                GonderimZamani = DateTime.Now
            };

            _context.Mesajlar.Add(mesaj);

            // Katılımcılara bildirim gönder
            var katilimcilar = _context.Katılımcılar.Where(k => k.EtkinlikID == etkinlikId).ToList();
            foreach (var katilimci in katilimcilar)
            {
                var bildirim = new Bildirim
                {
                    KullanıcıID = katilimci.KullanıcıID,
                    Mesaj = $"'{etkinlikId}' ID'li etkinliğe yeni bir mesaj eklendi: '{mesajIcerik}'.",
                    Tarih = DateTime.Now,
                    OkunduMu = false
                };
                _context.Bildirimler.Add(bildirim);
            }

            _context.SaveChanges();

            // Tarihe göre yönlendirme
            if (etkinlik.Tarih > DateTime.Now)
            {
                return RedirectToAction("Index", new { id = etkinlikId });
            }
            else
            {
                

                // Eğer kullanıcı bulunamazsa hata sayfasına yönlendir
                if (kullanıcı == null)
                {
                    return View("Error");
                }

                // Kullanıcı türünü kontrol et
                if (kullanıcı.KullaniciTuru == "admin")
                {
                    return RedirectToAction("AdminGecmisEtkinlik", new { id = etkinlikId });
                }
                else
                {
                    return RedirectToAction("Index", new { id = etkinlikId });
                }

            }
        }

        [HttpPost]
        public IActionResult AddGeriBildirim(int etkinlikId, string geriBildirimMetni)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var kullanıcı = _context.Kullanıcılar.FirstOrDefault(k => k.ID.ToString() == userId);
            if (userId == null || string.IsNullOrEmpty(geriBildirimMetni))
            {
                TempData["ErrorMessage"] = "Geri bildirim gönderilemedi.";
                return RedirectToAction("Index", new { id = etkinlikId });
            }

            var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.ID == etkinlikId);
            if (etkinlik == null)
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı.";
                return RedirectToAction("Index");
            }

            // Geri Bildirim Kaydetme
            var geriBildirim = new GeriBildirim
            {
                GondericiID = int.Parse(userId),
                KullanıcıID = int.Parse(userId),
                EtkinlikID = etkinlikId,
                GeriBidirimMetni = geriBildirimMetni,
                GonderimZamani = DateTime.Now,
                GeriBildirimDurum = 1 // Varsayılan durum
            };

            _context.GeriBildirimler.Add(geriBildirim);
            _context.SaveChanges();

            // Bildirim Oluşturma
            var bildirim = new Bildirim
            {
                KullanıcıID = int.Parse(userId),
                Mesaj = $"Etkinlik ID: {etkinlikId} için bir geri bildirim gönderildi.",
                Tarih = DateTime.Now,
                OkunduMu = false
            };

            _context.Bildirimler.Add(bildirim);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Geri bildiriminiz başarıyla gönderildi ve bildirim oluşturuldu.";

            // Tarihe göre yönlendirme
            if (etkinlik.Tarih > DateTime.Now)
            {
                return RedirectToAction("Index", new { id = etkinlikId });
            }
            else
            {
                // Eğer kullanıcı bulunamazsa hata sayfasına yönlendir
                if (kullanıcı == null)
                {
                    return View("Error");
                }

                // Kullanıcı türünü kontrol et
                if (kullanıcı.KullaniciTuru == "admin")
                {
                    return RedirectToAction("AdminGecmisEtkinlik", new { id = etkinlikId });
                }
                else
                {
                    return RedirectToAction("Index", new { id = etkinlikId });
                }

            }
        }




        // Etkinlik düzenleme sayfasına yönlendiren action
        public IActionResult EditEtkinlik(int id)
        {
            var etkinlik = _context.Etkinlikler
                .Include(e => e.Kategori) // Kategori bilgisini dahil et
                .FirstOrDefault(e => e.ID == id);

            if (etkinlik == null)
            {
                return NotFound();
            }

            ViewBag.Kategoriler = _context.Kategoriler
     .Select(k => new SelectListItem
     {
         Value = k.ID.ToString(),  // Kategori ID
         Text = k.Ad              // Kategori Adı
     }).ToList();

            return View(etkinlik);
        }


        [HttpPost]
[ValidateAntiForgeryToken]
public IActionResult EditEtkinlik(int id, Etkinlik updatedEtkinlik)
{
    if (id != updatedEtkinlik.ID)
    {
        return NotFound();
    }

    // Veritabanındaki mevcut etkinliği al
    var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.ID == id);

    if (etkinlik == null)
    {
        return NotFound();
    }

    // Etkinlik bilgilerini güncelle
    etkinlik.EtkinlikAdi = updatedEtkinlik.EtkinlikAdi;
    etkinlik.Aciklama = updatedEtkinlik.Aciklama;
    etkinlik.Tarih = updatedEtkinlik.Tarih;
    etkinlik.Saat = updatedEtkinlik.Saat;
    etkinlik.EtkinlikSuresi = updatedEtkinlik.EtkinlikSuresi;
    etkinlik.Konum = updatedEtkinlik.Konum;

            // ComboBox'tan seçilen kategori ID'sini güncelle
            etkinlik.KategoriID = updatedEtkinlik.KategoriID;
            Debug.WriteLine($"Updated KategoriID: {updatedEtkinlik.KategoriID}");
            Debug.WriteLine($"Etkinlik KategoriID: {etkinlik.KategoriID}");



            try
            {
        // Bildirim oluştur
        var bildirim = new Bildirim
        {
            KullanıcıID = etkinlik.KullanıcıID,
            Mesaj = $"Etkinlik başarıyla düzenlendi: {etkinlik.EtkinlikAdi}",
            Tarih = DateTime.Now,
            OkunduMu = false
        };

        _context.Bildirimler.Add(bildirim);

        // Değişiklikleri kaydet
        _context.Update(etkinlik);
        _context.SaveChanges();

        // Etkinlik güncelleme başarılı ise liste sayfasına yönlendir
        return RedirectToAction("Index", new { id = etkinlik.ID });
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!_context.Etkinlikler.Any(e => e.ID == id))
        {
            return NotFound();
        }
        else
        {
            throw;
        }
    }
}


        [HttpPost]
        public IActionResult AddYildizPuan(int etkinlikId, int yildizPuan)
        {
            // Kullanıcı kimliğini al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || yildizPuan < 1 || yildizPuan > 5)
            {
                TempData["ErrorMessage"] = "Yıldız puanı gönderilemedi.";
                return RedirectToAction("Index", new { id = etkinlikId });
            }

            var userIdInt = int.Parse(userId);

            // Kullanıcının daha önce bu etkinliğe yıldız puanı verip vermediğini kontrol et
            var mevcutYildizPuan = _context.YildizPuanlar
                .FirstOrDefault(yp => yp.EtkinlikID == etkinlikId && yp.KullanıcıID == userIdInt);

            if (mevcutYildizPuan != null)
            {
                // Daha önce puan verdiyse güncelle
                mevcutYildizPuan.Puan = yildizPuan;
                mevcutYildizPuan.GonderimZamani = DateTime.Now;
                _context.Update(mevcutYildizPuan);
            }
            else
            {
                // İlk kez puan veriyorsa yeni kayıt oluştur
                var yeniYildizPuan = new YildizPuan
                {
                    KullanıcıID = userIdInt,
                    EtkinlikID = etkinlikId,
                    Puan = yildizPuan,
                    GonderimZamani = DateTime.Now
                };
                _context.YildizPuanlar.Add(yeniYildizPuan);
            }

            // Etkinlik bilgilerini güncelle
            var etkinlik = _context.Etkinlikler
                .Include(e => e.YildizPuanlar) // Yıldız puanlarını da getir
                .FirstOrDefault(e => e.ID == etkinlikId);

            if (etkinlik != null)
            {
                etkinlik.ToplamYildizPuani = etkinlik.YildizPuanlar.Sum(yp => yp.Puan);
                etkinlik.YildizPuaniSayisi = etkinlik.YildizPuanlar.Count();
                etkinlik.OrtalamaPuan = etkinlik.YildizPuaniSayisi > 0
                    ? (double)etkinlik.ToplamYildizPuani / etkinlik.YildizPuaniSayisi
                    : 0;

                _context.Update(etkinlik);
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Puanınız başarıyla kaydedildi.";
            return RedirectToAction("Index", new { id = etkinlikId });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            // Etkinlik veritabanında mevcut mu kontrol et
            var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.ID == id);

            if (etkinlik == null)
            {
                return NotFound();
            }

            // Etkinliğe katılımcı atanmış mı kontrol et
            var katilimciSayisi = _context.Katılımcılar.Count(k => k.EtkinlikID == id);

            if (katilimciSayisi > 0)
            {
                // Katılımcısı olan etkinlik silinemez, hata mesajı ekle
                TempData["ErrorMessage"] = "Katılımcısı olan etkinlik silinemez.";
                Debug.WriteLine("Katılımcısı olan etkinlik silinemez.");

                return RedirectToAction("KullanıcıEtkinlikleri", "Etkinlikler"); // Burada istediğiniz bir sayfaya yönlendirebilirsiniz


            }

            else
            {
                // Etkinliği sil
                _context.Etkinlikler.Remove(etkinlik);
                _context.SaveChanges();
            }
            // Silme işlemi başarılı, yönlendirme
            return RedirectToAction("Index", "Home"); // Burada istediğiniz bir sayfaya yönlendirebilirsiniz

        }


        public IActionResult AdminEditEtkinlik(int id)
        {
            // Veritabanından etkinliği bul
            var etkinlik = _context.Etkinlikler.
                Include(e => e.Kategori)
                .FirstOrDefault(e => e.ID == id);
            // Kategoriler listesini View'e gönder
            ViewBag.Kategoriler = _context.Kategoriler.ToList();
            if (etkinlik == null)
            {
                // Etkinlik bulunamazsa 404 döndür
                return NotFound();
            }
            ViewBag.Kategoriler = _context.Kategoriler
          .Select(k => new SelectListItem
          {
              Value = k.ID.ToString(),  // Kategori ID
              Text = k.Ad              // Kategori Adı
          }).ToList();
            return View(etkinlik); // Etkinliği düzenleme View'ına gönder
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminEditEtkinlik(Etkinlik model, IFormFile? yeniEtkinlikResmi)
        {
            // Veritabanından etkinliği bul
            var etkinlik = _context.Etkinlikler
                .Include(e => e.Katılımcılar)
                .Include(e => e.Kategori)
                .FirstOrDefault(e => e.ID == model.ID);

            // Kategoriler listesini View'e gönder
            ViewBag.Kategoriler = _context.Kategoriler.ToList();

            if (etkinlik == null)
            {
                return NotFound();
            }

            // Eğer yeni bir resim yüklenmişse
            if (yeniEtkinlikResmi != null && yeniEtkinlikResmi.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/etkinlikler");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Yeni dosya adı
                var fileName = $"{Guid.NewGuid()}_{yeniEtkinlikResmi.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Dosyayı kaydet
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    yeniEtkinlikResmi.CopyTo(stream);
                }

                // Eski resmi sil
                if (!string.IsNullOrEmpty(etkinlik.EtkinlikResmi))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, etkinlik.EtkinlikResmi);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Yeni dosya yolunu kaydet
                etkinlik.EtkinlikResmi = $"/images/etkinlikler/{fileName}";
            }

            // Etkinlik bilgilerini güncelle
            etkinlik.EtkinlikAdi = model.EtkinlikAdi;
            etkinlik.Aciklama = model.Aciklama;
            etkinlik.Tarih = model.Tarih;
            etkinlik.Saat = model.Saat;
            etkinlik.Konum = model.Konum;
            etkinlik.EtkinlikSuresi = model.EtkinlikSuresi;
            etkinlik.KategoriID = model.KategoriID;

            // Tüm katılımcılara bildirim oluştur
            foreach (var katilimci in etkinlik.Katılımcılar)
            {
                var bildirim = new Bildirim
                {
                    KullanıcıID = katilimci.KullanıcıID,
                    Mesaj = $"'{etkinlik.EtkinlikAdi}' adlı etkinlikte düzenleme yapıldı. Detayları kontrol edebilirsiniz.",
                    Tarih = DateTime.Now,
                    OkunduMu = false
                };
                _context.Bildirimler.Add(bildirim);
            }

            // Değişiklikleri kaydet
            _context.SaveChanges();

            return RedirectToAction("AdminIndex", "Admin");
        }

        public IActionResult AdminGecmisEtkinlik(int id)
        {
            // Kullanıcının kimliğini al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Etkinlik bilgilerini getir
            var etkinlik = _context.Etkinlikler
                .Include(e => e.Mesajlar)
                    .Include(e => e.GeriBildirimler)
                    .ThenInclude(gb => gb.Gonderici)
                .Include(e => e.Kategori) // Kategori bilgisi dahil
                .FirstOrDefault(e => e.ID == id);
            
            if (etkinlik == null)
            {
                return NotFound();
            }

            // Etkinlik geçmişte mi kontrol et
            var isEventInPast = etkinlik.Tarih.Add(etkinlik.Saat) < DateTime.Now;
            if (!isEventInPast)
            {
                TempData["ErrorMessage"] = "Bu etkinlik geçmişte değil.";
                return RedirectToAction("Index", new { id });
            }

            // Admin geçmiş etkinlik işlemleri için ek veriler
            var geriBildirimler = _context.GeriBildirimler
                .Where(gb => gb.EtkinlikID == id)
                .Include(gb => gb.Gonderici)
                .ToList();

            var mesajlar = _context.Mesajlar
                .Where(m => m.EtkinlikID == id)
                .Include(m => m.Gonderici)
                .ToList();

            // Tüm geri bildirimleri ve mesajları görüntülemek için ViewBag'e ekle
            ViewBag.GeriBildirimler = geriBildirimler;
            ViewBag.Mesajlar = mesajlar;

            // Yıldız puanlarını getir
            var yildizPuanlar = _context.YildizPuanlar
                .Where(yp => yp.EtkinlikID == id)
                .Include(yp => yp.Kullanıcı)
                .ToList();

            // Yıldız puanları ViewBag'e ekle
            ViewBag.YildizPuanlar = yildizPuanlar;

            // Etkinlik detaylarını View'e gönder
            return View("AdminGecmisEtkinlik", etkinlik);
        }


    }
}