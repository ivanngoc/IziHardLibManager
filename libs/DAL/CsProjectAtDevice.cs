using System.Text.Json.Serialization;
using IziHardGames.DotNetProjects;
using Microsoft.EntityFrameworkCore;

namespace IziLibrary.Database.DataBase.EfCore
{
    [Index(nameof(PathAbs), IsUnique = true)]
    public class CsProjectAtDevice
    {
        public int Id { get; set; }
        public Guid DeviceId { get; set; }
        [JsonIgnore] public Device Device { get; set; } = null!;
        public string PathAbs { get; set; } = null!;
        public CsprojId EntityCsprojId { get; set; }
        public EntityCsproj EntityCsproj { get; set; }
    }
}
