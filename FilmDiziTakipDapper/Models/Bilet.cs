using System;
using System.ComponentModel.DataAnnotations;

namespace FilmDiziTakipDapper.Models
{
    public class Bilet
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Film/Dizi")]
        public int FilmId { get; set; }

        [Display(Name = "İçerik Adı")]
        public string? FilmAd { get; set; } // JOIN field

        [Required]
        public int AppUserId { get; set; }

        [Display(Name = "Kullanıcı")]
        public string? KullanıcıAdi { get; set; } // JOIN field

        [Required(ErrorMessage = "Lütfen izleyici ad soyad girin.")]
        [Display(Name = "İzleyici Adı Soyadı")]
        public string IzleyiciAdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen koltuk seçimi yapın.")]
        [Display(Name = "Koltuk Numarası")]
        public string KoltukNo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Bilet Fiyatı")]
        public decimal BiletFiyati { get; set; }

        [Required(ErrorMessage = "Telefon numarası boş bırakılamaz.")]
        [Display(Name = "Telefon Numarası")]
        public string Telefon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fatura & İletişim adresi boş bırakılamaz.")]
        [Display(Name = "Fatura Adresi")]
        public string Adres { get; set; } = string.Empty;

        [Display(Name = "Satın Alınma Tarihi")]
        public DateTime Tarih { get; set; } = DateTime.Now;
    }
}
