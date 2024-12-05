using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.DotNetProjects;
using IziHardGames.Environments;

namespace IziHardGames.IziProjectsManager.Common.Dtos
{

    public class CsprojDto
    {
        public CsprojId Guid { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public IEnumerable<DeviceDto> Devices { get; set; } = Enumerable.Empty<DeviceDto>();
        public IEnumerable<TagDto> Tags { get; set; } = Enumerable.Empty<TagDto>();
        public IEnumerable<string> Paths { get; set; } = Enumerable.Empty<string>();
    }
}
