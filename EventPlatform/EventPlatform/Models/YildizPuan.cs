namespace Yazlab2.Models
{
    public class YildizPuan
    {
        public int ID { get; set; } // Birincil anahtar

        // Kullanıcı ile ilişki
        public int KullanıcıID { get; set; }
        public Kullanıcı Kullanıcı { get; set; }

        // Yıldız puanı
        public int Puan { get; set; } // 1-5 arasında bir puan

        // İlgili etkinlik (nullable değil çünkü her zaman bir etkinlik ile ilişkili olacak)
        public int EtkinlikID { get; set; }
        public Etkinlik Etkinlik { get; set; }

        // Gönderim zamanı
        public DateTime GonderimZamani { get; set; } = DateTime.Now; // Varsayılan olarak şu anki zamanı ayarla
    }
}
