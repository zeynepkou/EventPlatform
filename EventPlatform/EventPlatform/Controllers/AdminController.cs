using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Yazlab2.Data;
using Yazlab2.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yazlab2.Migrations;

namespace Yazlab2.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AdminIndex()
        {
            int adminId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Admin bilgilerini alın
            var admin = await _context.Kullanıcılar.FirstOrDefaultAsync(a => a.ID == adminId);
            if (admin == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Profil fotoğrafı yolunu ViewBag'e ekledik
            ViewBag.AdminProfilFoto = admin.ProfilFotografiYolu;

            // Onay bekleyen ve onaylanmış etkinlikleri filtrele
            var onayBekleyenEtkinlikler = _context.Etkinlikler
                .Where(e => e.EtkinlikDurum == 2)
                .ToList();

            var onaylanmisEtkinlikler = _context.Etkinlikler
                .Where(e => e.EtkinlikDurum == 3)
                .ToList();

            var silinmisEtkinlikler = _context.Etkinlikler
                .Where(e => e.EtkinlikDurum == 4)
                .ToList();


            // Bildirimleri alıyoruz
            var kullaniciBildirimleri = _context.Bildirimler
                .Where(b => b.KullanıcıID == adminId && b.OkunduMu == false)
                .OrderByDescending(b => b.Tarih)
                .ToList();

            ViewBag.Bildirimler = kullaniciBildirimleri;
            ViewBag.BildirimSayisi = kullaniciBildirimleri.Count;

            // İki listeyi bir ViewModel kullanarak gönderiyoruz
            var model = new AdminIndexViewModel
            {
                OnayBekleyenEtkinlikler = onayBekleyenEtkinlikler,
                OnaylanmisEtkinlikler = onaylanmisEtkinlikler,
                SilinmisEtkinlikler = silinmisEtkinlikler
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Onayla(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik != null && etkinlik.EtkinlikDurum == 2)
            {
                etkinlik.EtkinlikDurum = 3;

                
                int adminId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));


                // Bildirim oluştur
                var bildirim = new Bildirim
                {
                    KullanıcıID = adminId,
                    Mesaj = $"Etkinliğiniz onaylandı: {etkinlik.EtkinlikAdi}",
                    Tarih = DateTime.Now,
                    OkunduMu = false,

                };

                _context.Bildirimler.Add(bildirim);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AdminIndex");
        }


        [HttpPost]
        public async Task<IActionResult> OnayBeklemeSil(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik != null && etkinlik.EtkinlikDurum == 2)
            {
                etkinlik.EtkinlikDurum = 4;

               
                int adminId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));


                // Bildirim oluştur
                var bildirim = new Bildirim
                {
                    KullanıcıID = adminId,
                    Mesaj = $"Etkinliğiniz silindi: {etkinlik.EtkinlikAdi}",
                    Tarih = DateTime.Now,
                    OkunduMu = false,

                };

                _context.Bildirimler.Add(bildirim);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AdminIndex");
        }

        [HttpPost]
        public async Task<IActionResult> GeriDonusum(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik != null && etkinlik.EtkinlikDurum == 4)
            {
                etkinlik.EtkinlikDurum = 2;

               
                int adminId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));


                // Bildirim oluştur
                var bildirim = new Bildirim
                {
                    KullanıcıID = adminId,
                    Mesaj = $"Etkinliğiniz geri dönüşümde: {etkinlik.EtkinlikAdi}",
                    Tarih = DateTime.Now,
                    OkunduMu = false,

                };

                _context.Bildirimler.Add(bildirim);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AdminIndex");
        }

        [HttpPost]
        public async Task<IActionResult> OnaySil2(int id)
        {
            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik != null)
            {
                
                int adminId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));


                // Bildirim oluştur
                var bildirim = new Bildirim
                {
                    KullanıcıID = adminId,
                    Mesaj = $"Etkinliğiniz onaylandı2: {etkinlik.EtkinlikAdi}",
                    Tarih = DateTime.Now,
                    OkunduMu = false,

                };

                _context.Bildirimler.Add(bildirim);

                _context.Etkinlikler.Remove(etkinlik);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AdminIndex");
        }


        public IActionResult KullanıcıYönetimi()
        {
            var kullanıcılar = _context.Kullanıcılar
                .Where(k => k.KullaniciTuru != "Admin")
                .ToList();

            // Kullanıcıların puanlarını hesapla ve ViewBag ile View'e gönder
            var kullanıcılarVePuanlar = kullanıcılar.Select(kullanıcı => new
            {
                Kullanıcı = kullanıcı,
                ToplamPuan = CalculateUserPoints(kullanıcı.ID) 
            }).ToList();

            ViewBag.KullanıcılarVePuanlar = kullanıcılarVePuanlar;

            return View();
        }
        public int CalculateUserPoints(int userId)
        {

            int kullaniciPuanlari = _context.Puanlar
                   .Where(p => p.KullanıcıID == userId)
                   .Sum(p => p.Puanlar);

            return kullaniciPuanlari;
        }



        [HttpPost]
        public async Task<IActionResult> KullaniciSil(int id)
        {
            
            var kullanıcı = await _context.Kullanıcılar
                .Include(k => k.KatildigiEtkinlikler)
                .Include(k => k.GonderilenMesajlar)
                .Include(k => k.AlinanMesajlar)
                .Include(k => k.Puanlar)
                .Include(k => k.Favoriler)
                .FirstOrDefaultAsync(k => k.ID == id);

            if (kullanıcı == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı!";
                return RedirectToAction("KullanıcıYönetimi");
            }

            // Admin kullanıcıyı bul
            var admin = await _context.Kullanıcılar.FirstOrDefaultAsync(k => k.KullaniciTuru == "Admin");
            if (admin == null)
            {
                TempData["ErrorMessage"] = "Admin kullanıcı bulunamadı!";
                return RedirectToAction("KullanıcıYönetimi");
            }

            // Kullanıcının oluşturduğu etkinliklerin kullanıcı ID'sini Admin'in ID'siyle değiştir
            var etkinlikler = _context.Etkinlikler.Where(e => e.KullanıcıID == id).ToList();
            foreach (var etkinlik in etkinlikler)
            {
                etkinlik.KullanıcıID = admin.ID; 
            }

            // Kullanıcıya ait diğer verileri sil
            var katılımcılar = _context.Katılımcılar.Where(k => k.KullanıcıID == id).ToList();
            _context.Katılımcılar.RemoveRange(katılımcılar);

            var gonderilenMesajlar = _context.Mesajlar.Where(m => m.GondericiID == id).ToList();
            _context.Mesajlar.RemoveRange(gonderilenMesajlar);

            var alinanMesajlar = _context.Mesajlar.Where(m => m.AliciID == id).ToList();
            _context.Mesajlar.RemoveRange(alinanMesajlar);

            var geriBildirimler = _context.GeriBildirimler.Where(g => g.GondericiID == id).ToList();
            _context.GeriBildirimler.RemoveRange(geriBildirimler);

            var favoriler = _context.Favoriler.Where(f => f.KullanıcıID == id).ToList();
            _context.Favoriler.RemoveRange(favoriler);

            var yildizPuanlar = _context.YildizPuanlar.Where(yp => yp.KullanıcıID == id).ToList();
            _context.YildizPuanlar.RemoveRange(yildizPuanlar);

            var puanlar = _context.Puanlar.Where(p => p.KullanıcıID == id).ToList();
            _context.Puanlar.RemoveRange(puanlar);

            // Kullanıcıyı sil
            _context.Kullanıcılar.Remove(kullanıcı);
            
            int adminId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));


            // Bildirim oluştur
            var bildirim = new Bildirim
            {
                KullanıcıID = adminId,
                Mesaj = $"kullanıcı silindi: {kullanıcı.KullaniciAdi}",
                Tarih = DateTime.Now,
                OkunduMu = false,

            };

            _context.Bildirimler.Add(bildirim);
            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi ve etkinlikleri Admin'e devredildi.";
            return RedirectToAction("KullanıcıYönetimi");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Kullanıcı model, IFormFile ProfilFotografi)
        {
            // Kullanıcıyı ID ile veritabanında bul
            var kullanıcı = await _context.Kullanıcılar.FirstOrDefaultAsync(k => k.ID == model.ID);
            if (kullanıcı == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı!";
                return RedirectToAction("KullanıcıYönetimi");
            }

            // Kullanıcı bilgilerini güncelle
            kullanıcı.KullaniciAdi = model.KullaniciAdi ?? kullanıcı.KullaniciAdi;
            kullanıcı.Eposta = model.Eposta ?? kullanıcı.Eposta;
            kullanıcı.Ad = model.Ad ?? kullanıcı.Ad;
            kullanıcı.Soyad = model.Soyad ?? kullanıcı.Soyad;
            kullanıcı.DogumTarihi = model.DogumTarihi != default(DateTime) ? model.DogumTarihi : kullanıcı.DogumTarihi;
            kullanıcı.Cinsiyet = model.Cinsiyet ?? kullanıcı.Cinsiyet;
            kullanıcı.TelefonNumarasi = model.TelefonNumarasi ?? kullanıcı.TelefonNumarasi;
            kullanıcı.Konum = model.Konum ?? kullanıcı.Konum;
            kullanıcı.IlgiAlanlari = model.IlgiAlanlari ?? kullanıcı.IlgiAlanlari;

            // Profil fotoğrafı güncellemesi
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

                kullanıcı.ProfilFotografiYolu = $"/resim/{uniqueFileName}";
            }

            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();
            
            int adminId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));


            // Bildirim oluştur
            var bildirim = new Bildirim
            {
                KullanıcıID = adminId,
                Mesaj = $"kullanıcı profili güncellendi: ",
                Tarih = DateTime.Now,
                OkunduMu = false,

            };

            _context.Bildirimler.Add(bildirim);
            return RedirectToAction("KullanıcıYönetimi", "Admin");
        }

        public async Task<IActionResult> GeriBildirim()
        {
            // Okunmamış ve okunmuş geri bildirimleri ayrı ayrı listele
            var okunmamisGeriBildirimler = await _context.GeriBildirimler
                .Include(g => g.Gonderici) // Gönderen kullanıcı bilgisi
                .Include(g => g.Etkinlik) // İlgili etkinlik bilgisi
                .Where(g => g.GeriBildirimDurum == 1)
                .ToListAsync();

            var okunmusGeriBildirimler = await _context.GeriBildirimler
                .Include(g => g.Gonderici) // Gönderen kullanıcı bilgisi
                .Include(g => g.Etkinlik) // İlgili etkinlik bilgisi
                .Where(g => g.GeriBildirimDurum == 2)
                .ToListAsync();

            // ViewBag ile iki listeyi gönder
            ViewBag.OkunmamisGeriBildirimler = okunmamisGeriBildirimler;
            ViewBag.OkunmusGeriBildirimler = okunmusGeriBildirimler;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> GeriBildirimOkundu(int id)
        {
            // Geri bildirimi ID ile bul
            var geriBildirim = await _context.GeriBildirimler.FindAsync(id);
            if (geriBildirim != null && geriBildirim.GeriBildirimDurum == 1)
            {
                // Durumu güncelle
                geriBildirim.GeriBildirimDurum = 2;

                // Giriş yapan kullanıcının ID'sini alın ve int'e çevirin
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    throw new Exception("Admin ID alınamadı.");
                }

                int adminId = Convert.ToInt32(userIdClaim);

                // Bildirim oluştur
                var bildirim = new Bildirim
                {
                    KullanıcıID = adminId,
                    Mesaj = "Yeni bir geri bildirim okundu.",
                    Tarih = DateTime.Now,
                    OkunduMu = false
                };

                // Veritabanına ekle ve değişiklikleri kaydet
                _context.Bildirimler.Add(bildirim);
                await _context.SaveChangesAsync(); // Burada bildirim ve geri bildirim kaydedilir
            }

            // Sayfayı yenile
            return RedirectToAction("GeriBildirim");
        }


        public async Task<IActionResult> Rapor()
        {
            // Etkinliklere ait katılımcı sayısı ve yıldız puanı bilgilerini içeren bir liste hazırlayın
            var etkinlikRaporu = await _context.Etkinlikler
                .Select(e => new EtkinlikRaporViewModel
                {
                    EtkinlikAdi = e.EtkinlikAdi,
                    KatılımcıSayisi = e.Katılımcılar.Count,
                    OrtalamaPuan = e.YildizPuaniSayisi > 0 ? e.OrtalamaPuan : 0, // Ortalama puanı kontrol edin
                    ToplamYildizPuani = e.ToplamYildizPuani,
                    YildizPuaniSayisi = e.YildizPuaniSayisi,
                    FavoriSayisi = e.FavoriSayisi // Favori sayısını dahil ettik
                })
                .ToListAsync();

            // En yüksek puanlı kullanıcıyı belirle
            var kullanıcılar = await _context.Kullanıcılar.ToListAsync();
            var enYuksekPuanliKullanici = kullanıcılar
                .Select(k => new
                {
                    KullanıcıID = k.ID,
                    KullaniciAdi = k.KullaniciAdi,
                    ToplamPuan = CalculateUserPoints(k.ID)
                })
                .OrderByDescending(k => k.ToplamPuan)
                .FirstOrDefault();

            // En çok etkinliğe katılan kullanıcıyı belirle
            var katilimlar = await _context.Katılımcılar.ToListAsync();
            var enAktifKullanici = katilimlar
                .GroupBy(k => k.KullanıcıID)
                .Select(g => new
                {
                    KullanıcıID = g.Key,
                    KullaniciAdi = kullanıcılar.FirstOrDefault(k => k.ID == g.Key)?.KullaniciAdi,
                    KatildigiEtkinlikSayisi = g.Count()
                })
                .OrderByDescending(k => k.KatildigiEtkinlikSayisi)
                .FirstOrDefault();

            // ViewBag ile verileri View'e gönder
            ViewBag.EnYuksekPuanliKullanici = enYuksekPuanliKullanici;
            ViewBag.EnAktifKullanici = enAktifKullanici;

            return View("Rapor", etkinlikRaporu);
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


        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int eventId, bool forceDelete = false)
        {
            var etkinlik = await _context.Etkinlikler
                .Include(e => e.Katılımcılar)
                .Include(e => e.YildizPuanlar)
                .Include(e => e.Favoriler)
                .Include(e => e.GeriBildirimler)
                .Include(e => e.Mesajlar)
                .FirstOrDefaultAsync(e => e.ID == eventId);

            if (etkinlik == null)
            {
                TempData["ErrorMessage"] = "Etkinlik bulunamadı!";
                return RedirectToAction("AdminEtkinlik", "Etkinlikler");
            }

            // Eğer forceDelete değilse ve etkinliğin katılımcıları varsa uyarı ver
            if (!forceDelete && etkinlik.Katılımcılar.Any())
            {
                TempData["ErrorMessage"] = "Bu etkinliğe katılan kullanıcılar var!";
                return RedirectToAction("AdminEtkinlik", "Etkinlikler");
            }

            // İlgili kayıtları silme
            _context.Katılımcılar.RemoveRange(etkinlik.Katılımcılar);
            _context.YildizPuanlar.RemoveRange(etkinlik.YildizPuanlar);
            _context.Favoriler.RemoveRange(etkinlik.Favoriler);
            _context.GeriBildirimler.RemoveRange(etkinlik.GeriBildirimler);
            _context.Mesajlar.RemoveRange(etkinlik.Mesajlar);
            _context.Etkinlikler.Remove(etkinlik);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Etkinlik başarıyla silindi.";
            return RedirectToAction("AdminEtkinlik", "Etkinlikler");
        }


        public async Task<IActionResult> KullanıcıDetay(int id)
        {
            // İlgili kullanıcıyı ID ile bul
            var kullanıcı = await _context.Kullanıcılar
                .Include(k => k.KatildigiEtkinlikler)
                .ThenInclude(ke => ke.Etkinlik)
                .FirstOrDefaultAsync(k => k.ID == id);

            if (kullanıcı == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı!";
                return RedirectToAction("KullanıcıYönetimi");
            }

            // Şu anki tarih
            var currentDate = DateTime.Now;

            // Katıldığı ve katılacağı etkinlikler
            var katildigiEtkinlikler = kullanıcı.KatildigiEtkinlikler
                .Where(ke => ke.Etkinlik.Tarih < currentDate) // Geçmiş etkinlikler
                .Select(ke => ke.Etkinlik)
                .ToList();

            var katilacagiEtkinlikler = kullanıcı.KatildigiEtkinlikler
                .Where(ke => ke.Etkinlik.Tarih >= currentDate) // Gelecek etkinlikler
                .Select(ke => ke.Etkinlik)
                .ToList();

            // Kullanıcının ilgi alanlarını al
            var userIlgiAlanlari = await _context.KullaniciIlgiAlanlari
                .Where(ka => ka.KullanıcıID == id)
                .Select(ka => ka.IlgiAlaniID)
                .ToListAsync();

            // Tüm ilgi alanlarını al
            var tumIlgiAlanlari = await _context.IlgiAlanlari.ToListAsync();

            // ViewBag üzerinden bilgileri geç
            ViewBag.KatildigiEtkinlikler = katildigiEtkinlikler;
            ViewBag.KatilacagiEtkinlikler = katilacagiEtkinlikler;
            ViewBag.TumIlgiAlanlari = tumIlgiAlanlari;
            ViewBag.SeciliIlgiAlanlari = userIlgiAlanlari;

            // Kullanıcı detaylarını görünüme gönder
            return View("~/Views/KullanıcıProfil/KullaniciProfil2.cshtml", kullanıcı);
        }

        [HttpPost]
        public async Task<IActionResult> KullaniciProfilGuncelle(Kullanıcı model, IFormFile ProfilFotografi, List<int> selectedIlgiAlanlari)
        {
            // Kullanıcıyı ID ile veritabanında bul
            var kullanıcı = await _context.Kullanıcılar
                .Include(k => k.KullaniciIlgiAlanlari) // İlgi alanlarını da dahil edin
                .FirstOrDefaultAsync(k => k.ID == model.ID);
            if (kullanıcı == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı!";
                return RedirectToAction("KullanıcıProfil2", new { id = model.ID });
            }

            // Kullanıcı bilgilerini güncelle
            kullanıcı.KullaniciAdi = model.KullaniciAdi ?? kullanıcı.KullaniciAdi;
            kullanıcı.Eposta = model.Eposta ?? kullanıcı.Eposta;
            kullanıcı.Ad = model.Ad ?? kullanıcı.Ad;
            kullanıcı.Soyad = model.Soyad ?? kullanıcı.Soyad;
            kullanıcı.DogumTarihi = model.DogumTarihi != default(DateTime) ? model.DogumTarihi : kullanıcı.DogumTarihi;
            kullanıcı.Cinsiyet = model.Cinsiyet ?? kullanıcı.Cinsiyet;
            kullanıcı.TelefonNumarasi = model.TelefonNumarasi ?? kullanıcı.TelefonNumarasi;
            kullanıcı.Konum = model.Konum ?? kullanıcı.Konum;

            // İlgi alanlarını güncelle
            if (selectedIlgiAlanlari != null)
            {
                // Eski ilgi alanlarını temizle
                var mevcutIlgiAlanlari = _context.KullaniciIlgiAlanlari.Where(kia => kia.KullanıcıID == kullanıcı.ID);
                _context.KullaniciIlgiAlanlari.RemoveRange(mevcutIlgiAlanlari);

                // Yeni ilgi alanlarını ekle
                foreach (var ilgiAlaniId in selectedIlgiAlanlari)
                {
                    _context.KullaniciIlgiAlanlari.Add(new KullaniciIlgiAlani
                    {
                        KullanıcıID = kullanıcı.ID,
                        IlgiAlaniID = ilgiAlaniId
                    });
                }
            }

            // Profil fotoğrafını güncelleme
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

                kullanıcı.ProfilFotografiYolu = $"/resim/{uniqueFileName}";
            }

            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kullanıcı bilgileri başarıyla güncellendi.";
            return RedirectToAction("KullanıcıProfil2", new { id = model.ID });
        }


        public async Task<IActionResult> KullanıcıProfil2(int id)
        {
            // Kullanıcıyı ID ile bul
            var kullanıcı = await _context.Kullanıcılar
                .Include(k => k.KatildigiEtkinlikler)
                .ThenInclude(ke => ke.Etkinlik)
                .FirstOrDefaultAsync(k => k.ID == id);

            if (kullanıcı == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı bulunamadı!";
                return RedirectToAction("KullanıcıYönetimi");
            }

            // Şu anki tarih
            var currentDate = DateTime.Now;

            // Katıldığı ve katılacağı etkinlikleri belirle
            var katildigiEtkinlikler = kullanıcı.KatildigiEtkinlikler
                .Where(ke => ke.Etkinlik.Tarih < currentDate)
                .Select(ke => ke.Etkinlik)
                .ToList();

            var katilacagiEtkinlikler = kullanıcı.KatildigiEtkinlikler
                .Where(ke => ke.Etkinlik.Tarih >= currentDate)
                .Select(ke => ke.Etkinlik)
                .ToList();

            // Kullanıcının ilgi alanlarını al
            var userIlgiAlanlari = await _context.KullaniciIlgiAlanlari
                .Where(ka => ka.KullanıcıID == id)
                .Select(ka => ka.IlgiAlaniID)
                .ToListAsync();

            // Tüm ilgi alanlarını al
            var tumIlgiAlanlari = await _context.IlgiAlanlari.ToListAsync();

            // ViewBag üzerinden bilgileri geç
            ViewBag.KatildigiEtkinlikler = katildigiEtkinlikler;
            ViewBag.KatilacagiEtkinlikler = katilacagiEtkinlikler;
            ViewBag.TumIlgiAlanlari = tumIlgiAlanlari;
            ViewBag.SeciliIlgiAlanlari = userIlgiAlanlari;

            // Kullanıcı detaylarını görünüme gönder
            return View("~/Views/KullanıcıProfil/KullaniciProfil2.cshtml", kullanıcı);
        }


    }




    public class EtkinlikRaporViewModel
    {
        public string EtkinlikAdi { get; set; }
        public int KatılımcıSayisi { get; set; }
        public double OrtalamaPuan { get; set; }
        public int ToplamYildizPuani { get; set; }
        public int YildizPuaniSayisi { get; set; }
        public int FavoriSayisi { get; set; } // Favori Sayısı
    }


    public class KullaniciRaporViewModel
    {
        public string KullaniciAdi { get; set; }
        public int KatildigiEtkinlikSayisi { get; set; }
        public int ToplamPuan { get; set; }
        public string EnAktif { get; set; } // "Evet" veya "Hayır"
        public string EnYuksekPuan { get; set; } // "Evet" veya "Hayır"
    }
}