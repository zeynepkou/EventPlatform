namespace Yazlab2.Models 
{ 
    public class Favori 
    { 
        public int ID { get; set; } 
                // Birincil anahtar public
        public int KullanıcıID { get; set; } // Kullanıcı ile ilişki
        public Kullanıcı Kullanıcı { get; set; } 
        public int EtkinlikID { get; set; } // Etkinlik ile ilişki
                                            
        public Etkinlik Etkinlik { get; set; } 
        public DateTime FavoriEklemeTarihi { get; set; } = DateTime.Now; // Favori ekleme tarihi (opsiyonel)
                                                                         
    } 
}