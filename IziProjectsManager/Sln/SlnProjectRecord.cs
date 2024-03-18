using System;
using System.IO;
using IziHardGames.FileSystem.NetStd21;

namespace IziHardGames.Projects.Sln
{
    public class SlnProjectRecord
    {
        private Guid guid;
        private Guid guidTemplate;
        private bool isRelativePath;


        private FileInfo sln;
        private FileInfo? csproj;
        private string name = string.Empty;
        private string pathAsFounded = string.Empty;
        private string pathAbs = string.Empty;
        private string guidAsString = string.Empty;
        private string line = string.Empty;

        public Guid Guid => guid;
        public string PathAbs => pathAbs;

        public SlnProjectRecord(FileInfo sln, string line)
        {
            this.sln = sln;
            ParseFrom(line);
        }

        public void ParseFrom(string line)
        {
            this.line = line;
            var split0 = line.Split('=');
            var left = split0[0];
            var guidBegin = left.IndexOf('{');
            var guidEnd = left.IndexOf('}');
            string guidTemplateAsString = left.Substring(guidBegin + 1, guidEnd - guidBegin - 1);
            guidTemplate = Guid.Parse(guidTemplateAsString);

            var right = split0[1];
            var split1 = right.Split(',');

            this.name = StringUtil.GetEnclosedValue(split1[0], '"');
            this.pathAsFounded = StringUtil.GetEnclosedValue(split1[1], '"');
            isRelativePath = UtilityForPath.IsRelative(pathAsFounded);
            this.guidAsString = StringUtil.GetEnclosedValue(split1[2], '{', '}');
            csproj = new FileInfo(UtilityForPath.Combine(sln.Directory!, pathAsFounded, Path.DirectorySeparatorChar));
            this.pathAbs = csproj.FullName;
            this.guid = Guid.Parse(guidAsString);
        }

        public override string ToString()
        {
            return $"Project(\"{guidTemplate.ToString("B")}\") = \"{name}\", \"{pathAsFounded}\", \"{guid.ToString("B")}\"\r\nEndProject\r\n";
        }

        internal string ToStringInfo()
        {
            return $"{guid.ToString()}; path:{pathAsFounded}";
        }

        internal void SetPath(string pathAbs)
        {
            this.pathAbs = pathAbs;
            if (isRelativePath)
            {
                this.pathAsFounded = UtilityForPath.AbsToRelative(sln.Directory!, pathAbs);
            }
            else
            {
                this.pathAsFounded = pathAbs;
            }
        }

        internal void SetGuid(string guid)
        {
            this.guid = Guid.Parse(guid);
            guidAsString = this.guid.ToString("B");
        }
    }
}