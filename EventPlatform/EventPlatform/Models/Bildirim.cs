using Microsoft.AspNetCore.Mvc;

namespace Yazlab2.Models
{
    public class Bildirim
    {
        public int ID { get; set; }
        public int KullanıcıID { get; set; } // Bildirim alacak kullanıcı
        public string Mesaj { get; set; } // Bildirim metni
        public DateTime Tarih { get; set; } // Bildirim zamanı
        public bool OkunduMu { get; set; } // Bildirim durumu (okundu/okunmadı)
    }


}
