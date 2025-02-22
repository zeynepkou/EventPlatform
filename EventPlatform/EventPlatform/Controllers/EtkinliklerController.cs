using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using Yazlab2.Data;
using Yazlab2.Models;

namespace Yazlab2.Controllers
{
    public class EtkinliklerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EtkinliklerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tüm Etkinlikleri Listele
        public IActionResult Index()
        {
            var bugun = DateTime.Now;

            // Kullanıcı ID'sini al
            var kullanıcıIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var kullanıcıId = kullanıcıIdClaim != null ? int.Parse(kullanıcıIdClaim.Value) : 0;

            // İlgi alanları ve kategorileri ViewBag'e ekle
            ViewBag.IlgiAlanlari = _context.IlgiAlanlari.ToList();

            var etkinlikler = _context.Etkinlikler
      .Include(e => e.Favoriler)
      .AsEnumerable() // Bellek tarafında işleme geçiş
      .Where(e =>
          e.Tarih.Add(e.Saat) > bugun && // TimeSpan doğrudan Add() ile eklenir
          (e.EtkinlikDurum == 1 || e.EtkinlikDurum == 3))
      .ToList();
      

    // Kategoriler (ilk başta boş olabilir)
    ViewBag.Kategoriler = new List<object>();

            ViewBag.KullanıcıID = kullanıcıId; // Kullanıcı ID'sini ViewBag ile gönderin

            return View(etkinlikler);
        }

        [HttpGet]
        public IActionResult Filtrele(int? kategoriId)
        {
            // Etkinlikleri bugünden itibaren olanları alarak başlat
            var etkinlikler = _context.Etkinlikler
                .Where(e => e.Tarih >= DateTime.Today)
                .AsQueryable();

            // İlgi alanlarını ViewBag'e ekleyin
            ViewBag.IlgiAlanlari = _context.IlgiAlanlari.ToList();

            // Kategori ID'ye göre filtreleme
            if (kategoriId.HasValue)
            {
                etkinlikler = etkinlikler.Where(e => e.KategoriID == kategoriId.Value);
            }

            // Etkinlikleri listeye çevir ve Index view'ına gönder
            return View("Index", etkinlikler.ToList());
        }

        [HttpGet]
        public IActionResult Ara(string arama)
        {
            // Etkinlikleri bugünden itibaren olanları alarak başlat
            var etkinlikler = _context.Etkinlikler
                .Where(e => e.Tarih >= DateTime.Today)
                .AsQueryable();

            // İlgi alanlarını ViewBag'e ekleyin
            ViewBag.IlgiAlanlari = _context.IlgiAlanlari.ToList();

            if (!string.IsNullOrEmpty(arama))
            {
                // Etkinlik adını arama metnini içeren sonuçları filtrele
                etkinlikler = etkinlikler.Where(e => e.EtkinlikAdi.Contains(arama));
            }

            // Etkinlikleri listeye çevir ve Index view'ına gönder
            return View("Index", etkinlikler.ToList());
        }



        [HttpGet]
        public IActionResult GetKategoriler(int ilgiAlaniId)
        {
            if (ilgiAlaniId <= 0)
            {
                return BadRequest("Geçersiz ilgi alanı ID");
            }
            

            var kategoriler = _context.Kategoriler
                .Where(k => k.IlgiAlaniID == ilgiAlaniId) // İlgi alanı ID'sine göre filtreleme
                .Select(k => new
                {
                    k.ID,
                    k.Ad
                })
                .ToList();
            Debug.WriteLine($"ilgi alanı ID: {ilgiAlaniId}");
            Debug.WriteLine(string.Join(", ", kategoriler.Select(k => $"ID: {k.ID}, Ad: {k.Ad}")));
            return Json(kategoriler); // JSON formatında döndür
        }


        [HttpPost]
        public IActionResult FavoriEkle(int etkinlikId)
        {
            var kullanıcıIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (kullanıcıIdClaim == null)
            {
                return Json(new { success = false, message = "Kullanıcı giriş yapmamış." });
            }

            var kullanıcıId = int.Parse(kullanıcıIdClaim.Value);
            var favori = _context.Favoriler.FirstOrDefault(f => f.KullanıcıID == kullanıcıId && f.EtkinlikID == etkinlikId);

            var etkinlik = _context.Etkinlikler.FirstOrDefault(e => e.ID == etkinlikId);
            if (etkinlik == null)
            {
                return Json(new { success = false, message = "Etkinlik bulunamadı." });
            }

            bool isFavorite;

            if (favori != null)
            {
                // Favoriyi kaldır
                _context.Favoriler.Remove(favori);
                etkinlik.FavoriSayisi -= 1;
                isFavorite = false;
            }
            else
            {
                // Favori ekle
                favori = new Favori
                {
                    KullanıcıID = kullanıcıId,
                    EtkinlikID = etkinlikId,
                    FavoriEklemeTarihi = DateTime.Now
                };
                _context.Favoriler.Add(favori);
                etkinlik.FavoriSayisi += 1;
                isFavorite = true;

                var bildirim = new Bildirim
                {
                    KullanıcıID = kullanıcıId,
                    Mesaj = $"Etkinlik favorilere eklendi: {etkinlik.EtkinlikAdi}",
                    Tarih = DateTime.Now,
                    OkunduMu = false
                };

                _context.Bildirimler.Add(bildirim);
            }

            _context.SaveChanges();

            return Json(new { success = true, favoriSayisi = etkinlik.FavoriSayisi, isFavorite });
        }

        public IActionResult KullanıcıEtkinlikleri()
        {
            var kullanıcıIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (kullanıcıIdClaim == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var kullanıcıId = int.Parse(kullanıcıIdClaim.Value);

            var etkinlikler = _context.Etkinlikler
                .Include(e => e.Favoriler)
                .Where(e => e.KullanıcıID == kullanıcıId)
                .ToList();

            ViewBag.FavoriDurumlari = etkinlikler.ToDictionary(
                e => e.ID,
                e => e.Favoriler.Any(f => f.KullanıcıID == kullanıcıId)
            );

            return View(etkinlikler);
        }

        public IActionResult AdminEtkinlik()
        {
            var bugun = DateTime.Now;
            // İlgi alanları ve kategorileri ViewBag'e ekle
            ViewBag.IlgiAlanlari = _context.IlgiAlanlari.ToList();

            // Kategoriler (ilk başta boş olabilir)
            ViewBag.Kategoriler = new List<object>();

            // Etkinlikleri al
            var etkinlikler = _context.Etkinlikler
                .AsEnumerable() // Sorguyu belleğe çek
                .Where(e => (e.Tarih + e.Saat > bugun || e.Tarih + e.Saat < bugun) && (e.EtkinlikDurum == 1 || e.EtkinlikDurum == 3)) // Tarihi geçmemiş, geçmiş ve durum 1 veya 3
                .ToList();

            return View(etkinlikler); // AdminEtkinlik.cshtml görünümüne etkinlikler modelini gönderiyoruz.
        }


        [HttpGet]
        public IActionResult FiltreleAdmin(int? kategoriId)
        {
            // Etkinlikleri başlat
            var etkinlikler = _context.Etkinlikler.AsQueryable();

            // İlgi alanlarını ViewBag'e ekleyin
            ViewBag.IlgiAlanlari = _context.IlgiAlanlari.ToList();

            // Kategori ID'ye göre filtreleme
            if (kategoriId.HasValue)
            {
                etkinlikler = etkinlikler.Where(e => e.KategoriID == kategoriId.Value);
            }

            // Etkinlikleri listeye çevir ve Index view'ına gönder
            return View("AdminEtkinlik", etkinlikler.ToList());
        }
        [HttpGet]
        public IActionResult Ara2(string arama)
        {
            // Eğer arama parametresi boş veya null ise, tüm etkinlikleri getirir
            var etkinlikler = _context.Etkinlikler.AsQueryable();

            // İlgi alanlarını ViewBag'e ekleyin
            ViewBag.IlgiAlanlari = _context.IlgiAlanlari.ToList();

            if (!string.IsNullOrEmpty(arama))
            {
                // Etkinlik adını arama metnini içeren sonuçları filtrele
                etkinlikler = etkinlikler.Where(e => e.EtkinlikAdi.Contains(arama));
            }

            // Etkinlikleri listeye çevir ve Index view'ına gönder
            return View("AdminEtkinlik", etkinlikler.ToList());
        }

        [HttpGet]
        public IActionResult Favoriler()
        {
            // Kullanıcı ID'sini al
            var kullanıcıIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (kullanıcıIdClaim == null)
            {
                return RedirectToAction("Index", "Home"); // Kullanıcı giriş yapmamışsa ana sayfaya yönlendir
            }

            var kullanıcıId = int.Parse(kullanıcıIdClaim.Value);

            // Kullanıcının favorilediği etkinlikleri al
            var favoriler = _context.Favoriler
                .Include(f => f.Etkinlik)
                .ThenInclude(e => e.Favoriler) // Etkinliğin favori bilgilerini de yükle
                .Where(f => f.KullanıcıID == kullanıcıId)
                .Select(f => f.Etkinlik)
                .ToList();

            ViewBag.KullanıcıID = kullanıcıId; // Kullanıcı ID'sini ViewBag ile gönder

            return View(favoriler);
        }
    }
}
