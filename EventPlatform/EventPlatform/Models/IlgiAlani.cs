namespace Yazlab2.Models
{
    public class IlgiAlani
    {
        public int ID { get; set; }
        public string Ad { get; set; } // Örneğin: SANAT, SPOR vb.
                                       // Kategoriler ile ilişki
        public ICollection<Kategori> Kategoriler { get; set; } = new List<Kategori>();
    }
}
