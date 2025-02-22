# Etkinlik Planlama Platformu

Bu proje, kullanıcıların etkinlik oluşturabileceği, katılabileceği ve etkinlikler etrafında sosyal etkileşim kurabileceği bir **Etkinlik Planlama Platformu**dur. Kullanıcılar, kural tabanlı kişiselleştirilmiş etkinlik önerileri alabilir, sohbet edebilir ve etkinlikleri harita üzerinden takip edebilir.

Bu proje **ASP.NET Core MVC** ile geliştirilmiştir ve harita entegrasyonu için **OpenStreetMap API** kullanılmıştır.

---

## Teknolojiler ve Araçlar
- **Backend:** ASP.NET Core MVC
- **Frontend:** HTML, CSS, JavaScript 
- **Veritabanı:** Microsoft SQL Server
- **Harita Entegrasyonu:** OpenStreetMap API

---

## Ana Özellikler

### Kullanıcı Yönetimi
- Kullanıcı kayıt, giriş ve profil güncelleme
- Şifre sıfırlama ve doğrulama
- Kullanıcı rolleri (Admin / Kullanıcı)

### Etkinlik Yönetimi
- Etkinlik oluşturma, düzenleme ve silme
- Etkinlik detaylarını listeleme
- Zaman çakışma algoritması ile etkinlik çakışmalarını engelleme

### Kural Tabanlı Etkinlik Öneri Sistemi
- Kullanıcı ilgi alanlarına dayalı etkinlik önerileri
- Katılım geçmişine göre kişiselleştirilmiş öneriler
- Konum bazlı etkinlik tavsiyeleri

### Harita ve Rota Planlama
- **OpenStreetMap API** entegrasyonu
- Kullanıcılar etkinlik konumlarını harita üzerinde görebilir
- Kullanıcılar etkinliğe giden en uygun rotayı hesaplayabilir

### Mesajlaşma ve Bildirimler
- Kullanıcılar etkinlik sayfalarında mesajlaşabilir
- Anlık mesaj bildirimi ve mesaj geçmişi

### Oyunlaştırma Sistemi
- Kullanıcılar etkinliklere katıldıkça puan kazanabilir
- Kullanıcı puanları profil sayfasında gösterilir

### Admin Paneli
- Etkinlikleri onaylama, düzenleme ve silme yetkisi
- Kullanıcı hesaplarını yönetme
- Sistem ayarları ve raporlama

## API Kullanımı
Proje, **OpenStreetMap API** ile harita ve rota planlama hizmetlerini kullanmaktadır. API entegrasyonu Leaflet.js gibi kütüphaneler aracılığıyla gerçekleştirilmiştir.

- **Etkinlik Konumu Görüntüleme:**
  Kullanıcılar etkinlik detay sayfalarında harita üzerinden etkinlik konumunu görebilir.
- **Rota Planlama:**
  Kullanıcılar, mevcut konumlarından etkinliğe en uygun rotayı hesaplayabilir.

---

## **Kurulum ve Çalıştırma**  

### **1. Projeyi Klonlayın**  

Aşağıdaki komutları terminal veya komut istemcisine girerek projeyi klonlayın:  

```sh
git clone https://github.com/zeynepkou/EventPlatform.git
cd  EventPlatform
```

### **2. Uygulamayı Çalıştırın**  

**Visual Studio'da `F5` tuşuna basarak** veya **terminalde aşağıdaki komutu çalıştırarak** uygulamayı başlatabilirsiniz:  

```sh
dotnet run
```

---

## **Ekran Görüntüleri** 

![Resim](https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_1.png)

![Resim](https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_2.png)

![Resim](https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_11.png)

<table>
  <tr>
    <td><img src="https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_3.png"/></td>
    <td><img src="https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_4.png"/></td>
  </tr>
  <tr>
    <td><img src="https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_5.png"/></td>
    <td><img src="https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_6.png"/></td>
  </tr>
  <tr>
    <td><img src="https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_7.png"/></td>
    <td><img src="https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_8.png"/></td>
  </tr>
  <tr>
    <td><img src="https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_9.png"/></td>
    <td><img src="https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_10.png"/></td>
  </tr>
  <tr>
    <td><img src="https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_12.png"/></td>
    <td><img src="https://github.com/zeynepkou/EventPlatform/blob/c64c00b24ef4b754f94b2bcc2aeef33d11dce921/images2/EventPlatform_13.png"/></td>
  </tr>
</table>
