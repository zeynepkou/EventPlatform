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
        
        
        // T�m ba��ml�l�klar� tek bir kurucuda toplay�n
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
                // Kullan�c� verilerini al�n
                var kullaniciVerileri = _context.Kullan�c�lar
                    .Include(k => k.KullaniciIlgiAlanlari) // �lgi alanlar�n� y�klemek i�in Include
                    .ThenInclude(ia => ia.IlgiAlani)
                    .FirstOrDefault(u => u.ID == int.Parse(userId));

                if (kullaniciVerileri == null)
                {
                    return RedirectToAction("Error", "Home");
                }

                ViewBag.KullaniciVerileri = kullaniciVerileri;
                ViewBag.KullaniciPuan = CalculateUserPoints(int.Parse(userId));
                ViewBag.Bildirimler = _context.Bildirimler
      .Where(b => b.Kullan�c�ID == int.Parse(userId) && b.OkunduMu == false)
      .OrderByDescending(b => b.Tarih)
      .ToList();

                ViewBag.BildirimSayisi = ViewBag.Bildirimler.Count;

                // �lgi alan�na g�re etkinlikler
                var kullaniciIlgiAlanlari = kullaniciVerileri.KullaniciIlgiAlanlari.Select(ia => ia.IlgiAlani.ID).ToList();
                var ilgiAlaninaGoreEtkinlikler = _context.Etkinlikler
                    .Include(e => e.Kategori)
                    .Where(e => kullaniciIlgiAlanlari.Contains(e.Kategori.IlgiAlaniID))
                    .ToList();

                ViewBag.IlgiAlaninaGoreEtkinlikler = ilgiAlaninaGoreEtkinlikler;


                // Kullan�c�n�n kat�ld��� etkinlikleri al
                var kullaniciEtkinlikleri = _context.Kat�l�mc�lar
                    .Where(k => k.Kullan�c�ID == int.Parse(userId))
                    .Include(k => k.Etkinlik)
                    .ThenInclude(e => e.Kategori)
                    .ToList();

                // Kullan�c�n�n kat�ld��� etkinliklerin kategorilerini bul
                var katildigiKategoriIds = kullaniciEtkinlikleri
                    .Select(k => k.Etkinlik.Kategori.ID)
                    .Distinct()
                    .ToList();

                // Bu kategorilere ait ilgi alanlar�n� bul
                var katildigiIlgiAlaniIds = _context.Kategoriler
                    .Where(k => katildigiKategoriIds.Contains(k.ID))
                    .Select(k => k.IlgiAlaniID)
                    .Distinct()
                    .ToList();

                // Bu ilgi alanlar�na ait t�m kategorilerdeki etkinlikleri al
                var ilgiliEtkinlikler = _context.Etkinlikler
                    .Include(e => e.Kategori)
                    .Where(e => katildigiIlgiAlaniIds.Contains(e.Kategori.IlgiAlaniID) &&
                                !kullaniciEtkinlikleri.Select(ke => ke.Etkinlik.ID).Contains(e.ID)) // Ayn� etkinlik hari�
                    .ToList();

                // �nerilen etkinlikleri ViewBag'e aktar
                ViewBag.KatilimGecmisineGoreEtkinlikler = ilgiliEtkinlikler;

                // Favori durumlar�n� kontrol et
                var favorietkinlikler = _context.Etkinlikler
                    .Include(e => e.Favoriler)
                    .ToList();

                // Favori durumlar�n� Dictionary olarak olu�tur ve ViewBag'e aktar
                ViewBag.FavoriDurumlari = favorietkinlikler.ToDictionary(
                    e => e.ID,
                    e => e.Favoriler.Any(f => f.Kullan�c�ID == int.Parse(userId))
                );

                ViewBag.Kullan�c�ID = int.Parse(userId);

                // Kullan�c�n�n konum bilgisini al�yoruz
                var kullaniciKonum = kullaniciVerileri.Konum; // "Lat:41.01866120303227,Lng:29.09544067498994" �eklinde

                if (!string.IsNullOrEmpty(kullaniciKonum))
                {
                    var konumParcalari = kullaniciKonum.Split(',');

                    if (konumParcalari.Length == 2)
                    {
                        // Koordinatlar� ay�klay�p, virg�l� noktayla de�i�tiriyoruz
                        var latPart = konumParcalari[0].Replace("Lat:", "").Trim().Replace(".",","); // "38.07577126389039"
                        var lngPart = konumParcalari[1].Replace("Lng:", "").Trim().Replace(".", ","); // "34.936083544170685"

                        // Koordinatlar� debug ile kontrol edelim
                        Debug.WriteLine($"Kullan�c� Koordinatlar�: {latPart},{lngPart}");

                        // Double t�r�ne �evirelim
                        double kullaniciEnlem, kullaniciBoylam;
                        if (double.TryParse(latPart, out kullaniciEnlem) && double.TryParse(lngPart, out kullaniciBoylam))
                        {
                            // Etkinlikleri al
                            var etkinlikler = _context.Etkinlikler.ToList();

                            var etkinlikMesafeleri = new List<(Etkinlik etkinlik, double mesafe)>();

                            foreach (var etkinlik in etkinlikler)
                            {
                                var etkinlikKonum = etkinlik.Konum; // Etkinliklerin konumu da ayn� formatta olmal�

                                if (!string.IsNullOrEmpty(etkinlikKonum))
                                {
                                    var etkinlikParcalari = etkinlikKonum.Split(',');

                                    if (etkinlikParcalari.Length == 2)
                                    {
                        
                                        // Do�rulama: Her iki par�a da do�ru formatta m�?
                                        if (etkinlikParcalari.Length != 2 || !etkinlikParcalari[0].Contains("Lat:") || !etkinlikParcalari[1].Contains("Lng:"))
                                        {
                                            Debug.WriteLine($"Ge�ersiz format: {etkinlik.Konum}");
                                            continue; // Bu etkinlik atlan�r
                                        }

                                        // Fazla bo�luklar� kald�r�rken virg�lleri etkilemiyor
                                        var etkinlikLatPart = etkinlikParcalari[0].Replace("Lat:", "").Trim(); // Lat k�sm�
                                        var etkinlikLngPart = etkinlikParcalari[1].Replace("Lng:", "").Trim(); // Lng k�sm�

                                        double etkinlikEnlem, etkinlikBoylam;

                                        // Burada virg�l�n korunmas�n� sa�layarak do�ru �ekilde say�ya d�n��t�r�lmesi sa�lan�r
                                        if (double.TryParse(etkinlikLatPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out etkinlikEnlem) &&
                                            double.TryParse(etkinlikLngPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out etkinlikBoylam))
                                        {
                                            // Mesafeyi hesapla
                                            double mesafe = CalculateDistance(kullaniciEnlem, kullaniciBoylam, etkinlikEnlem, etkinlikBoylam);
                                            Debug.WriteLine($"Etkinlik Konumu: Lat: {etkinlikEnlem}, Lng: {etkinlikBoylam}");
                                            Debug.WriteLine($"kullan�c� Konumu: Lat: {kullaniciEnlem}, Lng: {kullaniciBoylam}");

                                            Debug.WriteLine($"Etkinlik: {etkinlik.EtkinlikAdi}, Mesafe: {mesafe}");

                                            etkinlikMesafeleri.Add((etkinlik, Math.Abs(mesafe))); // Mesafeyi pozitif hale getiriyoruz
                                        }

                                    }
                                }
                            }

                            var siraliEtkinlikler = etkinlikMesafeleri
                             .OrderBy(em => em.mesafe)  // k���kten b�y��e s�ralama
                             .Select(em => em.etkinlik)  // yaln�zca etkinlikleri al
                             .ToList();  // Sonu�lar� listeye �evir


                            foreach (var etkinlik in siraliEtkinlikler)
                            {
                                Debug.WriteLine($"S�ral� Etkinlik: {etkinlik.EtkinlikAdi}, Mesafe: {etkinlikMesafeleri.FirstOrDefault(x => x.etkinlik == etkinlik).mesafe}");
                            }

                            // S�ralanm�� etkinlikleri ViewBag'e ekleyin
                            ViewBag.Etkinlikler = siraliEtkinlikler;

                            // S�ralanm�� etkinlikleri ViewBag'e ekleyin
                            ViewBag.Etkinlikler = siraliEtkinlikler;
                            Debug.WriteLine(siraliEtkinlikler[0].EtkinlikAdi);
                            Debug.WriteLine("S�ral� etkinlikler:");
                            foreach (var etkinlik in siraliEtkinlikler)
                            {
                                Debug.WriteLine($"Etkinlik Ad�: {etkinlik.EtkinlikAdi}");
                            }

                            // T�m listelerde gelecek tarihli etkinlikleri filtrele
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
                            // Ge�ersiz konum verisi durumunda hata mesaj�
                            ViewBag.Etkinlikler = new List<Etkinlik>(); // Ge�ersiz konum verisi
                        }
                    }
                    else
                    {
                        // Ge�ersiz konum format�
                        ViewBag.Etkinlikler = new List<Etkinlik>(); // Hatal� konum format�
                    }
                }
                else
                {
                    // Kullan�c� konumu eksikse, etkinlikleri s�f�rdan listele
                    ViewBag.Etkinlikler = _context.Etkinlikler.ToList();
                }
            }
            else
            {
                // Kullan�c� kimli�i yoksa, giri� yapmas� i�in y�nlendir
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Kilometre cinsinden D�nya'n�n yar��ap�
            var dLat = ToRad(lat2 - lat1); // Dereceden radiana d�n��t�rme
            var dLon = ToRad(lon2 - lon1); // Dereceden radiana d�n��t�rme

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = R * c; // Mesafe kilometre cinsinden

            return distance;
        }

        // Dereceden radiana d�n��t�rme fonksiyonu
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
                return Json(new { success = false, message = "Kullan�c� oturum a�mam�� veya kimlik bilgisi eksik." });
            }

            var user = _context.Kullan�c�lar.FirstOrDefault(k => k.ID == int.Parse(userId));
            if (user == null)
            {
                return Json(new { success = false, message = "Kullan�c� bulunamad�." });
            }

            if (string.IsNullOrEmpty(user.Konum) || !user.Konum.Contains(",") || user.Konum.Split(',').Length != 2)
            {
                return Json(new { success = false, message = "Ge�ersiz konum format� veya konum eksik." });
            }

            return Json(new { success = true, location = user.Konum });
        }

        [HttpGet]
        public IActionResult GetEventLocations()
        {
            // Etkinlikleri veritaban�ndan almak
            var etkinlikler = _context.Etkinlikler.ToList();

            // Etkinliklerin konumlar�n� i�eren liste
            var etkinlikKonumlari = new List<object>();

            foreach (var etkinlik in etkinlikler)
            {
                // Konumun ge�erli olup olmad���n� kontrol et
                if (!string.IsNullOrEmpty(etkinlik.Konum) && etkinlik.Konum.Contains(","))
                {
                    try
                    {
                        // Konumu ay�r ve enlem-boylam bilgilerini al
                        var konumVerisi = etkinlik.Konum.Split(',');

                        // Fazla bo�luklar� kald�r�rken virg�lleri etkilemiyor
                        var etkinlikLatPart = konumVerisi[0].Replace("Lat:", "").Trim(); // Lat k�sm�
                        var etkinlikLngPart = konumVerisi[1].Replace("Lng:", "").Trim(); // Lng k�sm�
                        double etkinlikEnlem, etkinlikBoylam;

                        // Enlem ve boylam�n ge�erli say�sal de�erler olup olmad���n� kontrol et
                        if (double.TryParse(etkinlikLatPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out etkinlikEnlem) &&
                            double.TryParse(etkinlikLngPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out etkinlikBoylam))
                        {
                            // Konum bilgileriyle birlikte etkinlik ad� ve ID ekle
                            etkinlikKonumlari.Add(new
                            {
                                id = etkinlik.ID, // Etkinlik ID'si
                                latitude = etkinlikEnlem,
                                longitude = etkinlikBoylam,
                                name = etkinlik.EtkinlikAdi // Etkinlik ad�
                            });
                        }
                        else
                        {
                            // Ge�ersiz enlem ve boylam de�eri varsa hata loglama
                            Debug.WriteLine($"Ge�ersiz enlem/boylam: {etkinlik.Konum}");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Hatal� format durumunda hata loglama
                        Debug.WriteLine($"Hata: {ex.Message}, Etkinlik: {etkinlik.EtkinlikAdi}, Konum: {etkinlik.Konum}");
                        continue;
                    }
                }
                else
                {
                    Debug.WriteLine($"Ge�ersiz konum format�: {etkinlik.Konum}");
                }
            }

            // E�er etkinliklerin konumlar� varsa, veriyi d�nd�r
            if (etkinlikKonumlari.Any())
            {
                return Json(new { success = true, etkinlikKonumlari });
            }
            else
            {
                return Json(new { success = false, message = "Etkinliklerin konumu bulunamad�." });
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
                   .Where(p => p.Kullan�c�ID == userId)
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
