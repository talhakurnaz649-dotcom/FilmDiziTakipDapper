using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FilmDiziTakipDapper.Models;

namespace FilmDiziTakipDapper.Controllers
{
    public class BiletController : Controller
    {
        private readonly string _connectionString;

        public BiletController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("DefaultConnection connection string is missing.");
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId") != null;
        }

        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        // GET: Bilet (Biletlerim / Satışlar)
        public async Task<IActionResult> Index()
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            using (var conn = GetConnection())
            {
                string sql;
                List<Bilet> biletler;

                if (IsAdmin())
                {
                    sql = @"
                        SELECT b.*, f.Ad as FilmAd, u.KullanıcıAdi as KullanıcıAdi 
                        FROM Biletler b 
                        INNER JOIN Filmler f ON b.FilmId = f.Id 
                        INNER JOIN AppUsers u ON b.AppUserId = u.Id 
                        ORDER BY b.Id DESC";
                    biletler = (await conn.QueryAsync<Bilet>(sql)).ToList();
                }
                else
                {
                    sql = @"
                        SELECT b.*, f.Ad as FilmAd 
                        FROM Biletler b 
                        INNER JOIN Filmler f ON b.FilmId = f.Id 
                        WHERE b.AppUserId = @UserId 
                        ORDER BY b.Id DESC";
                    biletler = (await conn.QueryAsync<Bilet>(sql, new { UserId = GetUserId() })).ToList();
                }

                return View(biletler);
            }
        }

        // GET: Bilet/SelectSeat?filmId=5
        [HttpGet]
        public async Task<IActionResult> SelectSeat(int filmId)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            using (var conn = GetConnection())
            {
                var sqlFilm = "SELECT * FROM Filmler WHERE Id = @Id";
                var film = await conn.QuerySingleOrDefaultAsync<Film>(sqlFilm, new { Id = filmId });

                if (film == null)
                {
                    return NotFound();
                }

                var sqlSeats = "SELECT KoltukNo FROM Biletler WHERE FilmId = @FilmId";
                var bookedSeats = (await conn.QueryAsync<string>(sqlSeats, new { FilmId = filmId })).ToList();

                ViewBag.Film = film;
                ViewBag.BookedSeats = bookedSeats;

                return View();
            }
        }

        // GET: Bilet/Checkout?filmId=5&seatNo=A5
        [HttpGet]
        public async Task<IActionResult> Checkout(int filmId, string seatNo)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            using (var conn = GetConnection())
            {
                var sqlFilm = "SELECT * FROM Filmler WHERE Id = @Id";
                var film = await conn.QuerySingleOrDefaultAsync<Film>(sqlFilm, new { Id = filmId });

                if (film == null)
                {
                    return NotFound();
                }

                // Verify seat is not already taken
                var checkSql = "SELECT COUNT(1) FROM Biletler WHERE FilmId = @FilmId AND KoltukNo = @KoltukNo";
                var isTaken = await conn.ExecuteScalarAsync<int>(checkSql, new { FilmId = filmId, KoltukNo = seatNo });
                if (isTaken > 0)
                {
                    TempData["ErrorMessage"] = "Seçtiğiniz koltuk başkası tarafından alındı. Lütfen yeni koltuk seçin.";
                    return RedirectToAction("SelectSeat", new { filmId = filmId });
                }

                var model = new Bilet
                {
                    FilmId = filmId,
                    FilmAd = film.Ad,
                    KoltukNo = seatNo,
                    BiletFiyati = film.BiletFiyati,
                    AppUserId = GetUserId()
                };

                ViewBag.Film = film;
                return View(model);
            }
        }

        // POST: Bilet/Purchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(Bilet model)
        {
            if (!IsLoggedIn()) return Unauthorized();

            model.AppUserId = GetUserId();

            using (var conn = GetConnection())
            {
                // Double check seat availability
                var checkSql = "SELECT COUNT(1) FROM Biletler WHERE FilmId = @FilmId AND KoltukNo = @KoltukNo";
                var isTaken = await conn.ExecuteScalarAsync<int>(checkSql, new { FilmId = model.FilmId, KoltukNo = model.KoltukNo });

                if (isTaken > 0)
                {
                    ModelState.AddModelError("", "Bu koltuk başka bir kullanıcı tarafından satın alındı.");
                    
                    var film = await conn.QuerySingleOrDefaultAsync<Film>("SELECT * FROM Filmler WHERE Id = @Id", new { Id = model.FilmId });
                    ViewBag.Film = film;
                    return View("Checkout", model);
                }

                if (!ModelState.IsValid)
                {
                    var film = await conn.QuerySingleOrDefaultAsync<Film>("SELECT * FROM Filmler WHERE Id = @Id", new { Id = model.FilmId });
                    ViewBag.Film = film;
                    return View("Checkout", model);
                }

                // Create ticket record
                var sqlInsert = @"
                    INSERT INTO Biletler (FilmId, AppUserId, IzleyiciAdSoyad, KoltukNo, BiletFiyati, Telefon, Adres, Tarih) 
                    VALUES (@FilmId, @AppUserId, @IzleyiciAdSoyad, @KoltukNo, @BiletFiyati, @Telefon, @Adres, GETDATE());
                    SELECT SCOPE_IDENTITY();";

                var newId = await conn.ExecuteScalarAsync<int>(sqlInsert, model);

                // Fetch details for success view
                var successSql = @"
                    SELECT b.*, f.Ad as FilmAd 
                    FROM Biletler b 
                    INNER JOIN Filmler f ON b.FilmId = f.Id 
                    WHERE b.Id = @Id";
                
                var savedBilet = await conn.QuerySingleAsync<Bilet>(successSql, new { Id = newId });

                return View("Success", savedBilet);
            }
        }
    }
}
