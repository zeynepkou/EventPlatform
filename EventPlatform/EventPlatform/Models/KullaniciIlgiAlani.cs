namespace Yazlab2.Models
{
    public class KullaniciIlgiAlani
    {
        public int KullanıcıID { get; set; }
        public Kullanıcı Kullanıcı { get; set; }

        public int IlgiAlaniID { get; set; }
        public IlgiAlani IlgiAlani { get; set; }
    }
}
