using System;

namespace IziHardGames.Projects.Sln
{
    public class SlnSectionExtensibilityGlobals : SlnGlobalSection
    {
        private string guidAsString = string.Empty;
        private Guid guid;

        public SlnSectionExtensibilityGlobals(string newLine) : base(newLine)
        {

        }
        public SlnSectionExtensibilityGlobals(string newLine, Guid guid) : base(newLine)
        {
            this.guid = guid;
            this.guidAsString = guid.ToString("B");
            SetSection(-1, -1, ToString(guid).AsMemory());
        }

        public Guid Guid => guid;

        public override void SetSection(int start, int end, ReadOnlyMemory<char> readOnlyMemory)
        {
            base.SetSection(start, end, readOnlyMemory);
            Parse(readOnlyMemory);
        }
        private void Parse(ReadOnlyMemory<char> readOnlyMemory)
        {
            var splits = readOnlyMemory.ToString().Split(newLine);
            var count = splits.Length - 1;

            if (count > 3) throw new NotImplementedException();

            for (int i = 1; i < count; i++)
            {
                var line = splits[i].Trim();
                if (line.StartsWith("SolutionGuid"))
                {
                    this.guidAsString = line.Split('=')[1].Trim();
                    this.guid = Guid.Parse(guidAsString);
                }
            }
        }

        public string ToString(Guid guid)
        {
            string s =
                $"\tGlobalSection(ExtensibilityGlobals) = postSolution{newLine}" +
                $"\t\tSolutionGuid = {guid.ToString("B")}{newLine}" +
                $"\tEndGlobalSection{newLine}";
            return s;
        }

        public override string ToString()
        {
            return ToString(this.guid);
        }

        public void SetGuid(Guid guid)
        {
            this.guid = guid;
            this.guidAsString = guid.ToString("B");
        }
    }
}