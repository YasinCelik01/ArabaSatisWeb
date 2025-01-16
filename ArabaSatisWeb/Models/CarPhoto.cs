using System;

namespace ArabaSatisWeb.Models
{
    public class CarPhoto
    {
        public int FotografId { get; set; } // Benzersiz fotoğraf ID'si
        public int ArabaId { get; set; } // İlişkili araç ID'si
        public string Fotograf { get; set; }
        public string FotografUrl { get; set; } // Fotoğrafın URL'si
        public bool VarsayilanFotograf { get; set; } = false; // Varsayılan fotoğraf mı?
        public DateTime KayitTarihi { get; set; } = DateTime.Now; // Kayıt tarihi

        // İlişkili araç
        public Car Araba { get; set; }
    }
}
