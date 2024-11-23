using System.Collections.Generic;
using IziLibrary.Database.DataBase.EfCore;

namespace IziHardGames.DotNetProjects
{
    public class EntityCsproj : IziEntity
    {
        public CsprojId EntityCsprojId { get; set; }
        public ICollection<CsProjectAtDevice> CsProjectAtDevices { get; set; } = null!;
    }
}
