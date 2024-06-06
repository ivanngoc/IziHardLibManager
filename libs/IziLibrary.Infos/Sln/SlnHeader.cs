namespace IziHardGames.Projects.Sln
{
	public class SlnHeader
    {
        private string text = string.Empty;
        public string Text => text;

        public void Set(string text)
        {
            this.text = text;
        }

		public override string ToString()
		{
            return text;
		}
	}
}