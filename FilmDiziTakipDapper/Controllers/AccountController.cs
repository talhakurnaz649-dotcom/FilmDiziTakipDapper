using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FilmDiziTakipDapper.Models;
using System.Security.Cryptography;
using System.Text;

namespace FilmDiziTakipDapper.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("DefaultConnection connection string is missing.");
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var conn = GetConnection())
            {
                var sql = "SELECT * FROM AppUsers WHERE KullanıcıAdi = @KullanıcıAdi AND Aktifmi = 1";
                var user = await conn.QuerySingleOrDefaultAsync<AppUser>(sql, new { KullanıcıAdi = model.KullanıcıAdi });

                if (user != null)
                {
                    var hashedInput = HashPassword(model.Sifre);
                    if (user.SifreHASH == hashedInput)
                    {
                        // Save login info to session
                        HttpContext.Session.SetInt32("UserId", user.Id);
                        HttpContext.Session.SetString("Username", user.KullanıcıAdi);
                        HttpContext.Session.SetString("Role", user.Rol);

                        return RedirectToAction("Index", "Film");
                    }
                }

                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var conn = GetConnection())
            {
                // Check if user exists
                var checkSql = "SELECT COUNT(1) FROM AppUsers WHERE KullanıcıAdi = @Username OR Email = @Email";
                var count = await conn.ExecuteScalarAsync<int>(checkSql, new { Username = model.KullanıcıAdi, Email = model.Email });

                if (count > 0)
                {
                    ModelState.AddModelError("", "Bu kullanıcı adı veya e-posta adresi zaten kullanımda.");
                    return View(model);
                }

                // Register user
                var registerSql = @"
                    INSERT INTO AppUsers (KullanıcıAdi, Email, SifreHASH, Rol, Aktifmi) 
                    VALUES (@KullanıcıAdi, @Email, @SifreHASH, 'Kullanıcı', 1)";

                var hashedSifre = HashPassword(model.Sifre);
                
                await conn.ExecuteAsync(registerSql, new {
                    KullanıcıAdi = model.KullanıcıAdi,
                    Email = model.Email,
                    SifreHASH = hashedSifre
                });

                // Auto login
                var getUserSql = "SELECT * FROM AppUsers WHERE KullanıcıAdi = @Username";
                var user = await conn.QuerySingleAsync<AppUser>(getUserSql, new { Username = model.KullanıcıAdi });

                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.KullanıcıAdi);
                HttpContext.Session.SetString("Role", user.Rol);

                return RedirectToAction("Index", "Film");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Film");
        }
    }
}
