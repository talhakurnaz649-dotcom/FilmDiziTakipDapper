using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FilmDiziTakipDapper.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FilmDiziTakipDapper.Controllers
{
    public class FilmController : Controller
    {
        private readonly string _connectionString;

        public FilmController(IConfiguration configuration)
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

        private async Task LoadTurlerDropdown()
        {
            using (var conn = GetConnection())
            {
                var sql = "SELECT * FROM Turler ORDER BY Ad";
                var turler = await conn.QueryAsync<Tur>(sql);
                ViewBag.Turler = new SelectList(turler, "Id", "Ad");
            }
        }

        // GET: Film
        public async Task<IActionResult> Index(string? status, string? format, string? search)
        {
            using (var conn = GetConnection())
            {
                var sql = @"
                    SELECT f.*, t.Ad as TurAd 
                    FROM Filmler f 
                    INNER JOIN Turler t ON f.TurId = t.Id
                    WHERE 1 = 1";

                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(status))
                {
                    sql += " AND f.Durum = @Status";
                    parameters.Add("Status", status);
                }

                if (!string.IsNullOrEmpty(format))
                {
                    sql += " AND f.TurSınıfı = @Format";
                    parameters.Add("Format", format);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    sql += " AND f.Ad LIKE @Search";
                    parameters.Add("Search", "%" + search + "%");
                }

                sql += " ORDER BY f.Id DESC";

                var list = await conn.QueryAsync<Film>(sql, parameters);
                
                ViewBag.SelectedStatus = status;
                ViewBag.SelectedFormat = format;
                ViewBag.SearchTerm = search;

                return View(list.ToList());
            }
        }

        // GET: Film/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction(nameof(Index));
            }

            await LoadTurlerDropdown();
            return View(new Film());
        }

        // POST: Film/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Film model)
        {
            if (!IsAdmin()) return Unauthorized();

            if (!ModelState.IsValid)
            {
                await LoadTurlerDropdown();
                return View(model);
            }

            using (var conn = GetConnection())
            {
                var sql = @"
                    INSERT INTO Filmler (Ad, TurId, Yil, Yonetmen, TurSınıfı, Durum, Puan, Yorum, AfisUrl, Maliyet, BiletFiyati, Vergi) 
                    VALUES (@Ad, @TurId, @Yil, @Yonetmen, @TurSınıfı, @Durum, @Puan, @Yorum, @AfisUrl, @Maliyet, @BiletFiyati, @Vergi)";

                await conn.ExecuteAsync(sql, model);
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Film/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction(nameof(Index));
            }

            using (var conn = GetConnection())
            {
                var sql = "SELECT * FROM Filmler WHERE Id = @Id";
                var film = await conn.QuerySingleOrDefaultAsync<Film>(sql, new { Id = id });

                if (film == null)
                {
                    return NotFound();
                }

                await LoadTurlerDropdown();
                return View(film);
            }
        }

        // POST: Film/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Film model)
        {
            if (!IsAdmin()) return Unauthorized();

            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await LoadTurlerDropdown();
                return View(model);
            }

            using (var conn = GetConnection())
            {
                var sql = @"
                    UPDATE Filmler 
                    SET Ad = @Ad, 
                        TurId = @TurId, 
                        Yil = @Yil, 
                        Yonetmen = @Yonetmen, 
                        TurSınıfı = @TurSınıfı, 
                        Durum = @Durum, 
                        Puan = @Puan, 
                        Yorum = @Yorum, 
                        AfisUrl = @AfisUrl,
                        Maliyet = @Maliyet,
                        BiletFiyati = @BiletFiyati,
                        Vergi = @Vergi
                    WHERE Id = @Id";

                await conn.ExecuteAsync(sql, model);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Film/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction(nameof(Index));

            using (var conn = GetConnection())
            {
                var sql = "DELETE FROM Filmler WHERE Id = @Id";
                await conn.ExecuteAsync(sql, new { Id = id });
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
