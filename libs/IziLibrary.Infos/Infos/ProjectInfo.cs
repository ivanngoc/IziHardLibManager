namespace IziHardGames.Projects
{
	public class ProjectInfo
    {
        private InfoCsproj proj;
        private OldInfoAsmdef item;
        public bool IsPaired => proj != null && item != null;

        public ProjectInfo(InfoCsproj proj, OldInfoAsmdef item)
        {
            this.proj = proj;
            this.item = item;
        }
    }
}