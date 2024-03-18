using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using IziHardGames.FileSystem.NetStd21;

namespace IziHardGames.Projects.Sln
{
    public class SlnProjects
    {
        private readonly List<SlnProjectRecord> projects = new List<SlnProjectRecord>();
        public static Guid cpsCsProjectGuid = Guid.Parse("9A19103F-16F7-4668-BE54-9A1E7A4F7556");
        /// <summary>
        /// https://github.com/VISTALL/visual-studio-project-type-guids
        /// </summary>
        public static Guid SolutionFolder = Guid.Parse("2150E333-8FDC-42A3-9474-1A3956D46DE8");

        internal void Add(FileInfo sln, string line)
        {
            SlnProjectRecord record = new SlnProjectRecord(sln, line);
            projects.Add(record);
        }

        internal void Add(FileInfo sln, InfoCsproj csproj)
        {
            var line = $"Project(\"{cpsCsProjectGuid.ToString("B")}\") = \"{csproj.ProjectName}\", \"{UtilityForPath.AbsToRelative(sln.Directory!, csproj.FileInfo!.FullName)}\", \"{csproj.GuidStruct.ToString("B")}\"\r\nEndProject\r\n";
            Add(sln, line);
        }

        public override string ToString()
        {
            if (projects.Count > 0)
            {
                return projects.Select(x => x.ToString()).Aggregate((x, y) => x + y)!;
            }
            return string.Empty;
        }

        internal SlnProjectRecord? GetByGuid(Guid guid)
        {
            return projects.FirstOrDefault(x => x.Guid == guid);
        }
        internal SlnProjectRecord? GetByGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            return projects.FirstOrDefault(x => x.Guid == Guid.Parse(guid));
        }
        internal SlnProjectRecord? GetByPath(string pathAbs)
        {
            return projects.FirstOrDefault(x => x.PathAbs == pathAbs);
        }

        internal void Remove(SlnProjectRecord byGuid)
        {
#if DEBUG
            if (!projects.Contains(byGuid)) throw new NullReferenceException($"Not founded. {byGuid.ToStringInfo()}");
#endif
            projects.Remove(byGuid);

        }
    }
}