using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        public IEnumerable<string> GetIncludes()
        {
            ProjectRootElement root = global::Microsoft.Build.Construction.ProjectRootElement.Open(FilePathAbsolute);

            foreach (var projReference in root.GetProjectReferences())
            {
                var include = projReference.Include;
                var path = projReference.GetIncludePath(this.FileInfo);
                yield return path;
            }
        }

        public async Task SetChilds(IEnumerable<CsprojRelationAtDevice> childs)
        {
            ProjectRootElement root = global::Microsoft.Build.Construction.ProjectRootElement.Open(FilePathAbsolute);

            root.RemoveProjectReferences();

            foreach (var item in childs.OrderBy(x => x.Include))
            {
                root.AddProjectReference(item.Include, item.Relation.ChildId);
            }
            root.Save(System.Text.Encoding.UTF8);
        }
    }
}
