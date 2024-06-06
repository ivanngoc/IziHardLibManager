using System;

namespace IziHardGames.Projects
{
	public class InfoItem
    {
        public Guid Guid => System.Guid.Parse(guid);
        public string elementName = string.Empty;
        public string guid = string.Empty;
        public string pathToItemAbsolute = string.Empty;
        public string pathToItemRelative = string.Empty;
        public string assemblyName = string.Empty;
        public int order;

        public bool isGuidFinded;
        public bool isRelativePath;
        public ERefType refType;

        public string ToStringInfo()
        {
            return $"{nameof(InfoItem)}. {nameof(isGuidFinded)}:{isGuidFinded}; {nameof(guid)}:{guid}; {nameof(pathToItemAbsolute)}:{pathToItemAbsolute}";
        }
    }
}