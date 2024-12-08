using IziHardGames.Asmdefs;
using IziHardGames.Asmdefs.Contracts;
using IziHardGames.DotNetProjects;
using IziHardGames.IziLibrary.Commands.AtDataBase;
using IziHardGames.Projects;
using IziHardGames.Projects.DataBase;
using IziLibrary.Database.DataBase.EfCore;
using IziMetas.Application;
using Metas.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IziLibraryApiGate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // если прокинуть его как синглтон, то все равно в конфигурацию AddDbContextPool будет пробрасываться другой инстанс
            var optBuilder = new DbContextOptionsBuilder<IziProjectsDbContext>();


            //builder.Services.AddCors(x => x.AddPolicy(policy => policy.));
            builder.Services.AddSingleton<IDbConfigurator, ApiGateConfigurator>();
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null; // Use PascalCase
                options.JsonSerializerOptions.DictionaryKeyPolicy = null;  // For dictionary keys
            });
            builder.Services.AddDbContextPool<IziProjectsDbContext>(x => x.ConfigureWithIziSpecifics());

            //builder.Services.AddDbContext<ModulesDbContextV1>();
            //builder.Services.AddDbContext<ModulesDbContextV2>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddCors(options => options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            }));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGenWithSpecificOfIziHardGames();
            builder.Services.AddScoped<ICsproSearcher, CsprojSearcher>();
            builder.Services.AddScoped<IMetaSearcher, MetaSearcher>();
            builder.Services.AddScoped<ICsprojProcessor, CsprojProcessor>();
            builder.Services.AddScoped<ICsprojSaver, CsprojSaver>();
            builder.Services.AddScoped<IAsmdefSearcher, AsmdefSearcher>();

            IServiceConfig.Configure(builder.Services);

            builder.Services.AddSingleton<FillDatabaseWithAsmdef>();

            var app = builder.Build();

            app.UseRouting();
            app.UseCors("AllowAll");
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerWithSpecificOfIziHardGames();
            }

            //app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var context = services.GetRequiredService<IziProjectsDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
            }

            app.Run();
        }
    }
}
