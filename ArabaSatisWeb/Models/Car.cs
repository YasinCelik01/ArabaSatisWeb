using System;
using System.Collections.Generic;

namespace ArabaSatisWeb.Models
{
    public class Car
    {
        public int ArabaId { get; set; } // Benzersiz araç ID'si
        public string Marka { get; set; } // Araç markası
        public string Model { get; set; } // Araç modeli
        public int Yil { get; set; } // Üretim yılı
        public decimal Fiyat { get; set; } // Araç fiyatı
        public int Kilometre { get; set; } // Kilometre
        public string YakitTuru { get; set; } // Yakıt türü
        public string VitesTuru { get; set; } // Vites türü
        public string KasaTuru { get; set; } // Araç tipi
        public decimal MotorHacmi { get; set; } // Motor hacmi
        public string Renk { get; set; } // Renk
        public string Aciklama { get; set; } // Araç açıklaması
        public DateTime KayitTarihi { get; set; } = DateTime.Now; // Kayıt tarihi
        public int UserId { get; set; }
        public string Telefon { get; set; }
        // İlişkili fotoğraflar
        public List<CarPhoto> FotografListesi { get; set; } = new List<CarPhoto>();

        // Admin onayı
        public int Onay { get; set; } = 0; // Onaylanmamış olarak başlar (0 = onaysız, 1 = onaylı)
    }
}
