using System;
using System.IO;
using IziHardGames.DotNetProjects.Extensions;
using Microsoft.Build.Construction;

namespace IziHardGames.DotNetProjects
{
    public class Csproj : ICsproj
    {
        public FileInfo FileInfo { get; set; }
        public string FilePathAbsolute { get => FileInfo.FullName; set => throw new NotSupportedException(); }

        public Csproj(FileInfo fileInfo)
        {
            this.FileInfo = fileInfo;
        }

        public bool TryGetGuid(out CsprojId guid)
        {
            ProjectRootElement root = global::Microsoft.Build.Construction.ProjectRootElement.Open(FilePathAbsolute);
            if (root.TryGetTag(ECsprojTag.ProjectGuid, out var prop))
            {
                ArgumentNullException.ThrowIfNull(prop);
                if (Guid.TryParse(prop.Value, out var guidRaw))
                {
                    guid = (CsprojId)guidRaw;
                    return true;
                }
            }
            guid = default;
            return false;
        }
    }
}
