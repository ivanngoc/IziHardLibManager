using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Construction;

namespace IziHardGames.DotNetProjects.Extensions
{
    public static class ExtensionsForProjectPropertyElement
    {
        public static void SetValue(this ProjectPropertyElement property, ECsprojTag projectGuid, string value)
        {
            property.Value = value;
        }
        public static bool HasValue(this ProjectPropertyElement property, ECsprojTag projectGuid, string value)
        {
            return property.Value == value;
        }
        public static bool IsTag(this ProjectPropertyElement property, ECsprojTag tag)
        {
            if (property.ElementName.Equals(tag.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
