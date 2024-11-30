using Microsoft.EntityFrameworkCore;

namespace IziLibrary.Database.DataBase.EfCore
{
    public interface IDbConfigurator
    {
        void Configure(DbContextOptionsBuilder optionsBuilder);
    }
}
