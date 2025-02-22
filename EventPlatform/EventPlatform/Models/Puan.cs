namespace Yazlab2.Models
{
   
        public class Puan
        {
            public int ID { get; set; }
            public int KullanıcıID { get; set; }
            public Kullanıcı Kullanıcı { get; set; }
            public int Puanlar { get; set; }
            public DateTime KazanilanTarih { get; set; }
        }
    

}
