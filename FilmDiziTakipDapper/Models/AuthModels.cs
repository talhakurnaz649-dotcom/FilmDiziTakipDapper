using System.ComponentModel.DataAnnotations;

namespace FilmDiziTakipDapper.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı boş bırakılamaz.")]
        [Display(Name = "Kullanıcı Adı")]
        public string KullanıcıAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre boş bırakılamaz.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı boş bırakılamaz.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3 ila 50 karakter arasında olmalıdır.")]
        [Display(Name = "Kullanıcı Adı")]
        public string KullanıcıAdi { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre boş bırakılamaz.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Şifre en az 4 karakter uzunluğunda olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;
    }
}
