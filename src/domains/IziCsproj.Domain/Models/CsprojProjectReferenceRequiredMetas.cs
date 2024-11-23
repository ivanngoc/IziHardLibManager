namespace IziHardGames.DotNetProjects
{
    public class CsprojProjectReferenceRequiredMetas
    {
        public CsprojId CsprojId { get; set; }

        /// <summary>
        /// Значение пути из тэга <ProjectReference> аттрибута Include
        /// </summary>
        public string Include { get; set; } = string.Empty;
        public string IncludeLocation { get; set; } = string.Empty;
    }
}