using Dapper;
using Microsoft.Data.SqlClient;

namespace FilmDiziTakipDapper.Data
{
    public static class DbInitializer
    {
        public static void InitializeDatabase(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var targetDatabase = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            var masterConnString = builder.ConnectionString;

            using (var masterConn = new SqlConnection(masterConnString))
            {
                masterConn.Open();
                
                var checkDbSql = $"SELECT database_id FROM sys.databases WHERE name = '{targetDatabase}'";
                var dbId = masterConn.QueryFirstOrDefault<int?>(checkDbSql);

                if (dbId == null)
                {
                    var createDbSql = $"CREATE DATABASE [{targetDatabase}]";
                    masterConn.Execute(createDbSql);
                }
            }

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 1. Create Turler Table
                var createTurlerTableSql = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Turler')
                    BEGIN
                        CREATE TABLE Turler (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Ad NVARCHAR(50) NOT NULL UNIQUE
                        )
                    END";
                conn.Execute(createTurlerTableSql);

                // 2. Create Filmler Table
                var createFilmlerTableSql = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Filmler')
                    BEGIN
                        CREATE TABLE Filmler (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Ad NVARCHAR(150) NOT NULL,
                            TurId INT NOT NULL FOREIGN KEY REFERENCES Turler(Id) ON DELETE CASCADE,
                            Yil INT NOT NULL,
                            Yonetmen NVARCHAR(100) NOT NULL,
                            TurSınıfı NVARCHAR(10) NOT NULL, -- Film, Dizi
                            Durum NVARCHAR(15) NOT NULL,    -- Izlenecek, Izleniyor, Izlendi
                            Puan INT NOT NULL DEFAULT 0,
                            Yorum NVARCHAR(MAX) NULL,
                            AfisUrl NVARCHAR(500) NULL
                        )
                    END";
                conn.Execute(createFilmlerTableSql);

                // 3. Add financial columns to Filmler if not exist
                var addFinancialColumnsSql = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Filmler') AND name = 'Maliyet')
                    BEGIN
                        ALTER TABLE Filmler ADD Maliyet DECIMAL(18,2) NOT NULL DEFAULT 0.0;
                        ALTER TABLE Filmler ADD BiletFiyati DECIMAL(18,2) NOT NULL DEFAULT 0.0;
                        ALTER TABLE Filmler ADD Vergi DECIMAL(18,2) NOT NULL DEFAULT 0.0;
                    END";
                conn.Execute(addFinancialColumnsSql);

                // 4. Create AppUsers Table
                var createAppUsersTableSql = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AppUsers')
                    BEGIN
                        CREATE TABLE AppUsers (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            KullanıcıAdi NVARCHAR(50) NOT NULL UNIQUE,
                            Email NVARCHAR(100) NOT NULL UNIQUE,
                            SifreHASH NVARCHAR(200) NOT NULL,
                            Rol NVARCHAR(20) NOT NULL DEFAULT 'Kullanıcı',
                            Aktifmi BIT NOT NULL DEFAULT 1
                        )
                    END";
                conn.Execute(createAppUsersTableSql);

                // 5. Create Biletler Table
                var createBiletlerTableSql = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Biletler')
                    BEGIN
                        CREATE TABLE Biletler (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            FilmId INT NOT NULL FOREIGN KEY REFERENCES Filmler(Id) ON DELETE CASCADE,
                            AppUserId INT NOT NULL FOREIGN KEY REFERENCES AppUsers(Id) ON DELETE CASCADE,
                            IzleyiciAdSoyad NVARCHAR(100) NOT NULL,
                            KoltukNo NVARCHAR(10) NOT NULL,
                            BiletFiyati DECIMAL(18,2) NOT NULL,
                            Telefon NVARCHAR(20) NOT NULL,
                            Adres NVARCHAR(250) NOT NULL,
                            Tarih DATETIME NOT NULL DEFAULT GETDATE()
                        )
                    END";
                conn.Execute(createBiletlerTableSql);

                // Seed Turler
                var countTurler = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Turler");
                if (countTurler == 0)
                {
                    var insertTurlerSql = "INSERT INTO Turler (Ad) VALUES (@Ad)";
                    conn.Execute(insertTurlerSql, new[]
                    {
                        new { Ad = "Bilim Kurgu" },
                        new { Ad = "Dram" },
                        new { Ad = "Aksiyon" },
                        new { Ad = "Komedi" },
                        new { Ad = "Gerilim" },
                        new { Ad = "Belgesel" },
                        new { Ad = "Fantastik" }
                    });
                }

                // Seed Filmler
                var countFilmler = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM Filmler");
                if (countFilmler == 0)
                {
                    var turIdBilimKurgu = conn.ExecuteScalar<int>("SELECT Id FROM Turler WHERE Ad = 'Bilim Kurgu'");
                    var turIdFantastik = conn.ExecuteScalar<int>("SELECT Id FROM Turler WHERE Ad = 'Fantastik'");
                    var turIdDram = conn.ExecuteScalar<int>("SELECT Id FROM Turler WHERE Ad = 'Dram'");

                    var insertFilmSql = @"
                        INSERT INTO Filmler (Ad, TurId, Yil, Yonetmen, TurSınıfı, Durum, Puan, Yorum, AfisUrl, Maliyet, BiletFiyati, Vergi)
                        VALUES (@Ad, @TurId, @Yil, @Yonetmen, @TurSınıfı, @Durum, @Puan, @Yorum, @AfisUrl, @Maliyet, @BiletFiyati, @Vergi)";

                    conn.Execute(insertFilmSql, new[]
                    {
                        new {
                            Ad = "Interstellar",
                            TurId = turIdBilimKurgu,
                            Yil = 2014,
                            Yonetmen = "Christopher Nolan",
                            TurSınıfı = "Film",
                            Durum = "Izlendi",
                            Puan = 10,
                            Yorum = "Uzay ve zaman algısını harika işleyen, müzikleri büyüleyici bir başyapıt.",
                            AfisUrl = "https://images.unsplash.com/photo-1534447677768-be436bb09401?w=500&auto=format&fit=crop",
                            Maliyet = 15000.00m,
                            BiletFiyati = 12.00m,
                            Vergi = 2.00m
                        },
                        new {
                            Ad = "Breaking Bad",
                            TurId = turIdDram,
                            Yil = 2008,
                            Yonetmen = "Vince Gilligan",
                            TurSınıfı = "Dizi",
                            Durum = "Izlendi",
                            Puan = 10,
                            Yorum = "Karakter gelişiminin dizi tarihindeki zirve noktası.",
                            AfisUrl = "https://images.unsplash.com/photo-1594909122845-11baa439b7bf?w=500&auto=format&fit=crop",
                            Maliyet = 25000.00m,
                            BiletFiyati = 15.00m,
                            Vergi = 2.50m
                        },
                        new {
                            Ad = "The Lord of the Rings: The Fellowship of the Ring",
                            TurId = turIdFantastik,
                            Yil = 2001,
                            Yonetmen = "Peter Jackson",
                            TurSınıfı = "Film",
                            Durum = "Izlenecek",
                            Puan = 0,
                            Yorum = "Tekrar tekrar izlenecek efsane fantastik seri başlangıcı.",
                            AfisUrl = "https://images.unsplash.com/photo-1461360370896-922624d12aa1?w=500&auto=format&fit=crop",
                            Maliyet = 8000.00m,
                            BiletFiyati = 10.00m,
                            Vergi = 1.80m
                        }
                    });
                }
                else
                {
                    // Ensure financial columns are seeded/updated for existing films
                    conn.Execute("UPDATE Filmler SET Maliyet = 15000.00, BiletFiyati = 12.00, Vergi = 2.00 WHERE Ad = 'Interstellar' AND Maliyet = 0");
                    conn.Execute("UPDATE Filmler SET Maliyet = 25000.00, BiletFiyati = 15.00, Vergi = 2.50 WHERE Ad = 'Breaking Bad' AND Maliyet = 0");
                    conn.Execute("UPDATE Filmler SET Maliyet = 8000.00, BiletFiyati = 10.00, Vergi = 1.80 WHERE Ad = 'The Lord of the Rings: The Fellowship of the Ring' AND Maliyet = 0");
                }

                // 6. Seed Admin User
                var adminCount = conn.ExecuteScalar<int>("SELECT COUNT(1) FROM AppUsers WHERE KullanıcıAdi = 'admin'");
                if (adminCount == 0)
                {
                    var insertAdminSql = @"
                        INSERT INTO AppUsers (KullanıcıAdi, Email, SifreHASH, Rol, Aktifmi) 
                        VALUES ('admin', 'admin@cinearsiv.com', 'A6xnQhbz4Vx2HuGl4lXwZ5U2I8iziLRFnhP5eNfIRvQ=', 'Admin', 1)";
                    conn.Execute(insertAdminSql);
                }
            }
        }
    }
}
