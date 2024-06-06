using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

namespace IziHardGames.Projects.Sln
{
    public class SlnMappedFile
    {
        const string keyWordProject = "Project";
        const string keyWordProjectEnd = "EndProject";

        const string keyWordGlobal = "Global";
        const string keyWordGlobalEnd = "EndGlobal";
        const string keyWordGlobalSection = "GlobalSection";
        const string keyWordGlobalSCP = "(SolutionConfigurationPlatforms)";
        const string keyWordGlobalPCP = "(ProjectConfigurationPlatforms)";
        const string keyWordGlobalEG = "(ExtensibilityGlobals)";
        const string keyWordGlobalSectionEnd = "EndGlobalSection";

        public Guid SolutionGuid => eg?.Guid ?? default;

        private SolutionFile? solutionFile;

        private readonly FileInfo fileInfo;
        private readonly SlnHeader header = new SlnHeader();
        private readonly SlnProjects projects = new SlnProjects();
        private readonly SlnGlobal global = new SlnGlobal();

        private readonly List<SlnGlobalSection> sections = new List<SlnGlobalSection>();
        private SlnSolutionConfigurationPlatforms? scp;
        private SlnSectionProjectConfigurationPlatforms? pcp;
        private SlnSectionExtensibilityGlobals? eg;
        private string lineEnding = Environment.NewLine;

        public SlnMappedFile(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            this.solutionFile = global::Microsoft.Build.Construction.SolutionFile.Parse(fileInfo!.FullName);
        }

        public async Task ExecuteAsync()
        {
            if (fileInfo.Exists)
            {
                var text = await File.ReadAllTextAsync(fileInfo.FullName);
                Parse(text);
            }
            else throw new FileNotFoundException(fileInfo.FullName);
        }
        private void Parse(string text)
        {
            var lineEnding = this.lineEnding = Environment.NewLine;
            if (text.Contains("\r\n"))
            {
                lineEnding = "\r\n";
            }
            else
            {
                lineEnding = "\n";
            }

            var mem = text.AsMemory();
            var span = mem.Span;

            int offsetProjects = -1;
            int offsetProjectsGlobal = default;
            int offset = default;
            int count0 = text.Length - keyWordProject.Length + 1;

            for (int i = 0; i < count0; i++)
            {
                if (mem.Slice(i, keyWordProject.Length).Span.StartsWith(keyWordProject, StringComparison.InvariantCultureIgnoreCase))
                {
                    offsetProjects = i;
                    break;
                }
            }
            if (offsetProjects < 0)
            {
                for (int i = 0; i < text.Length - keyWordGlobal.Length + 1; i++)
                {
                    if (mem.Slice(i, keyWordGlobal.Length).Span.StartsWith(keyWordGlobal, StringComparison.InvariantCultureIgnoreCase))
                    {
                        offsetProjects = i;
                        break;
                    }
                }
            }
            header.Set(text.Substring(0, offsetProjects));

            for (int i = offsetProjects; i < text.Length; i++)
            {
                var slice = mem.Slice(i, keyWordProject.Length);
                if (slice.Span.StartsWith(keyWordProject, StringComparison.InvariantCultureIgnoreCase))
                {
                    int itemProjectBegin = i;
                    for (int j = i + keyWordProject.Length; j < text.Length; j++)
                    {
                        if (mem.Slice(j, keyWordProjectEnd.Length).Span.StartsWith(keyWordProjectEnd, StringComparison.InvariantCultureIgnoreCase))
                        {
                            string line = mem.Slice(itemProjectBegin, j - itemProjectBegin + keyWordProjectEnd.Length).Span.ToString();
                            //Console.WriteLine($"{line}");
                            projects.Add(fileInfo, line);
                            i = j + keyWordProjectEnd.Length;
                            break;
                        }
                    }
                }
                else if (slice.Span.StartsWith(keyWordGlobal, StringComparison.InvariantCultureIgnoreCase))
                {
                    global.SetStart(i);
                    offsetProjectsGlobal = i + keyWordGlobal.Length;
                    break;
                }
            }
            for (int i = offsetProjectsGlobal; i < text.Length; i++)
            {
                int lengthLeftI = text.Length - i;

                if (lengthLeftI >= keyWordGlobalSection.Length && span.Slice(i, keyWordGlobalSection.Length).StartsWith(keyWordGlobalSection, StringComparison.InvariantCultureIgnoreCase))
                {
                    SlnGlobalSection? section = default;
#if DEBUG
                    string debug = mem.Slice(i, mem.Length - i).Span.ToString();
#endif

                    for (int j = i + keyWordGlobalSection.Length; j < text.Length; j++)
                    {
                        int lengthLeftJ = text.Length - j;
                        if (lengthLeftJ >= keyWordGlobalSCP.Length && span.Slice(j, keyWordGlobalSCP.Length).StartsWith(keyWordGlobalSCP, StringComparison.InvariantCultureIgnoreCase))
                        {
                            for (int k = j; k < text.Length - keyWordGlobalSectionEnd.Length + 1; k++)
                            {
                                if (span.Slice(k, keyWordGlobalSectionEnd.Length).StartsWith(keyWordGlobalSectionEnd, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    scp = new SlnSolutionConfigurationPlatforms(lineEnding);
                                    section = scp;
                                    offset = k + keyWordGlobalSectionEnd.Length;
                                    goto NextSection;
                                }
                            }
                            throw new System.NotImplementedException();
                        }
                        else if (lengthLeftJ >= keyWordGlobalSCP.Length && span.Slice(j, keyWordGlobalPCP.Length).StartsWith(keyWordGlobalPCP, StringComparison.InvariantCultureIgnoreCase))
                        {
                            for (int k = j; k < text.Length - keyWordGlobalSectionEnd.Length + 1; k++)
                            {
                                if (span.Slice(k, keyWordGlobalSectionEnd.Length).StartsWith(keyWordGlobalSectionEnd, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    pcp = new SlnSectionProjectConfigurationPlatforms(lineEnding);
                                    section = pcp;
                                    offset = k + keyWordGlobalSectionEnd.Length;
                                    goto NextSection;
                                }
                            }
                            throw new System.NotImplementedException();
                        }
                        else if (lengthLeftJ >= keyWordGlobalEG.Length && span.Slice(j, keyWordGlobalEG.Length).StartsWith(keyWordGlobalEG, StringComparison.InvariantCultureIgnoreCase))
                        {
                            for (int k = j; k < text.Length - keyWordGlobalSectionEnd.Length + 1; k++)
                            {
                                if (span.Slice(k, keyWordGlobalSectionEnd.Length).StartsWith(keyWordGlobalSectionEnd, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    eg = new SlnSectionExtensibilityGlobals(lineEnding);
                                    section = eg;
                                    offset = k + keyWordGlobalSectionEnd.Length;
                                    goto NextSection;
                                }
                            }
                            throw new System.NotImplementedException();
                        }
                        else if (span.Slice(j, keyWordGlobalSectionEnd.Length).StartsWith(keyWordGlobalSectionEnd))
                        {
                            offset = j + keyWordGlobalSectionEnd.Length;
                            goto NextSection;
                        }
                        // skip whitespaces
                        //throw new System.NotImplementedException(mem.Slice(j, mem.Length - j).Span.ToString());
                    }
                    throw new System.NotImplementedException();
                    NextSection:
                    {
                        if (section == null)
                        {
                            section = new SlnGlobalSection(lineEnding);
                        }
                        section.SetSection(i, offset, mem.Slice(i, offset - i));
                        sections.Add(section);
                        i = offset;
                    }
                }
                else if (span.Slice(i, keyWordGlobalEnd.Length).StartsWith(keyWordGlobalEnd, StringComparison.InvariantCultureIgnoreCase))
                {
                    offset = i + keyWordGlobalEnd.Length;
                    global.SetEnd(offset);
                    goto END_GLOBAL;
                }
            }
            throw new System.NotImplementedException();
            END_GLOBAL: { }
            global.SetSlice(mem.Slice(global.Start, global.Length));

            string s = this.ToString();
        }

        public void FindDependecies(List<InfoItem> resultToFill)
        {
            foreach (var projRef in solutionFile!.ProjectsInOrder)
            {
                if (projRef.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat)
                {
                    var guid = System.Guid.Parse(projRef.ProjectGuid);

                    InfoItem infoItem = new InfoItem()
                    {
                        pathToItemAbsolute = projRef.AbsolutePath,
                        pathToItemRelative = projRef.RelativePath,
                        guid = guid.ToString("D"),
                        refType = ERefType.SlnCsproj,
                        // если в .csproj не было установлено PropertyGroup.ProjectGuid то в .sln ставится свой автосгенерированный GUID
                        isGuidFinded = true,
                    };
                    resultToFill.Add(infoItem);
                }
            }
        }


        /// <summary>
        /// Убедиться в исправности ссылки *.csproj в *.sln
        /// </summary>
        /// <param name="sln"></param>
        /// <param name="csproj"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public static async Task EnsureCsprojReferencedAsync(FileInfo sln, FileInfo csproj)
        {
            if (!sln.Exists) throw new FileNotFoundException(sln.FullName);
            if (!csproj.Exists) throw new FileNotFoundException(csproj.FullName);

            var text = await File.ReadAllTextAsync(sln.FullName);
            if (text.Length < keyWordProject.Length + keyWordProject.Length) throw new FormatException();

            InfoCsproj infoCsproj = new InfoCsproj(csproj);
            await infoCsproj.ExecuteAsync();

            SlnMappedFile slnMappedFile = new SlnMappedFile(sln);
            await slnMappedFile.ExecuteAsync();

            if (await slnMappedFile.EnsureProjectAsync(csproj.FullName, infoCsproj).ConfigureAwait(false))
            {
                await slnMappedFile.SaveToFileAsync();
            }
        }

        public void Edit(SolutionFile sln)
        {
            var pc = new ProjectCollection();

            foreach (var item in sln.ProjectsInOrder)
            {
                pc.LoadProject(item.AbsolutePath);
            }
            var parameters = new BuildParameters(pc)
            {
                Loggers = default, //Instance of ILogger instantiated earlier
            };
            var request = new BuildRequestData(
                projectFullPath: default, //Solution file path
                globalProperties: default,
                toolsVersion: null,
                targetsToBuild: default,
                hostServices: null,
                flags: BuildRequestDataFlags.ProvideProjectStateAfterBuild);
            var buildResult = BuildManager.DefaultBuildManager.Build(parameters, request);
        }

        public async Task UpdateProjectAsync(InfoCsproj csproj)
        {
            throw new System.NotImplementedException();
        }
        public async Task RemoveProjectAsync(InfoCsproj csproj)
        {
            throw new System.NotImplementedException();
        }
        public Task AddProjectAsync(string pathAbs)
        {
            FileInfo fileInfo = new FileInfo(pathAbs);
            if (!fileInfo.Exists) throw new FileNotFoundException(pathAbs);
            return AddProjectAsync(fileInfo);
        }
        public Task AddProjectAsync(FileInfo cjproj)
        {
            InfoCsproj csproj = new InfoCsproj(cjproj);
            return AddProjectAsync(csproj);
        }
        public async Task AddProjectAsync(InfoCsproj csproj)
        {
            if (!csproj.IsExecuted) await csproj.ExecuteAsync().ConfigureAwait(false);
            var guid = csproj.GuidStruct;

            projects.Add(fileInfo, csproj);
            if (pcp == null)
            {
                pcp = new SlnSectionProjectConfigurationPlatforms(lineEnding);
                sections.Add(pcp);
            }
            pcp.Add(guid);
        }
        public string GetConfigToInsert(string guid)
        {
            return
                $"{{{guid}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU\r\n" +
                $"{{{guid}}}.Debug|Any CPU.Build.0 = Debug|Any CPU\r\n" +
                $"{{{guid}}}.Release|Any CPU.ActiveCfg = Release|Any CPU\r\n" +
                $"{{{guid}}}.Release|Any CPU.Build.0 = Release|Any CPU";
        }


        /// <summary>
        /// Если существует запись с тем же путем но другим guid => исправляет guid<br/>
        /// Если существует запись с guid но с другим путем => исправляет путь<br/>
        /// Если есть 2 записи в которой совпадает по отдельности guid и путь, объединяет записи в одну с указанными Guid и путем
        /// </summary>
        /// <param name="pathAbsForCsproj"></param>
        /// <param name="guid"></param>
        /// <returns>
        /// <see langword="true"/> - были произведены изменения<br/>
        /// <see langword="false"/> - изменений не было<br/> 
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> EnsureProjectAsync(string pathAbsForCsproj, InfoCsproj csproj)
        {
            var guid = csproj.Guid;
            var byPath = projects.GetByPath(pathAbsForCsproj);
            var byGuid = projects.GetByGuid(guid);

            if (byPath != null && byGuid != null)
            {
                if (byPath == byGuid)
                {
                    return false;
                }
                else
                {
                    projects.Remove(byPath);
                    byGuid.SetPath(pathAbsForCsproj);
                    return true;
                }
            }
            else
            {
                if (byPath == null && byGuid == null)
                {
                    await AddProjectAsync(csproj).ConfigureAwait(false);
                }
                else
                {
                    if (byPath != null)
                    {
                        byPath!.SetGuid(guid);
                    }
                    else
                    {
                        byGuid!.SetPath(pathAbsForCsproj);
                    }
                }
                return true;
            }
        }

        public override string ToString()
        {
            return
                 $"{header.ToString()}" +
                 $"{projects.ToString()}" +
                 $"Global{lineEnding}" +
                 $"{sections.Select(x => x.ToString()).Aggregate((x, y) => x + y)}" +
                 $"EndGlobal{lineEnding}";
        }
        public static async Task Test0()
        {
            await EnsureCsprojReferencedAsync(
                new FileInfo(@"C:\Users\ngoc\Documents\[Unity] Projects\GameProject3\Packages\packs.sln"),
                //new FileInfo(@"C:\Users\ngoc\Documents\[Unity] Projects\GameProject3\Packages\com.izihardgames.user-control-abstractions.lib\izhg.user-control.abstractions.csproj")
                new FileInfo(@"C:\Users\ngoc\Documents\[Projects] C#\IziHardGamesProxy\com.izihardgames.tasking.pack\Pack\netstd21\izhg.tasking.netstd21.csproj")
                );

        }
        public static async Task Test1()
        {
            SlnMappedFile slnMappedFile = new SlnMappedFile(new FileInfo(@"C:\Users\ngoc\Documents\[Unity] Projects\GameProject3\Packages\packs.sln"));
            await slnMappedFile.ExecuteAsync();
        }

        public void OverrideGuid(Guid guid)
        {
            if (eg == null)
            {
                eg = new SlnSectionExtensibilityGlobals(lineEnding, guid);
                sections.Add(eg);
            }
            else
            {
                eg.SetGuid(guid);
            }
        }

        public async Task SaveToFileAsync()
        {
            await File.WriteAllBytesAsync(fileInfo!.FullName, Encoding.UTF8.GetBytes(this.ToString()));
        }
    }


    public static class StringUtil
    {
        public static string GetEnclosedValue(string input, char open, char close)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == open)
                {
                    for (int j = i + 1; j < input.Length; j++)
                    {
                        if (input[j] == close)
                        {
                            return input.Substring(i + 1, j - i - 1);
                        }
                    }
                    break;
                }
            }
            return string.Empty;
        }

        public static string GetEnclosedValue(string input, char openClose)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == openClose)
                {
                    for (int j = i + 1; j < input.Length; j++)
                    {
                        if (input[j] == openClose)
                        {
                            return input.Substring(i + 1, j - i - 1);
                        }
                    }
                    break;
                }
            }
            return string.Empty;
        }


    }
}