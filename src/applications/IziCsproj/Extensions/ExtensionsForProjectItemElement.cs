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
    }
}
