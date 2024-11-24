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

        public IEnumerable<CsprojProjectReferenceRequiredMetas> GetIncludesMetas()
        {
            ProjectRootElement root = global::Microsoft.Build.Construction.ProjectRootElement.Open(FilePathAbsolute);
            foreach (var projReference in root.GetProjectReferences())
            {
                var metas = projReference.GetMetas();

                if (!metas.IsDefault())
                {
                    yield return metas;
                }
            }
        }
        public IEnumerable<ProjectItemElement> GetProjectReferences()
        {
            ProjectRootElement root = global::Microsoft.Build.Construction.ProjectRootElement.Open(FilePathAbsolute);

            foreach (var projReference in root.GetProjectReferences())
            {
                //var include = projReference.Include;
                //var path = projReference.GetIncludePath(this.FileInfo);
                //yield return path;
                yield return projReference;
            }
        }

        public async Task ReSetChilds(IEnumerable<CsprojRelationAtDevice> childs)
        {
            ProjectRootElement root = global::Microsoft.Build.Construction.ProjectRootElement.Open(FilePathAbsolute);
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            root.RemoveProjectReferences();

            foreach (var item in childs.OrderBy(x => x.Include))
            {
                var targetProj = item.Relation.Child?.CsProjectAtDevices.Where(x => x.DeviceId == idDevice).FirstOrDefault();
                if (targetProj != null)
                {
                    root.AddProjectReference(targetProj.PathAbs, item.Relation.ChildId);
                }
            }
            root.Save(System.Text.Encoding.UTF8);
        }

        public async Task FormatAllProjectReferencesPathToRelativeAsync()
        {
            ProjectRootElement root = global::Microsoft.Build.Construction.ProjectRootElement.Open(FilePathAbsolute);
            root.FormatPathsToRelativeWithEnvVariables();
            root.Save();
            await Task.CompletedTask;
        }
    }
}
