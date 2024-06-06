namespace IziHardGames.Projects
{
	public class ProjectInfo
    {
        private InfoCsproj proj;
        private InfoAsmdef item;
        public bool IsPaired => proj != null && item != null;

        public ProjectInfo(InfoCsproj proj, InfoAsmdef item)
        {
            this.proj = proj;
            this.item = item;
        }
    }
}