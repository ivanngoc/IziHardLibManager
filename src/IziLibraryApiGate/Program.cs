using IziHardGames.IziLibrary.Commands.AtDataBase;
using IziHardGames.Projects;
using IziHardGames.Projects.DataBase;
using Npgsql;

namespace IziLibraryApiGate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var uid = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_USER_DEV");
            var pwd = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PASSWORD_DEV");
            var server = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_SERVER_DEV");
            var port = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PORT_DEV");
            var portVal = $";port={port}";

            var cs = $"server={server};uid={uid};pwd={pwd}{(port is null ? string.Empty : portVal)};database=IziProjectsDbContext";


            var npsqlCsb = new NpgsqlConnectionStringBuilder(cs);

            builder.Services.AddControllers();
            //builder.Services.AddDbContextPool<IziProjectsDbContext>(x => x.UseNpgsql(npsqlCsb.ConnectionString));
            builder.Services.AddDbContext<ModulesDbContextV1>();
            builder.Services.AddDbContext<ModulesDbContextV2>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            IServiceConfig.Configure(builder.Services);

            builder.Services.AddSingleton<FillDatabaseWithAsmdef>();

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
