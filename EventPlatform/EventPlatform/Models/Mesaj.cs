namespace Yazlab2.Models
{
    
        public class Mesaj
        {
            public int ID { get; set; }
            public int GondericiID { get; set; }
            public Kullanıcı Gonderici { get; set; }
            public string MesajMetni { get; set; }
            public DateTime GonderimZamani { get; set; }
            public Kullanıcı Kullanıcı { get; set; }

            public int? EtkinlikID { get; set; }
            public int? AliciID { get; set; } // Nullable hale getirildi
            public Kullanıcı? Alici { get; set; } // Alici için ilişkisel yapı da nullable oldu
            public int KullanıcıID { get; set; } // Kullanıcı ile ilişkilendirme
            public bool IsGeneralChat { get; set; } // Genel sohbet mesajlarını ayırt etmek için
    }
    

}
