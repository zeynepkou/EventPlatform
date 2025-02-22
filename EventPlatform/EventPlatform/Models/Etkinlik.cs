namespace Yazlab2.Models
{
    
        public class Etkinlik
        {
            public int ID { get; set; }
            public string EtkinlikAdi { get; set; }
            public string Aciklama { get; set; }
            public DateTime Tarih { get; set; }
            public TimeSpan Saat { get; set; }
            public TimeSpan EtkinlikSuresi { get; set; }
            public string Konum { get; set; }
            public int EtkinlikDurum { get; set; } // 1 = Admin tarafından eklenen, 2 = Kullanıcı tarafından eklenen onay bekleyen, 3 = Admin tarafından onaylanan

            public int KullanıcıID { get; set; }
            public Kullanıcı Kullanıcı { get; set; } // Etkinliği oluşturan kullanıcı ile ilişki
            public ICollection<Mesaj> Mesajlar { get; set; }

            public ICollection<GeriBildirim> GeriBildirimler { get; set; } // Geri Bildirimlerle ilişki
                                                                           // İlişkiler
            public List<Katılımcı>? Katılımcılar { get; set; } =null;

            // Yeni özellik: Etkinlik resmi
            public string? EtkinlikResmi { get; set; } = null; // Etkinlik resmi için dosya yolu

            public int ToplamYildizPuani { get; set; } // Etkinlik için toplam yıldız puanı
            public int YildizPuaniSayisi { get; set; } // Yıldız puanı veren kullanıcı sayısı

            public double OrtalamaPuan { get; set; }
        // Yeni özellik: Yıldız puanları
            public ICollection<YildizPuan> YildizPuanlar { get; set; } // Etkinlik ile yıldız puan ilişkisi

        // Yeni özellik: Favori sayısı
            public int FavoriSayisi { get; set; } = 0; // Varsayılan olarak 0   

        // Yeni özellik: Favoriler
            public ICollection<Favori>? Favoriler { get; set; } // Etkinlik ile Favori ilişkisi


            // Etkinliğin kategorisi
            public int KategoriID { get; set; }
            public Kategori Kategori { get; set; }

    }

}
