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
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        
        
        // Tüm baðýmlýlýklarý tek bir kurucuda toplayýn
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                // Kullanýcý verilerini alýn
                var kullaniciVerileri = _context.Kullanýcýlar
                    .Include(k => k.KullaniciIlgiAlanlari) // Ýlgi alanlarýný yüklemek için Include
                    .ThenInclude(ia => ia.IlgiAlani)
                    .FirstOrDefault(u => u.ID == int.Parse(userId));

                if (kullaniciVerileri == null)
                {
                    return RedirectToAction("Error", "Home");
                }

                ViewBag.KullaniciVerileri = kullaniciVerileri;
                ViewBag.KullaniciPuan = CalculateUserPoints(int.Parse(userId));
                ViewBag.Bildirimler = _context.Bildirimler
      .Where(b => b.KullanýcýID == int.Parse(userId) && b.OkunduMu == false)
      .OrderByDescending(b => b.Tarih)
      .ToList();

                ViewBag.BildirimSayisi = ViewBag.Bildirimler.Count;

                // Ýlgi alanýna göre etkinlikler
                var kullaniciIlgiAlanlari = kullaniciVerileri.KullaniciIlgiAlanlari.Select(ia => ia.IlgiAlani.ID).ToList();
                var ilgiAlaninaGoreEtkinlikler = _context.Etkinlikler
                    .Include(e => e.Kategori)
                    .Where(e => kullaniciIlgiAlanlari.Contains(e.Kategori.IlgiAlaniID))
                    .ToList();

                ViewBag.IlgiAlaninaGoreEtkinlikler = ilgiAlaninaGoreEtkinlikler;


                // Kullanýcýnýn katýldýðý etkinlikleri al
                var kullaniciEtkinlikleri = _context.Katýlýmcýlar
                    .Where(k => k.KullanýcýID == int.Parse(userId))
                    .Include(k => k.Etkinlik)
                    .ThenInclude(e => e.Kategori)
                    .ToList();

                // Kullanýcýnýn katýldýðý etkinliklerin kategorilerini bul
                var katildigiKategoriIds = kullaniciEtkinlikleri
                    .Select(k => k.Etkinlik.Kategori.ID)
                    .Distinct()
                    .ToList();

                // Bu kategorilere ait ilgi alanlarýný bul
                var katildigiIlgiAlaniIds = _context.Kategoriler
                    .Where(k => katildigiKategoriIds.Contains(k.ID))
                    .Select(k => k.IlgiAlaniID)
                    .Distinct()
                    .ToList();

                // Bu ilgi alanlarýna ait tüm kategorilerdeki etkinlikleri al
                var ilgiliEtkinlikler = _context.Etkinlikler
                    .Include(e => e.Kategori)
                    .Where(e => katildigiIlgiAlaniIds.Contains(e.Kategori.IlgiAlaniID) &&
                                !kullaniciEtkinlikleri.Select(ke => ke.Etkinlik.ID).Contains(e.ID)) // Ayný etkinlik hariç
                    .ToList();

                // Önerilen etkinlikleri ViewBag'e aktar
                ViewBag.KatilimGecmisineGoreEtkinlikler = ilgiliEtkinlikler;

                // Favori durumlarýný kontrol et
                var favorietkinlikler = _context.Etkinlikler
                    .Include(e => e.Favoriler)
                    .ToList();

                // Favori durumlarýný Dictionary olarak oluþtur ve ViewBag'e aktar
                ViewBag.FavoriDurumlari = favorietkinlikler.ToDictionary(
                    e => e.ID,
                    e => e.Favoriler.Any(f => f.KullanýcýID == int.Parse(userId))
                );

                ViewBag.KullanýcýID = int.Parse(userId);

                // Kullanýcýnýn konum bilgisini alýyoruz
                var kullaniciKonum = kullaniciVerileri.Konum; // "Lat:41.01866120303227,Lng:29.09544067498994" þeklinde

                if (!string.IsNullOrEmpty(kullaniciKonum))
                {
                    var konumParcalari = kullaniciKonum.Split(',');

                    if (konumParcalari.Length == 2)
                    {
                        // Koordinatlarý ayýklayýp, virgülü noktayla deðiþtiriyoruz
                        var latPart = konumParcalari[0].Replace("Lat:", "").Trim().Replace(".",","); // "38.07577126389039"
                        var lngPart = konumParcalari[1].Replace("Lng:", "").Trim().Replace(".", ","); // "34.936083544170685"

                        // Koordinatlarý debug ile kontrol edelim
                        Debug.WriteLine($"Kullanýcý Koordinatlarý: {latPart},{lngPart}");

                        // Double türüne çevirelim
                        double kullaniciEnlem, kullaniciBoylam;
                        if (double.TryParse(latPart, out kullaniciEnlem) && double.TryParse(lngPart, out kullaniciBoylam))
                        {
                            // Etkinlikleri al
                            var etkinlikler = _context.Etkinlikler.ToList();

                            var etkinlikMesafeleri = new List<(Etkinlik etkinlik, double mesafe)>();

                            foreach (var etkinlik in etkinlikler)
                            {
                                var etkinlikKonum = etkinlik.Konum; // Etkinliklerin konumu da ayný formatta olmalý

                                if (!string.IsNullOrEmpty(etkinlikKonum))
                                {
                                    var etkinlikParcalari = etkinlikKonum.Split(',');

                                    if (etkinlikParcalari.Length == 2)
                                    {
                        
                                        // Doðrulama: Her iki parça da doðru formatta mý?
                                        if (etkinlikParcalari.Length != 2 || !etkinlikParcalari[0].Contains("Lat:") || !etkinlikParcalari[1].Contains("Lng:"))
                                        {
                                            Debug.WriteLine($"Geçersiz format: {etkinlik.Konum}");
                                            continue; // Bu etkinlik atlanýr
                                        }

                                        // Fazla boþluklarý kaldýrýrken virgülleri etkilemiyor
                                        var etkinlikLatPart = etkinlikParcalari[0].Replace("Lat:", "").Trim(); // Lat kýsmý
                                        var etkinlikLngPart = etkinlikParcalari[1].Replace("Lng:", "").Trim(); // Lng kýsmý

                                        double etkinlikEnlem, etkinlikBoylam;

                                        // Burada virgülün korunmasýný saðlayarak doðru þekilde sayýya dönüþtürülmesi saðlanýr
                                        if (double.TryParse(etkinlikLatPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out etkinlikEnlem) &&
                                            double.TryParse(etkinlikLngPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out etkinlikBoylam))
                                        {
                                            // Mesafeyi hesapla
                                            double mesafe = CalculateDistance(kullaniciEnlem, kullaniciBoylam, etkinlikEnlem, etkinlikBoylam);
                                            Debug.WriteLine($"Etkinlik Konumu: Lat: {etkinlikEnlem}, Lng: {etkinlikBoylam}");
                                            Debug.WriteLine($"kullanýcý Konumu: Lat: {kullaniciEnlem}, Lng: {kullaniciBoylam}");

                                            Debug.WriteLine($"Etkinlik: {etkinlik.EtkinlikAdi}, Mesafe: {mesafe}");

                                            etkinlikMesafeleri.Add((etkinlik, Math.Abs(mesafe))); // Mesafeyi pozitif hale getiriyoruz
                                        }

                                    }
                                }
                            }

                            var siraliEtkinlikler = etkinlikMesafeleri
                             .OrderBy(em => em.mesafe)  // küçükten büyüðe sýralama
                             .Select(em => em.etkinlik)  // yalnýzca etkinlikleri al
                             .ToList();  // Sonuçlarý listeye çevir


                            foreach (var etkinlik in siraliEtkinlikler)
                            {
                                Debug.WriteLine($"Sýralý Etkinlik: {etkinlik.EtkinlikAdi}, Mesafe: {etkinlikMesafeleri.FirstOrDefault(x => x.etkinlik == etkinlik).mesafe}");
                            }

                            // Sýralanmýþ etkinlikleri ViewBag'e ekleyin
                            ViewBag.Etkinlikler = siraliEtkinlikler;

                            // Sýralanmýþ etkinlikleri ViewBag'e ekleyin
                            ViewBag.Etkinlikler = siraliEtkinlikler;
                            Debug.WriteLine(siraliEtkinlikler[0].EtkinlikAdi);
                            Debug.WriteLine("Sýralý etkinlikler:");
                            foreach (var etkinlik in siraliEtkinlikler)
                            {
                                Debug.WriteLine($"Etkinlik Adý: {etkinlik.EtkinlikAdi}");
                            }

                            // Tüm listelerde gelecek tarihli etkinlikleri filtrele
                            var simdikiTarih = DateTime.Now;
                            ViewBag.IlgiAlaninaGoreEtkinlikler = ilgiAlaninaGoreEtkinlikler
                                .Where(e => e.Tarih >= simdikiTarih)
                                .ToList();

                            ViewBag.KatilimGecmisineGoreEtkinlikler = ilgiliEtkinlikler
                                .Where(e => e.Tarih >= simdikiTarih)
                                .ToList();

                            ViewBag.Etkinlikler = siraliEtkinlikler
                                .Where(e => e.Tarih >= simdikiTarih)
                            .ToList();
                        }
                        else
                        {
                            // Geçersiz konum verisi durumunda hata mesajý
                            ViewBag.Etkinlikler = new List<Etkinlik>(); // Geçersiz konum verisi
                        }
                    }
                    else
                    {
                        // Geçersiz konum formatý
                        ViewBag.Etkinlikler = new List<Etkinlik>(); // Hatalý konum formatý
                    }
                }
                else
                {
                    // Kullanýcý konumu eksikse, etkinlikleri sýfýrdan listele
                    ViewBag.Etkinlikler = _context.Etkinlikler.ToList();
                }
            }
            else
            {
                // Kullanýcý kimliði yoksa, giriþ yapmasý için yönlendir
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Kilometre cinsinden Dünya'nýn yarýçapý
            var dLat = ToRad(lat2 - lat1); // Dereceden radiana dönüþtürme
            var dLon = ToRad(lon2 - lon1); // Dereceden radiana dönüþtürme

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c; // Mesafe kilometre cinsinden

            return distance;
        }

        // Dereceden radiana dönüþtürme fonksiyonu
        public static double ToRad(double deg)
        {
            return deg * (Math.PI / 180);
        }



        [HttpGet]
        public IActionResult GetUserLocation()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Json(new { success = false, message = "Kullanýcý oturum açmamýþ veya kimlik bilgisi eksik." });
            }

            var user = _context.Kullanýcýlar.FirstOrDefault(k => k.ID == int.Parse(userId));
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanýcý bulunamadý." });
            }

            if (string.IsNullOrEmpty(user.Konum) || !user.Konum.Contains(",") || user.Konum.Split(',').Length != 2)
            {
                return Json(new { success = false, message = "Geçersiz konum formatý veya konum eksik." });
            }

            return Json(new { success = true, location = user.Konum });
        }

        [HttpGet]
        public IActionResult GetEventLocations()
        {
            // Etkinlikleri veritabanýndan almak
            var etkinlikler = _context.Etkinlikler.ToList();

            // Etkinliklerin konumlarýný içeren liste
            var etkinlikKonumlari = new List<object>();

            foreach (var etkinlik in etkinlikler)
            {
                // Konumun geçerli olup olmadýðýný kontrol et
                if (!string.IsNullOrEmpty(etkinlik.Konum) && etkinlik.Konum.Contains(","))
                {
                    try
                    {
                        // Konumu ayýr ve enlem-boylam bilgilerini al
                        var konumVerisi = etkinlik.Konum.Split(',');

                        // Fazla boþluklarý kaldýrýrken virgülleri etkilemiyor
                        var etkinlikLatPart = konumVerisi[0].Replace("Lat:", "").Trim(); // Lat kýsmý
                        var etkinlikLngPart = konumVerisi[1].Replace("Lng:", "").Trim(); // Lng kýsmý
                        double etkinlikEnlem, etkinlikBoylam;

                        // Enlem ve boylamýn geçerli sayýsal deðerler olup olmadýðýný kontrol et
                        if (double.TryParse(etkinlikLatPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out etkinlikEnlem) &&
                            double.TryParse(etkinlikLngPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out etkinlikBoylam))
                        {
                            // Konum bilgileriyle birlikte etkinlik adý ve ID ekle
                            etkinlikKonumlari.Add(new
                            {
                                id = etkinlik.ID, // Etkinlik ID'si
                                latitude = etkinlikEnlem,
                                longitude = etkinlikBoylam,
                                name = etkinlik.EtkinlikAdi // Etkinlik adý
                            });
                        }
                        else
                        {
                            // Geçersiz enlem ve boylam deðeri varsa hata loglama
                            Debug.WriteLine($"Geçersiz enlem/boylam: {etkinlik.Konum}");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Hatalý format durumunda hata loglama
                        Debug.WriteLine($"Hata: {ex.Message}, Etkinlik: {etkinlik.EtkinlikAdi}, Konum: {etkinlik.Konum}");
                        continue;
                    }
                }
                else
                {
                    Debug.WriteLine($"Geçersiz konum formatý: {etkinlik.Konum}");
                }
            }

            // Eðer etkinliklerin konumlarý varsa, veriyi döndür
            if (etkinlikKonumlari.Any())
            {
                return Json(new { success = true, etkinlikKonumlari });
            }
            else
            {
                return Json(new { success = false, message = "Etkinliklerin konumu bulunamadý." });
            }
        }




        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "LoginPage");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public int CalculateUserPoints(int userId)
        {
           
            int kullaniciPuanlari = _context.Puanlar
                   .Where(p => p.KullanýcýID == userId)
                   .Sum(p => p.Puanlar);

            return kullaniciPuanlari;
        }
        [HttpPost]
        public IActionResult MarkAsRead(int id)
        {
            var bildirim = _context.Bildirimler.FirstOrDefault(b => b.ID == id);
            if (bildirim != null)
            {
                bildirim.OkunduMu = true;
                _context.SaveChanges();
                return Ok();
            }
            return NotFound();
        }
    }
}
