using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Construction;

namespace IziHardGames.DotNetProjects.Extensions
{
    public static class ExtensionsForProjectRootElement
    {
        public static void RemoveProjectReferences(this ProjectRootElement root)
        {
            foreach (var grp in root.ItemGroups)
            {
                foreach (var item in grp.Items)
                {
                    if (item.ItemType == nameof(ECsprojTag.ProjectReference))
                    {
                        grp.RemoveChild(item);
                    }
                }
            }
        }
        public static IEnumerable<ProjectItemElement> GetProjectReferences(this ProjectRootElement root)
        {
            foreach (var grp in root.ItemGroups)
            {
                foreach (var item in grp.Items)
                {
                    if (item.ItemType == nameof(ECsprojTag.ProjectReference))
                    {
                        yield return item;
                    }
                }
            }
        }

        /// <returns><see langword="false"/> - отсутствовал <see cref="Guid"/> но был создан</returns>
        public static (bool, CsprojId) EnsureGuid(this ProjectRootElement root)
        {
            if (root.TryGetTag(ECsprojTag.ProjectGuid, out var prop))
            {
                ArgumentNullException.ThrowIfNull(prop);
                if (Guid.TryParse(prop.Value.Trim(), out var guid))
                {
                    return (true, (CsprojId)guid);
                }
            }
            var newGuid = Guid.NewGuid();
            root.EnsureTag(ECsprojTag.ProjectGuid, newGuid.ToString(IziProjectsConstants.GUID_FORMAT));
            return (false, (CsprojId)newGuid);
        }

        public static bool EnsureAuthor(this ProjectRootElement root, string value)
        {
            return root.EnsureTag(ECsprojTag.Author, value);
        }

        public static bool EnsureCompany(this ProjectRootElement root, string value)
        {
            return root.EnsureTag(ECsprojTag.Company, value);
        }

        public static bool EnsureTag(this ProjectRootElement root, ECsprojTag tag, string value)
        {
            if (root.TryGetTag(tag, out var prop))
            {
                ArgumentNullException.ThrowIfNull(prop);

                if (prop!.HasValue(tag, value))
                {
                    return true;
                }
                else
                {
                    prop.SetValue(tag, value);
                }
            }
            else
            {
                root.AddProperty(tag.ToString(), value);
            }
            return false;
        }
        public static bool TryGetTag(this ProjectRootElement root, ECsprojTag tag, out ProjectPropertyElement? property)
        {
            foreach (var prop in root.Properties)
            {
                if (prop.IsTag(tag))
                {
                    property = prop;
                    return true;
                }
            }
            property = null;
            return false;
        }
        public static bool HasTag(this ProjectRootElement root, ECsprojTag tag)
        {
            return root.Properties.Any(x => x.IsTag(tag));
        }

        public static void AddProjectReference(this ProjectRootElement root, string include, CsprojId? childId)
        {
            if (childId.HasValue)
            {
                var item = root.AddItem(nameof(ECsprojTag.ProjectReference), include);
                item.AddMetadata(name: CsprojProjectReferenceRequiredMetas.TAG_REF_PROJECT_GUID, childId.ToString(), false);
            }
        }

        public static void FormatPathsToRelativeWithEnvVariables(this ProjectRootElement root)
        {
            foreach (var grp in root.ItemGroups)
            {
                foreach (var item in grp.Items)
                {
                    if (item.ItemType == nameof(ECsprojTag.ProjectReference))
                    {
                        var include = item.Include;
                        if (IziEnvironmentsHelper.TryReplacePathWithEnvVariables(include, out var result))
                        {
                            item.Include = result;
                        }
                    }
                }
            }
        }
    }
}
