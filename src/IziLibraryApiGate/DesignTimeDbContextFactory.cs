using IziLibrary.Database.DataBase.EfCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace IziLibraryApiGate
{
    public class DesignTimeDbContextService : IDesignTimeServices
    {
        public void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            //serviceCollection.AddSingleton<IProviderCodeGenerator, CustomCodeGenerator>();
        }
    }
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IziProjectsDbContext>
    {
        public IziProjectsDbContext CreateDbContext(string[] args)
        {
            var dbContextBuilder = new DbContextOptionsBuilder<IziProjectsDbContext>();
            dbContextBuilder.ConfigureWithIziSpecifics();
            return new IziProjectsDbContext(dbContextBuilder.Options);
        }
    }
}
