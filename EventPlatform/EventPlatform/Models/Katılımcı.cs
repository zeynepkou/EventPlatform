namespace Yazlab2.Models
{
    
        public class Katılımcı
        {
            public int KullanıcıID { get; set; }
            public Kullanıcı Kullanıcı { get; set; }

            public int EtkinlikID { get; set; }
            public Etkinlik Etkinlik { get; set; }
        }
    

}
