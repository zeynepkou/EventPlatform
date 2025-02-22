namespace Yazlab2.Models
{
    public class AdminIndexViewModel
    {
        public List<Etkinlik> OnayBekleyenEtkinlikler { get; set; }
        public List<Etkinlik> OnaylanmisEtkinlikler { get; set; }

        public List<Etkinlik> SilinmisEtkinlikler { get; set; } // Silinmiş etkinlikler
    }
}


