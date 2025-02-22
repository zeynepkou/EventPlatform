namespace Yazlab2.Models
{
    public class Kategori
    {
        public int ID { get; set; }
        public string Ad { get; set; } // Örneğin: Resim, Sinema vb.
        public int IlgiAlaniID { get; set; }
        public IlgiAlani IlgiAlani { get; set; }
    }
}
