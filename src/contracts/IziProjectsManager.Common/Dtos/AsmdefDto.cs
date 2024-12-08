using System.Collections.Generic;
using System.Linq;
using IziHardGames.Asmdefs;

namespace IziHardGames.IziProjectsManager.Common.Dtos
{
    public class AsmdefDto
    {
        public AsmdefId Guid { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public IEnumerable<DeviceDto> Devices { get; set; } = Enumerable.Empty<DeviceDto>();
        public IEnumerable<TagDto> Tags { get; set; } = Enumerable.Empty<TagDto>();
        public IEnumerable<string> Paths { get; set; } = Enumerable.Empty<string>();
    }
}
