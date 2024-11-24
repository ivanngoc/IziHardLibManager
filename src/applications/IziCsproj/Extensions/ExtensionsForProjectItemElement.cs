using System.Collections;
using System.IO;
using IziHardGames.FileSystem.NetStd21;
using Microsoft.Build.Construction;

namespace IziHardGames.DotNetProjects.Extensions
{
    public static class ExtensionsForProjectItemElement
    {
        public static CsprojProjectReferenceRequiredMetas EnsureMetas(this ProjectItemElement projectItemElement)
        {
            var res = new CsprojProjectReferenceRequiredMetas()
            {
                Include = projectItemElement.Include,
                IncludeLocation = projectItemElement.IncludeLocation.LocationString,
                CsprojId = default,
            };
            return res;
        }

        public static string GetIncludePath(this ProjectItemElement element, FileInfo fileInfo)
        {
            var pathAbs = IziEnvironmentsHelper.GetActualAbsolutePath(element.Include, fileInfo.Directory?.FullName);
            return pathAbs;
        }

        public static CsprojProjectReferenceRequiredMetas GetMetas(this ProjectItemElement projectItemElement)
        {
            var metas = new CsprojProjectReferenceRequiredMetas();
            foreach (var metaItem in projectItemElement.Metadata)
            {
                if (metaItem.ElementName == CsprojProjectReferenceRequiredMetas.TAG_REF_PROJECT_GUID)
                {
                    if (System.Guid.TryParse(metaItem.Value, out var guid))
                    {
                        metas.CsprojId = (CsprojId)guid;
                    }
                }
            }
            return metas;
        }
    }
}
