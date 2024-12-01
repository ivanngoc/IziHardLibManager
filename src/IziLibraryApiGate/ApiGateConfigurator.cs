using System.Reflection;
using IziLibrary.Database.DataBase.EfCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IziLibraryApiGate
{
    public static class ExtensionsForDbContextOptionsBuilder
    {
        public static void ConfigureWithIziSpecifics(this DbContextOptionsBuilder optionsBuilder)
        {
            // Add services to the container.
            var uid = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_USER_DEV");
            var pwd = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PASSWORD_DEV");
            var server = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_SERVER_DEV");
            var port = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PORT_DEV");
            var portVal = $";port={port}";

            var cs = $"server={server};uid={uid};pwd={pwd}{(port is null ? string.Empty : portVal)};database={nameof(IziProjectsDbContext)}; Include Error Detail=true";

            var npsqlCsb = new NpgsqlConnectionStringBuilder(cs);

            optionsBuilder.UseNpgsql(npsqlCsb.ConnectionString, opt =>
            {
                //var asm = Assembly.GetEntryAssembly();
                var asm = Assembly.GetAssembly(typeof(Program));
                ArgumentNullException.ThrowIfNull(asm);
                var name = asm.GetName().Name;
                opt.MigrationsAssembly(name);
            }).EnableSensitiveDataLogging();
        }
    }

    public class ApiGateConfigurator : IDbConfigurator
    {
        public void Configure(DbContextOptionsBuilder optionsBuilder)
        {
            // Add services to the container.
            var uid = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_USER_DEV");
            var pwd = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PASSWORD_DEV");
            var server = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_SERVER_DEV");
            var port = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PORT_DEV");
            var portVal = $";port={port}";

            var cs = $"server={server};uid={uid};pwd={pwd}{(port is null ? string.Empty : portVal)};database={nameof(IziProjectsDbContext)}; Include Error Detail=true";


            var npsqlCsb = new NpgsqlConnectionStringBuilder(cs);

            optionsBuilder.UseNpgsql(npsqlCsb.ConnectionString, opt =>
            {
                var asm = Assembly.GetEntryAssembly();
                ArgumentNullException.ThrowIfNull(asm);
                var name = asm.GetName().Name;
                opt.MigrationsAssembly(name);
            });
        }
    }
}
