using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MooreHotelAndSuites.Infrastructure.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
          
            var apiProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "../MooreHotelAndSuites.API");

            var config = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = config.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Design;
// using MooreHotelAndSuites.Infrastructure.Data;

// namespace MooreHotelAndSuites.Infrastructure.Data
// {
//     public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
//     {
//         public AppDbContext CreateDbContext(string[] args)
//         {
//             var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

//             optionsBuilder.UseNpgsql(
//                 "Host=ep-square-cherry-a883j1zp.eastus2.azure.neon.tech;" +
//                 "Port=5432;" +
//                 "Database=MooreHotelAndSuite;" +
//                 "Username=neondb_owner;" +
//                 "Password=npg_yI9Jovi3Sjtg;" +
//                 "Ssl Mode=Require;" +
//                 "Trust Server Certificate=true;",
//                 b => b.MigrationsAssembly("MooreHotelAndSuites.Infrastructure")
//             );

//             return new AppDbContext(optionsBuilder.Options);
//         }
//     }
// }