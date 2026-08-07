using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace Messenger.DB
{
    internal class ApplicationContext : DbContext
    {
        public DbSet<Users> Users { get; set; } = null!;
        public DbSet<ChatRooms> ChatRooms { get; set; } = null!;

        public ApplicationContext()
        {
            // Схема БД теперь управляется через EF Core Migrations, а не через EnsureCreated():
            //   dotnet ef migrations add <Название>
            //   dotnet ef database update
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            string connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Строка подключения 'DefaultConnection' не найдена. Скопируй appsettings.Example.json в appsettings.json и заполни своими данными.");
            }

            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
