using System.Reflection;
using IziHardGames.IziLibrary.ForCsproj;
using IziLibrary.Database.DataBase.EfCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IziProjectsDiscoverWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Add services to the container.
            var uid = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_USER_DEV");
            var pwd = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PASSWORD_DEV");
            var server = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_SERVER_DEV");
            var port = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PORT_DEV");
            var portVal = $";port={port}";

            var cs = $"server={server};uid={uid};pwd={pwd}{(port is null ? string.Empty : portVal)};database={nameof(IziProjectsDbContext)}";


            var npsqlCsb = new NpgsqlConnectionStringBuilder(cs);

            builder.Services.AddDbContextPool<IziProjectsDbContext>(x => x.UseNpgsql(npsqlCsb.ConnectionString, opt =>
            {
                var asm = Assembly.GetEntryAssembly();
                ArgumentNullException.ThrowIfNull(asm);
                opt.MigrationsAssembly(asm.GetName().Name);
            }));

            builder.Services.AddScoped<Dicsover>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
