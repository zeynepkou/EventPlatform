using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Yazlab2.Models
{
    public class Kullanıcı
    {
        public int ID { get; set; }
        public string KullaniciAdi { get; set; }
        public string Sifre { get; set; }
        public string Eposta { get; set; }
        public string Konum { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public DateTime DogumTarihi { get; set; }
        public string? Cinsiyet { get; set; }
        public string TelefonNumarasi { get; set; }
        public string? ProfilFotografiYolu { get; set; } = string.Empty;

        // Şifre sıfırlama işlemleri için gerekli özellikler
        public string? ResetToken { get; set; } // Şifre sıfırlama tokeni
        public DateTime? ResetTokenExpireDate { get; set; } // Tokenin geçerlilik süresi
        public string KullaniciTuru { get; set; } = "Kullanıcı";

        // İlişkiler
        public ICollection<IlgiAlani>? IlgiAlanlari { get; set; }
        public List<Katılımcı> KatildigiEtkinlikler { get; set; } = new List<Katılımcı>();
        public List<Mesaj> GonderilenMesajlar { get; set; } = new List<Mesaj>();
        public List<Mesaj> AlinanMesajlar { get; set; } = new List<Mesaj>();
        public List<Puan> Puanlar { get; set; } = new List<Puan>();

        // Yeni Özellik: Kullanıcının İlgi Alanları
        public ICollection<KullaniciIlgiAlani> KullaniciIlgiAlanlari { get; set; } = new List<KullaniciIlgiAlani>();

        // Yeni özellik: Favoriler
        public ICollection<Favori>? Favoriler { get; set; } // Kullanıcı ile Favori ilişkisi
    }

}
