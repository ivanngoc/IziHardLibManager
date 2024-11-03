using IziHardGames.IziLibrary.Metas.ForAsmdef;
using IziHardGames.Libs.IziLibrary.Contracts;
using IziProjectsManager.DataBase;
using Microsoft.EntityFrameworkCore;
using static IziHardGames.Projects.DataBase.ConstantsForIziProjects;


namespace IziHardGames.Projects.DataBase
{
    public class ModulesDbContextV2 : DbContext, IDataBaseAdapter
    {
        public DbSet<ModelAsmdef> Asmdefs { get; set; }
        public ModulesDbContextV2()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString);
        }
    }
}
