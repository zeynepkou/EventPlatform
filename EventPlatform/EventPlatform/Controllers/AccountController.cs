using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Yazlab2.Data;
using Yazlab2.Models;

namespace Yazlab2.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        // E-posta ayarlarını burada sabit değişkenler olarak tanımlıyoruz
        private readonly string smtpServer = "smtp.gmail.com";
        private readonly int smtpPort = 123; //Buraya port numarası giriniz
        private readonly string senderEmail = ""; //Buraya e-mail adresinizi giriniz 
        private readonly string senderPassword = ""; //Burayada şifrenizi      
        private readonly bool enableSsl = true;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult SifremiUnuttum()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SifremiUnuttum(string eposta)
        {
            Debug.WriteLine("Şifre sıfırlama isteği alındı.");
            Debug.WriteLine("Girilen e-posta adresi: " + eposta);

            // E-posta adresinin boş olup olmadığını kontrol et
            if (string.IsNullOrEmpty(eposta))
            {
                Debug.WriteLine("Girilen e-posta adresi boş.");
                ViewBag.Message = "Lütfen geçerli bir e-posta adresi giriniz.";
                return View();
            }

            var user = _context.Kullanıcılar.FirstOrDefault(u => u.Eposta == eposta);

            if (user != null)
            {
                Debug.WriteLine("Kullanıcı bulundu: " + user.KullaniciAdi);

                var resetToken = Guid.NewGuid().ToString();
                user.ResetToken = resetToken;
                user.ResetTokenExpireDate = DateTime.Now.AddHours(1);

                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("Token ve tarih başarıyla kaydedildi.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Veritabanı hatası: " + ex.Message);
                    ViewBag.Message = "Veritabanı hatası oluştu. Lütfen daha sonra tekrar deneyiniz.";
                    return View();
                }

                string resetUrl = Url.Action("SifreSifirla", "Account", new { token = resetToken }, Request.Scheme);
                Debug.WriteLine("Şifre sıfırlama URL'si oluşturuldu: " + resetUrl);

                string emailBody = $"Merhaba {user.KullaniciAdi},<br><br>" +
                                   $"Şifrenizi sıfırlamak için <a href='{resetUrl}'>buraya tıklayın</a>.<br>" +
                                   $"Bu bağlantı 1 saat içinde geçerliliğini yitirecektir.<br><br>" +
                                   "Saygılarımızla, <br>Akıllı Etkinlik Planlama Platformu";

                try
                {
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(senderEmail);
                        mail.To.Add(user.Eposta);
                        mail.Subject = "Şifre Sıfırlama Talebi";
                        mail.Body = emailBody;
                        mail.IsBodyHtml = true;

                        using (SmtpClient smtp = new SmtpClient(smtpServer, smtpPort))
                        {
                            smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                            smtp.EnableSsl = enableSsl;
                            Debug.WriteLine("E-posta gönderimi başlıyor...");
                            await smtp.SendMailAsync(mail);
                            Debug.WriteLine("E-posta başarıyla gönderildi.");
                        }
                    }

                    ViewBag.Message = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("E-posta gönderim hatası: " + ex.Message);
                    ViewBag.Message = "E-posta gönderiminde bir hata oluştu. Lütfen daha sonra tekrar deneyiniz.";
                }
            }
            else
            {
                Debug.WriteLine("Kullanıcı bulunamadı: " + eposta);
                ViewBag.Message = "Bu e-posta adresine ait bir kullanıcı bulunamadı.";
            }

            return View();
        }


        [HttpGet]
        public IActionResult SifreSifirla(string token)
        {
            Debug.WriteLine("Şifre sıfırlama işlemi başlatıldı. Token: " + token);
            var user = _context.Kullanıcılar.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpireDate > DateTime.Now);

            if (user == null)
            {
                Debug.WriteLine("Geçersiz veya süresi dolmuş token.");
                ViewBag.Message = "Bu bağlantı geçersiz veya süresi dolmuştur.";
                return RedirectToAction("SifremiUnuttum");
            }

            ViewData["Token"] = token;
            Debug.WriteLine("Token geçerli, şifre sıfırlama sayfası açılıyor.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SifreSifirla(string token, string newPassword)
        {
            Debug.WriteLine("Yeni şifre belirleme işlemi başlatıldı. Token: " + token);
            var user = _context.Kullanıcılar.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpireDate > DateTime.Now);

            if (user != null)
            {
                user.Sifre = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.ResetToken = null;
                user.ResetTokenExpireDate = null;

                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    Debug.WriteLine("Şifre başarıyla güncellendi.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Veritabanı hatası: " + ex.Message);
                    ViewBag.Message = "Veritabanı hatası oluştu. Lütfen daha sonra tekrar deneyiniz.";
                    return View();
                }

                ViewBag.Message = "Şifreniz başarıyla güncellenmiştir.";
                return RedirectToAction("Index", "LoginPage");
            }

            Debug.WriteLine("Geçersiz veya süresi dolmuş token.");
            ViewBag.Message = "Bu bağlantı geçersiz veya süresi dolmuştur.";
            return View();
        }
    }
}