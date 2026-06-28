namespace FilmDiziTakipDapper.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string KullanıcıAdi { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SifreHASH { get; set; } = string.Empty;
        public string Rol { get; set; } = "Kullanıcı";
        public bool Aktifmi { get; set; } = true;
    }
}
