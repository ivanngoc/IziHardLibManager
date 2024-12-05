using System.Text.Json.Serialization;
using IziHardGames.DotNetProjects;
using Microsoft.EntityFrameworkCore;

namespace IziLibrary.Database.DataBase.EfCore
{
    [Index(nameof(PathAbs), IsUnique = true)]
    public class CsProjectAtDevice
    {
        public Guid DeviceId { get; set; }
        public CsprojId EntityCsprojId { get; set; }
        public string PathAbs { get; set; } = null!;
        [JsonIgnore] public Device Device { get; set; } = null!;
        public EntityCsproj EntityCsproj { get; set; } = null!;

        public string GetFileName()
        {
            return new FileInfo(PathAbs).Name;
        }
    }
}
