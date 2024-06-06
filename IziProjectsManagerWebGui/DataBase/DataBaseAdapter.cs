using IziHardGames.Projects.DataBase;
using IziHardGames.Projects.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace IziProjectsManagerWebGui.DataBase
{

    public class DataBaseAdapter : ModulesDbContext
    {
        public DbSet<IziModelCsproj> Models { get; set; }
    }
}
