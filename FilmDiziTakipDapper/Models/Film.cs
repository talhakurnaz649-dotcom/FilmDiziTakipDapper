using System.ComponentModel.DataAnnotations;

namespace FilmDiziTakipDapper.Models
{
    public class Film
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Film veya Dizi adı boş bırakılamaz.")]
        [Display(Name = "İçerik Adı")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen bir tür seçin.")]
        [Display(Name = "İçerik Türü")]
        public int TurId { get; set; }

        // Mapped from Tur table in JOIN queries
        [Display(Name = "Tür")]
        public string? TurAd { get; set; }

        [Range(1800, 2100, ErrorMessage = "Geçerli bir yıl girin (1800-2100).")]
        [Display(Name = "Yayın Yılı")]
        public int Yil { get; set; }

        [Required(ErrorMessage = "Yönetmen alanı boş bırakılamaz.")]
        [Display(Name = "Yönetmen / Yaratıcı")]
        public string Yonetmen { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Format")]
        public string TurSınıfı { get; set; } = "Film"; // Film or Dizi

        [Required]
        [Display(Name = "İzleme Durumu")]
        public string Durum { get; set; } = "Izlenecek"; // Izlenecek, Izleniyor, Izlendi

        [Range(0, 10, ErrorMessage = "Puan 0 ile 10 arasında olmalıdır.")]
        [Display(Name = "Kişisel Puan")]
        public int Puan { get; set; }

        [Display(Name = "Kişisel Yorum")]
        public string? Yorum { get; set; }

        [Url(ErrorMessage = "Lütfen geçerli bir URL girin.")]
        [Display(Name = "Afiş Resim URL")]
        public string? AfisUrl { get; set; }

        [Required(ErrorMessage = "Lisans maliyeti boş bırakılamaz.")]
        [Display(Name = "Lisans / Satın Alma Maliyeti ($)")]
        [Range(0, 10000000, ErrorMessage = "Maliyet 0'dan küçük olamaz.")]
        public decimal Maliyet { get; set; }

        [Required(ErrorMessage = "Bilet fiyatı boş bırakılamaz.")]
        [Display(Name = "Bilet Satış Fiyatı ($)")]
        [Range(0, 10000, ErrorMessage = "Bilet fiyatı 0'dan küçük olamaz.")]
        public decimal BiletFiyati { get; set; }

        [Required(ErrorMessage = "Vergi tutarı boş bırakılamaz.")]
        [Display(Name = "Bilet Başına Vergi ($)")]
        [Range(0, 1000, ErrorMessage = "Vergi 0'dan küçük olamaz.")]
        public decimal Vergi { get; set; }
    }
}
