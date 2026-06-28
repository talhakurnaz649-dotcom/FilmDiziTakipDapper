using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace FilmDiziTakipDapper.Controllers
{
    public class FilmFinansRaporu
    {
        public string FilmAd { get; set; } = string.Empty;
        public decimal Maliyet { get; set; }
        public int BiletSatisAdedi { get; set; }
        public decimal BiletFiyati { get; set; }
        public decimal ToplamGelir { get; set; }
        public decimal ToplamVergi { get; set; }
        public decimal NetKar { get; set; }
    }

    public class RaporController : Controller
    {
        private readonly string _connectionString;

        public RaporController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("DefaultConnection connection string is missing.");
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // GET: Rapor
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Film");
            }

            using (var conn = GetConnection())
            {
                var sql = @"
                    SELECT 
                        f.Ad AS FilmAd, 
                        f.Maliyet AS Maliyet, 
                        COUNT(b.Id) AS BiletSatisAdedi, 
                        f.BiletFiyati AS BiletFiyati,
                        ISNULL(SUM(b.BiletFiyati), 0) AS ToplamGelir,
                        (COUNT(b.Id) * f.Vergi) AS ToplamVergi,
                        (ISNULL(SUM(b.BiletFiyati), 0) - f.Maliyet - (COUNT(b.Id) * f.Vergi)) AS NetKar
                    FROM Filmler f
                    LEFT JOIN Biletler b ON f.Id = b.FilmId
                    GROUP BY f.Id, f.Ad, f.Maliyet, f.BiletFiyati, f.Vergi
                    ORDER BY NetKar DESC";

                var list = await conn.QueryAsync<FilmFinansRaporu>(sql);
                return View(list.ToList());
            }
        }
    }
}
