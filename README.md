# BeeMart POS System

- **Tech Stack & Versi**: .NET 8 (C#) dan Microsoft SQL Server.
- **Database Setup**: Jalankan script SQL pada `BeeMart.Api/Database/BeeMart_database.sql` di SQL Server Management Studio (SSMS) atau sqlcmd untuk membuat tabel DDL dan data awal (seeding).
- **Konfigurasi**: Sesuaikan file `.env` di root proyek (atau `appsettings.json`) untuk connection string database dan kredensial lainnya.
- **Urutan Menjalankan Aplikasi**:
  1. Jalankan Backend API terlebih dahulu: `dotnet run --project BeeMart.Api` (Port: `http://localhost:5100`).
  2. Jalankan Frontend WebClient: `dotnet run --project BeeMart.WebClient` (Port: `http://localhost:5052`).
- **Akun Uji Coba**:
  - `admin` (Role: **Admin**) - Password: `Password123`
  - `steven` (Role: **Cashier**) - Password: `Password123`
  - `gabri` (Role: **Cashier**) - Password: `Password123`
