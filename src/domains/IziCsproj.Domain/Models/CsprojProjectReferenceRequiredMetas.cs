using System;

namespace IziHardGames.DotNetProjects
{
    public class CsprojProjectReferenceRequiredMetas
    {
        //public const string TAG_REF_PROJECT_GUID = "IziProjId";
        public const string TAG_REF_PROJECT_GUID = "Project";
        public CsprojId? CsprojId { get; set; }
        /// <summary>
        /// Значение пути из тэга <ProjectReference> аттрибута Include
        /// </summary>
        public string Include { get; set; } = string.Empty;
        public string IncludeLocation { get; set; } = string.Empty;

        public bool IsDefault()
        {
            return (!CsprojId.HasValue || CsprojId == default) && string.IsNullOrEmpty(Include) && string.IsNullOrEmpty(IncludeLocation);
        }
    }
}