namespace Yazlab2.Models
{
    public class GeriBildirim
    {
        public int ID { get; set; }
        public int GondericiID { get; set; }
        public Kullanıcı Gonderici { get; set; }
        public string? GeriBidirimMetni { get; set; }
        public DateTime GonderimZamani { get; set; }
        public Kullanıcı Kullanıcı { get; set; }
        public int GeriBildirimDurum { get; set; }
        // İlişkisel Tanımlar
        public int? EtkinlikID { get; set; }
        public Etkinlik Etkinlik { get; set; } // Etkinlik ile ilişki
        public int? AliciID { get; set; } // Alıcı ID'si (nullable olabilir)
        public Kullanıcı? Alici { get; set; } // Alıcı ilişkisi (nullable olabilir)
        public int KullanıcıID { get; set; } // Kullanıcı ile ilişkilendirme
    }

}
