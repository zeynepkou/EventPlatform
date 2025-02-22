using Microsoft.EntityFrameworkCore;
using Yazlab2.Models;

namespace Yazlab2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Kullanıcı> Kullanıcılar { get; set; }
        public DbSet<Mesaj> Mesajlar { get; set; }
        public DbSet<Puan> Puanlar { get; set; }
        public DbSet<Etkinlik> Etkinlikler { get; set; }
        public DbSet<Katılımcı> Katılımcılar { get; set; }
        public DbSet<GeriBildirim> GeriBildirimler { get; set; }
        public DbSet<YildizPuan> YildizPuanlar { get; set; } // Yıldız puan tablosu
        public DbSet<Favori> Favoriler { get; set; } // Düzeltme burada!
        public DbSet<Bildirim> Bildirimler { get; set; }
        public DbSet<IlgiAlani> IlgiAlanlari { get; set; } // İlgi Alanları için DbSet tanımı
        public DbSet<Kategori> Kategoriler { get; set; } // Kategoriler için DbSet tanımı
        public DbSet<KullaniciIlgiAlani> KullaniciIlgiAlanlari { get; set; } // Kullanıcı-İlgi Alanı için DbSet



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Kullanıcı ve Mesaj İlişkisi (Gönderilen ve Alınan Mesajlar)
            modelBuilder.Entity<Mesaj>()
                .HasOne(m => m.Gonderici)
                .WithMany(k => k.GonderilenMesajlar)
                .HasForeignKey(m => m.GondericiID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mesaj>()
                .HasOne(m => m.Alici)
                .WithMany(k => k.AlinanMesajlar)
                .HasForeignKey(m => m.AliciID)
                .OnDelete(DeleteBehavior.Restrict);

            // Kullanıcı ve Puan İlişkisi
            modelBuilder.Entity<Puan>()
                .HasOne(p => p.Kullanıcı)
                .WithMany(k => k.Puanlar)
                .HasForeignKey(p => p.KullanıcıID);

            // Katılımcı ve Etkinlik İlişkisi (Çoka-çok)
            modelBuilder.Entity<Katılımcı>()
                .HasKey(k => new { k.KullanıcıID, k.EtkinlikID });

            modelBuilder.Entity<Katılımcı>()
                .HasOne(k => k.Kullanıcı)
                .WithMany(u => u.KatildigiEtkinlikler)
                .HasForeignKey(k => k.KullanıcıID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Katılımcı>()
                .HasOne(k => k.Etkinlik)
                .WithMany(e => e.Katılımcılar)
                .HasForeignKey(k => k.EtkinlikID)
                .OnDelete(DeleteBehavior.Restrict);

             modelBuilder.Entity<GeriBildirim>()
                .HasOne(g => g.Etkinlik)
                .WithMany(e => e.GeriBildirimler)
                .HasForeignKey(g => g.EtkinlikID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GeriBildirim>()
                .HasOne(g => g.Gonderici)
                .WithMany()
                .HasForeignKey(g => g.GondericiID)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);

            // YildizPuan için ilişkiler
            modelBuilder.Entity<YildizPuan>()
                .HasOne(yp => yp.Kullanıcı)
                .WithMany() // Kullanıcının yıldız puanları olabilir (One-to-Many değil, bağımsız ilişki)
                .HasForeignKey(yp => yp.KullanıcıID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<YildizPuan>()
                .HasOne(yp => yp.Etkinlik)
                .WithMany(e => e.YildizPuanlar) // Etkinlik birden fazla puana sahip olabilir
                .HasForeignKey(yp => yp.EtkinlikID)
                .OnDelete(DeleteBehavior.Cascade); // Etkinlik silinirse bağlı puanlar da silinir

            modelBuilder.Entity<Favori>()
                .HasOne(f => f.Kullanıcı)
                .WithMany(k => k.Favoriler)
                .HasForeignKey(f => f.KullanıcıID)
                .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silinirse favorileri sil

            modelBuilder.Entity<Favori>()
                .HasOne(f => f.Etkinlik)
                .WithMany(e => e.Favoriler)
                .HasForeignKey(f => f.EtkinlikID)
                .OnDelete(DeleteBehavior.Restrict); // Etkinlik silinirse favoriler etkilenmez

            // İlgi Alanı ve Kategori İlişkileri
            modelBuilder.Entity<Kategori>()
                .HasOne(k => k.IlgiAlani)
                .WithMany(ia => ia.Kategoriler)
                .HasForeignKey(k => k.IlgiAlaniID);

            // Kullanıcı ve İlgi Alanı (Çoka-Çok İlişki)
            modelBuilder.Entity<KullaniciIlgiAlani>()
                .HasKey(kia => new { kia.KullanıcıID, kia.IlgiAlaniID });

            modelBuilder.Entity<KullaniciIlgiAlani>()
                .HasOne(kia => kia.Kullanıcı)
                .WithMany(k => k.KullaniciIlgiAlanlari)
                .HasForeignKey(kia => kia.KullanıcıID);

            modelBuilder.Entity<KullaniciIlgiAlani>()
                .HasOne(kia => kia.IlgiAlani)
                .WithMany()
                .HasForeignKey(kia => kia.IlgiAlaniID);

            // Etkinlik ve Kategori İlişkisi
            modelBuilder.Entity<Etkinlik>()
                .HasOne(e => e.Kategori)
                .WithMany()
                .HasForeignKey(e => e.KategoriID);

            base.OnModelCreating(modelBuilder);


        }
    }
}
