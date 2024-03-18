using System;

namespace IziHardGames.Projects.Sln
{
    public class SlnGlobalSection
    {
        protected int start;
        protected int end;
        protected ReadOnlyMemory<char> mem;
        public string Value => mem.Span.ToString();
        protected string newLine;

        public SlnGlobalSection(string newLine)
        {
            this.newLine = newLine;
        }

        ///
        public virtual void SetSection(int start, int end, ReadOnlyMemory<char> readOnlyMemory)
        {
            this.start = start;
            this.end = end;
            this.mem = readOnlyMemory;
        }

        public override string ToString()
        {
            return $"\t{mem.Span.ToString()}{newLine}";
        }
    }
}