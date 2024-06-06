using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects.DataBase;
using Microsoft.Build.Construction;
using Microsoft.EntityFrameworkCore;

namespace IziHardGames.Projects
{

    public static class IziEnsureCsproj
    {
        public static async Task RegenerateByTemplate(DirectoryInfo directoryInfo)
        {
            var list = new List<FileInfo>();
            UtilityForDirectoryInfo.SearchRecursive(directoryInfo, list, (x) => x.Extension == ".csproj");
            string template = await File.ReadAllTextAsync(@"C:\Users\ngoc\Documents\[Unity] Projects\GameProject3\Packages\izhg.csproj.template");

            foreach (var file in list)
            {
                if (file.FullName == @"C:\Users\ngoc\Documents\[Unity] Projects\GameProject3\Packages\com.izihardgames.lib-control.pack\Console\IziProjectsManager.csproj") continue;
                if (file.FullName == @"C:\Users\ngoc\Documents\[Unity] Projects\GameProject3\Packages\com.izihardgames.lib-control.pack\izhg.io.netstd21\izhg.FileSystem.netstd21.csproj") continue;
                if (file.FullName == @"C:\Users\ngoc\Documents\[Unity] Projects\GameProject3\Packages\com.izihardgames.lib-control.pack\Naming\izhg.naming.csproj") continue;
                await File.WriteAllTextAsync(file.FullName, template);
            }
        }
        public static void EnsureProjectName(DirectoryInfo directoryInfo)
        {
            var list = new List<FileInfo>();
            UtilityForDirectoryInfo.SearchRecursive(directoryInfo, list, (x) => x.Extension == ".csproj");

            foreach (var file in list)
            {
                ProjectRootElement project = ProjectRootElement.Open(file.FullName);
                var node = project.Properties.FirstOrDefault(x => x.ElementName == "ProjectName");

                if (node == null)
                {
                    project.AddProperty("ProjectName", file.FileNameWithoutExtension());
                    project.Save();
                }
                else
                {
                    if (string.IsNullOrEmpty(node.Value))
                    {
                        node.Value = file.FileNameWithoutExtension();
                        project.Save();
                    }
                }
            }
        }
        public static async Task EnsureGuidInDataBase()
        {
            using ModulesDbContext context = new ModulesDbContext();
            var all = await context.Csprojs.Include(x => x.Module).ToArrayAsync();

            foreach (var csproj in all)
            {
                var existedGuid = csproj.Module!.Guid;

                ProjectRootElement project = ProjectRootElement.Open(csproj.PathFull);
                var elGuid = project.Properties.FirstOrDefault(x => x.ElementName == "ProjectGuid");

                if (elGuid != null && Guid.TryParse(elGuid.Value, out var guidFromFile))
                {
                    csproj.Module.Guid = guidFromFile;
                }
                else
                {
                    existedGuid = Guid.NewGuid();
                    var guidAsString = existedGuid.ToString("D");
                    if (elGuid == null)
                    {
                        project.AddProperty("ProjectGuid", guidAsString);
                    }
                    else
                    {
                        elGuid.Value = guidAsString;
                    }
                    project.Save();
                    csproj.Module.Guid = existedGuid;
                }
                Console.WriteLine($"Fixed Missed GUID:\t{csproj.PathFull}");
            }
            await context.SaveChangesAsync().ConfigureAwait(false);
        }
        public static void EnsureGuidBySearchingDir(DirectoryInfo directoryInfo)
        {
            var list = new List<FileInfo>();
            UtilityForDirectoryInfo.SearchRecursive(directoryInfo, list, (x) => x.Extension == ".csproj");

            foreach (var file in list)
            {
                ProjectRootElement project = ProjectRootElement.Open(file.FullName);
                var node = project.Properties.FirstOrDefault(x => x.ElementName == "ProjectGuid");

                if (node == null)
                {
                    project.AddProperty("ProjectGuid", Guid.NewGuid().ToString("D"));
                    project.Save();
                }
                else
                {
                    if (string.IsNullOrEmpty(node.Value))
                    {
                        node.Value = Guid.NewGuid().ToString("D");
                        project.Save();
                    }
                }
            }
        }
        public static void EnsureAuthorIsMe(DirectoryInfo directoryInfo)
        {
            var list = new List<FileInfo>();
            UtilityForDirectoryInfo.SearchRecursive(directoryInfo, list, (x) => x.Extension == ".csproj");

            foreach (var file in list)
            {
                ProjectRootElement project = ProjectRootElement.Open(file.FullName);
                var authors = project.Properties.FirstOrDefault(x => x.ElementName == "Authors");

                if (authors == null)
                {
                    project.AddProperty("Authors", "Tran Ngoc Anh");
                    project.Save();
                }
                else
                {
                    if (!authors.Value.Contains("Tran Ngoc Anh", System.StringComparison.InvariantCultureIgnoreCase) || !authors.Value.Contains("IziHardGames", System.StringComparison.InvariantCultureIgnoreCase) || !authors.Value.Contains("izhg", System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        authors.Value = "Tran Ngoc Anh";
                        project.Save();
                    }
                }
            }
        }
        public static async Task EnsureProjectReferenceMetaGuid()
        {
            using ModulesDbContext context = new ModulesDbContext();
            var csprojs = await context.Csprojs.ToArrayAsync().ConfigureAwait(false);

            foreach (var proj in csprojs)
            {
                FileInfo fileInfo = new FileInfo(proj.PathFull);
                InfoCsproj csproj = new InfoCsproj(fileInfo);
                await csproj.ExecuteAsync().ConfigureAwait(false);

                csproj.ForeachProjectReference((link) =>
                {
                    var pathAbs = UtilityForPath.Combine(fileInfo.Directory!, link.Include, Path.DirectorySeparatorChar);
                    FileInfo depFile = new FileInfo(pathAbs);

                    if (File.Exists(pathAbs))
                    {
                        ProjectRootElement project = ProjectRootElement.Open(pathAbs);
                        var propGuid = project.Properties.FirstOrDefault(x => x.ElementName == ConstantsForIziProjects.ForCsproj.PROP_GUID);
                        var projectName = project.Properties.FirstOrDefault(x => x.ElementName == ConstantsForIziProjects.ForCsproj.PROP_PROJ_NAME)?.Value;

                        if (propGuid != null)
                        {
                            var elGuid = link.Metadata.FirstOrDefault(x => string.Equals(x.ElementName, ConstantsForIziProjects.ForCsproj.EL_DEPENDECY_GUID, StringComparison.InvariantCultureIgnoreCase));
                            if (elGuid != null)
                            {
                                elGuid.Value = propGuid.Value;
                            }
                            else
                            {
                                link.AddMetadata(ConstantsForIziProjects.ForCsproj.EL_DEPENDECY_GUID, propGuid.Value);
                            }
                        }
                        var nameToSet = string.IsNullOrEmpty(projectName) ? depFile.FileNameWithoutExtension() : projectName;
                        var elProjectName = link.Metadata.FirstOrDefault(x => string.Equals(x.ElementName, ConstantsForIziProjects.ForCsproj.EL_DEPENDECY_PROJ_NAME, StringComparison.InvariantCultureIgnoreCase));
                        if (elProjectName != null)
                        {
                            elProjectName.Value = nameToSet;
                        }
                        else
                        {
                            link.AddMetadata(ConstantsForIziProjects.ForCsproj.EL_DEPENDECY_PROJ_NAME, nameToSet);
                        }

                        var elTag = link.Metadata.FirstOrDefault(x => string.Equals(x.ElementName, ConstantsForIziProjects.ForCsproj.EL_DEPENDECY_PROJ_TAG, StringComparison.InvariantCultureIgnoreCase));
                        if (elTag != null)
                        {
                            elTag.Value = DateTime.Now.ToString();
                        }
                        else
                        {
                            link.AddMetadata(ConstantsForIziProjects.ForCsproj.EL_DEPENDECY_PROJ_TAG, DateTime.Now.ToString());
                        }
                    }
                });
                csproj.Proj.Save();
            }
        }

        public static Task EnsureUnityDll(string pathFull, Guid guid)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// No Dependecies
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static async Task FormatProjectSingle(FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                // 1. check GUID
                InfoCsproj infoCsproj = new InfoCsproj(fileInfo);
                await infoCsproj.ExecuteAsync().ConfigureAwait(false);
                // 2. Add self meta to DataBase
                using ModulesDbContext context = new ModulesDbContext();
                var existed = context.Csprojs.Include(x => x.Module).FirstOrDefault(x => x.Module!.Guid == infoCsproj.GuidStruct);
                if (existed != null)
                {
                    existed.PathFull = fileInfo.FullName;
                    existed.DateTimeModify = DateTime.Now;
                }
                else
                {
                    await context.AddAsync(infoCsproj).ConfigureAwait(false);
                }
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        public static async Task FormatDepencdecies(FileInfo file)
        {
            InfoCsproj parent = new InfoCsproj(file);
            await parent.ExecuteAsync().ConfigureAwait(false);

            if (parent.IsGuidGenerated)
            {
                throw new System.InvalidOperationException($"Сначала нужно иициализировать проект. см. {nameof(IziEnsureCsproj.FormatProjectSingle)}");
            }

            if (!IziEnsureCsproj.ExistsInDataBaseByGuid(parent))
            {
                throw new System.InvalidOperationException($"Сначала нужно добавить проект в БД");
            }

            using ModulesDbContext context = new ModulesDbContext();
            bool isUpdateFile = false;

            // 3. Pull Update/Dependecies
            foreach (var itemFromFile in parent.Proj.Items)
            {
                var elName = itemFromFile.ElementName.Trim();
                var elNameLI = elName.ToLowerInvariant();
                string path = string.Empty;
                string pathAbs = string.Empty;

                if (elNameLI == ConstantsForIziProjects.ForCsproj.ITEM_DEPEND_PROJ_REF.ToLowerInvariant())
                {
                    var metas = CheckDependecyMeta(itemFromFile);
                    var projectName = metas.projectName;

                    if (itemFromFile.Include.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase))
                    {
                        path = itemFromFile.Include;
                        if (UtilityForPath.IsRelative(path))
                        {
                            pathAbs = UtilityForPath.Combine(file.Directory!, path, Path.DirectorySeparatorChar);
                        }
                        else
                        {
                            pathAbs = path;
                        }
                    }

                    string projectFileName = UtilityForPath.GetFileNameWithoutExtension(pathAbs);

                    var findResult = IziProjectsFinding.FindAndUpdateCsproj(metas.guid, pathAbs, projectFileName, projectName);

                    if (findResult.isExistsAny)
                    {
                        Guid guid = findResult.actualGuid;
                        pathAbs = findResult.actualPathAbs;
                        projectFileName = findResult.actualProjectFileName;
                        projectName = findResult.actualProjectName;

                        FileInfo csprojDependecyInfo = new FileInfo(pathAbs);
                        InfoCsproj child = new InfoCsproj(csprojDependecyInfo);
                        await child.ExecuteAsync();

                        if (EnsureDependecyMeta(metas, guid, projectName, pathAbs, projectFileName))
                        {
                            isUpdateFile = true;
                        }
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
            }

            if (isUpdateFile) parent.Proj.Save();
        }

        private static bool ExistsInDataBaseByGuid(InfoCsproj parent)
        {
            using ModulesDbContext context = new ModulesDbContext();
            if (context.TryFindByGuid(parent.GuidStruct, out var existed))
            {
                return true;
            }
            return false;
        }

        private static MetaBanch CheckDependecyMeta(ProjectItemElement itemFromFile)
        {
            var metaGuid = itemFromFile.Metadata.FirstOrDefault(x => x.ElementName == ConstantsForIziProjects.ForCsproj.EL_DEPENDECY_GUID);
            var metaName = itemFromFile.Metadata.FirstOrDefault(x => x.ElementName == ConstantsForIziProjects.ForCsproj.EL_DEPENDECY_PROJ_NAME);
            var metaTag = itemFromFile.Metadata.FirstOrDefault(x => x.ElementName == ConstantsForIziProjects.ForCsproj.EL_DEPENDECY_PROJ_TAG);
            var banch = new MetaBanch() { elGuid = metaGuid, elName = metaName, elTag = metaTag };
            banch.ReadMetas();
            return banch;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="metaBanch"></param>
        /// <param name="guid"></param>
        /// <param name="projName"></param>
        /// <returns>
        /// <see langword="true"/> - были изменения/исправлеия. файл нужно пересохранить<br/>
        /// </returns>
        private static bool EnsureDependecyMeta(MetaBanch metaBanch, Guid guid, string projName, string pathAbs, string projectFileName)
        {
            throw new System.NotImplementedException();
        }

        public class MetaBanch
        {
            public ProjectMetadataElement? elGuid;
            public ProjectMetadataElement? elName;
            public ProjectMetadataElement? elTag;

            public string projectName;

            public Guid guid;
            public bool isGuidGenerated;

            public bool isValidGuid;
            public bool isValidName;
            public bool isValidTag;

            public bool isNeedToSetName;
            public bool isNeedToSetTag;
            public bool isNeedToSetGuid;

            public bool isSetGuid;
            public bool isSetTag;
            public bool IsChanges => isSetGuid || isSetTag;

            public void ReadMetas()
            {
                if (elGuid != null)
                {
                    if (Guid.TryParse(elGuid.Value, out guid))
                    {
                        isValidGuid = true;
                    }
                    else
                    {
                        isSetGuid = true;
                        guid = Guid.NewGuid();
                        isGuidGenerated = true;
                    }
                }
                else
                {
                    isNeedToSetGuid = true;
                    guid = Guid.NewGuid();
                }

                if (elName != null)
                {
                    if (string.IsNullOrEmpty(elName.Value))
                    {
                        isNeedToSetName = true;
                    }
                    else
                    {
                        projectName = elName.Value;
                        isValidName = true;
                    }
                }
                else
                {
                    isNeedToSetName = false;
                }

                if (elTag != null)
                {
                    if (string.IsNullOrEmpty(elTag.Value))
                    {
                        elTag.Value = DateTime.Now.ToString();
                        isSetTag = true;
                    }
                    else
                    {
                        isValidTag = true;
                    }
                }
                else
                {
                    isNeedToSetTag = true;
                }
            }
        }
    }
}