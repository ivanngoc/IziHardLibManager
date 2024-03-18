using System;
using System.Collections.Generic;
using System.Linq;

namespace IziHardGames.Projects.Sln
{
    public class SlnSectionProjectConfigurationPlatforms : SlnGlobalSection
    {
        private readonly List<Configurations> configurations = new List<Configurations>();

        public SlnSectionProjectConfigurationPlatforms(string newLine) : base(newLine)
        {

        }

        public override void SetSection(int start, int end, ReadOnlyMemory<char> readOnlyMemory)
        {
            base.SetSection(start, end, readOnlyMemory);
            ParseAdditive(in readOnlyMemory);
        }

        public void ParseAdditive(in ReadOnlyMemory<char> mem)
        {
            var lines = mem.Span.ToString().Split(newLine);
            int count = lines.Length - 1;

            for (int i = 1; i < count; i++)
            {
                Configurations configuration = new Configurations();
                configuration.SetValue(lines[i].Trim());
                configurations.Add(configuration);
            }
        }

        public override string ToString()
        {
            return $"\tGlobalSection(ProjectConfigurationPlatforms) = postSolution{newLine}" +
                $"{configurations.Select(x => $"\t\t{x.ToString()}{newLine}").Aggregate((x, y) => x + y)}" +
                $"\tEndGlobalSection{newLine}";
        }

        internal void Add(Guid guid)
        {
            ParseAdditive(ToStringGroup(guid).AsMemory());
        }
        public string ToStringGroup(Guid guid)
        {
            var s = guid.ToString("B");
            return
                $"\tGlobalSection(ProjectConfigurationPlatforms) = postSolution{newLine}" +
                $"\t\t{s}.Debug|Any CPU.ActiveCfg = Debug|Any CPU{newLine}" +
                $"\t\t{s}.Debug|Any CPU.Build.0 = Debug|Any CPU{newLine}" +
                $"\t\t{s}.Release|Any CPU.ActiveCfg = Release|Any CPU{newLine}" +
                $"\t\t{s}.Release|Any CPU.Build.0 = Release|Any CPU{newLine}" +
                $"\tEndGlobalSection";
        }
    }

    public class Configurations
    {
        public string Value => mem.Span.ToString();
        private ReadOnlyMemory<char> mem;
        private Guid guid;
        private string guidAsString = string.Empty;
        private string restOfValue = string.Empty;

        public Configurations()
        {

        }

        internal void SetValue(string trimmedValue)
        {
            mem = trimmedValue.AsMemory();
            int dotIndex = trimmedValue.IndexOf('.');
            var left = trimmedValue.AsSpan().Slice(0, dotIndex);
            var right = trimmedValue.AsSpan().Slice(dotIndex + 1, mem.Length - dotIndex - 1);
            guid = Guid.Parse(left);
            restOfValue = right.ToString();
            this.guidAsString = StringUtil.GetEnclosedValue(left.ToString(), '{', '}');
        }


        public override string ToString()
        {
            return $"{guid.ToString("B")}.{restOfValue}";
        }
    }
}