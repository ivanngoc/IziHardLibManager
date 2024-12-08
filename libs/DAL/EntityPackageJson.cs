namespace IziHardGames.DotNetProjects
{
    public class EntityPackageJson : IziEntity
    {
        public PackageJsonId PackageJsonId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Company { get; set; }
    }
}
