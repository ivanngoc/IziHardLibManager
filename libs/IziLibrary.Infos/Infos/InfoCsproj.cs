using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects.DataBase;
using Microsoft.Build.Construction;

namespace IziHardGames.Projects
{
    public class InfoCsproj : InfoBase
    {
        public const string EXTENSION = ".csproj";
        public const string PROP_PROJ_NAME = "ProjectName";
        public const string PROP_GUID = "ProjectGuid";
        public ProjectRootElement Proj => proj ?? throw new NullReferenceException();
        private ProjectRootElement? proj;
        public InfoAsmdef? infoAsmdef;

        private readonly List<InfoCsproj> nested = new List<InfoCsproj>();
        private readonly List<InfoItem> items = new List<InfoItem>();

        public IEnumerable<InfoItem> Items => items;
        public IEnumerable<InfoCsproj> Nested => nested;
        public bool IsNestingSearched { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        public InfoCsproj(FileInfo info) : base(info)
        {
            targetExtension = ".csproj";
        }
        public override async Task ExecuteAsync()
        {
            ProjectRootElement proj = global::Microsoft.Build.Construction.ProjectRootElement.Open(info!.FullName);
            this.proj = proj;

            var props = proj.Properties;
            this.Content = await File.ReadAllTextAsync(info!.FullName);

            foreach (var item in props)
            {
                if (string.Compare(item.ElementName.Trim(), "ProjectGuid", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    if (System.Guid.TryParse(item.Value, out var existedGuid))
                    {
                        SetGuidFounded(existedGuid.ToString("D"));
                    }
                }
                else if (item.ElementName.Trim() == "Description")
                {
                    this.Description = item.Value;
                }
                else if (item.ElementName.Trim() == "ProjectName")
                {
                    ProjectName = item.Value.Trim();
                }
                Console.WriteLine($"Prop: Element:{item.ElementName}; value:{item.Value}");
            }

            if (string.IsNullOrEmpty(ProjectName))
            {
                ProjectName = FileInfo!.FileNameWithoutExtension();
                proj.AddProperty("ProjectName", ProjectName);
                proj.Save();
            }

            if (!this.isGuidFounded)
            {
                var guid = global::System.Guid.NewGuid();
                proj.AddProperty("ProjectGuid", guid.ToString());
                SetGuidGenerated(guid);
                proj.Save();
            }
            IsExecuted = true;
        }
        public override Task FindDependeciesInFileSystem()
        {
            if (!IsExecuted) throw new InvalidOperationException($"You must call {nameof(ExecuteAsync)}() before this");

            var itemsFromFile = proj!.Items;
            int order = default;

            foreach (var itemFromFile in itemsFromFile)
            {
                string guid = string.Empty;
                bool isGuidFinded = false;
                string pathToItem = string.Empty;
                string assemblyName = string.Empty;
                ERefType refType = ERefType.None;
                bool isAdd = false;
                var elName = itemFromFile.ElementName.Trim();
                var elNameLI = elName.ToLowerInvariant();

                if (elNameLI == "Reference".ToLowerInvariant())
                {
                    isAdd = true;
                    assemblyName = itemFromFile.Include;
                    refType = ERefType.NetSdkReference;
                }
                else if (elNameLI == "ProjectReference".ToLowerInvariant())
                {
                    if (itemFromFile.Include.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isAdd = true;
                        refType = ERefType.NetSdkProjectReference;
                        pathToItem = itemFromFile.Include;
                    }
                }

                if (itemFromFile.HasMetadata)
                {
                    foreach (var meta in itemFromFile.Metadata)
                    {
                        if (meta.ElementName.Trim().ToLowerInvariant() == "guid".ToLowerInvariant())
                        {
                            guid = meta.Value;
                            isGuidFinded = true;
                        }
                        else if (meta.ElementName.Trim().ToLowerInvariant() == "HintPath".ToLowerInvariant())
                        {
                            pathToItem = meta.Value;
                        }
                    }
                }

                if (isAdd)
                {
                    if (string.IsNullOrEmpty(guid)) guid = global::System.Guid.NewGuid().ToString();
                    if (string.IsNullOrEmpty(pathToItem)) throw new FormatException();
                    if (UtilityForPath.IsRelative(pathToItem))
                    {
                        pathToItem = UtilityForPath.Combine(FileInfo!.Directory!, pathToItem, Path.DirectorySeparatorChar);
                    }
                    InfoItem infoItem = new InfoItem()
                    {
                        elementName = elName,
                        guid = guid,
                        pathToItemAbsolute = pathToItem,
                        assemblyName = assemblyName,
                        isGuidFinded = isGuidFinded,
                        refType = refType,
                        order = order,
                    };
                    this.items.Add(infoItem);
                }
                order++;
            }
            return base.FindDependeciesInFileSystem();
        }

        public void SetPairAsmdef(InfoAsmdef item)
        {
            infoAsmdef = item;
            SetPaired(item);
        }

        public bool TryGetAsmdef(out InfoAsmdef? asmdef)
        {
            var result = infoAsmdef != null;
            asmdef = this.infoAsmdef;
            return result;
        }



        private string AbsPath()
        {
            throw new NotImplementedException();
        }

        private string RelativePath()
        {
            throw new NotImplementedException();
        }


        public Task EnsureRefToUnityCore(JsonObject config)
        {
            var absPath = (string)(config["unity_lib"]!["UnityEngine.CoreModule"]!);
            System.IO.FileInfo info = new FileInfo(absPath);
            //proj.AddItem(,);
            //proj.

            throw new NotImplementedException();
        }

        public void AddDll(FileInfo dll)
        {
            StaticAddDll(proj!, FileInfo!, dll);
        }
        public static void StaticAddDll(ProjectRootElement proj, FileInfo csproj, FileInfo target)
        {
            Dictionary<string, string> metas = new Dictionary<string, string>()
            {
                ["HintPath"] = UtilityForDirectoryInfo.GetRelativePath(csproj!.Directory!, target),
            };
            proj!.AddItem("Reference", target.Name.Substring(0, target.Name.Length - target.Extension.Length), metas);
            proj.Save();
        }

        public static async Task TestAsync3()
        {
            InfoCsproj infoCsproj = new InfoCsproj(new FileInfo(@"C:\Users\ngoc\Desktop\New folder (2)\New folder (2).csproj"));
            await infoCsproj.ExecuteAsync();
        }
        //public static async Task TestAsync2()
        //{
        //    var root = new DirectoryInfo("C:\\Users\\ngoc\\Documents\\[Unity] Projects\\GameProject3\\Packages\\");
        //    var list = await IziProjectsFinding.FindNestedCsprojAsync(root);

        //    foreach (var item in list)
        //    {
        //        if (item.Nested.Count() > 0)
        //        {
        //            Console.WriteLine();
        //            Console.WriteLine("NestedBegin");
        //            Console.WriteLine($"Root:   {item!.FileInfo!.FullName}");
        //            foreach (var nested in item.Nested)
        //            {
        //                Console.WriteLine($"Nested  {nested!.FileInfo!.FullName}");
        //            }
        //            Console.WriteLine("NestedEnd");
        //        }
        //    }
        //}

        public static async Task TestAsync()
        {
            var dd = new DirectoryInfo("..\\");
            FileInfo dll = new FileInfo("C:\\Users\\ngoc\\Documents\\[Unity] Projects\\GameProject3\\Packages\\Unity3d Dlls\\UnityEngine.CoreModule.dll");
            Dictionary<string, string> metas = new Dictionary<string, string>()
            {
                ["HintPath"] = dll.FullName,
            };
            FileInfo csprojInfo = new FileInfo(@"C:\Users\ngoc\Desktop\New folder (2)\New folder (2).csproj");
            ProjectRootElement proj = global::Microsoft.Build.Construction.ProjectRootElement.Open(@"C:\Users\ngoc\Desktop\New folder (2)\New folder (2).csproj");
            StaticAddDll(proj, csprojInfo, dll);
            return;
            // не проверяет на уникальность. может дублировать
            proj.AddItem("Reference", "this_is_include", metas);
            proj.Save();

            Console.WriteLine($"Completed Test");
            Console.ReadLine();
        }
        public void AddNested(InfoCsproj infoCsproj)
        {
            nested.Add(infoCsproj);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="projTo"></param>
        /// <returns>
        /// <see langword="true"/> - были внесены изменения <br/>
        /// <see langword="false"/> - не было изменений <br/>
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool EnsureRefToCsproj(InfoCsproj projTo)
        {
            if (!projTo.FileInfo!.Exists) throw new FileNotFoundException(projTo.FileInfo!.FullName);

            foreach (var x in proj!.Items.Where(x => x.ElementName == "ProjectReference"))
            {
                FileInfo infoConstructed = default;
                if (UtilityForPath.IsRelative(x.Include))
                {
                    var path = UtilityForPath.Combine(FileInfo!.Directory!, x.Include, Path.DirectorySeparatorChar);
                    if (UtilityForPath.SamePath(projTo.FileInfo!, path)) return false;
                    infoConstructed = new FileInfo(path);
                }
                else
                {
                    if (UtilityForPath.SamePath(projTo.FileInfo!, x.Include)) return false;
                    infoConstructed = new FileInfo(x.Include);
                }

                // same name diff path is not allowed. Project Name Must be Uniq
                if (infoConstructed.Name == projTo.FileInfo!.Name)
                {
                    if (!infoConstructed.Exists)
                    {
                        x.Include = UtilityForPath.AbsToRelative(this.FileInfo!.Directory!, projTo.FileInfo.FullName);
                        return true;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Projects with same name exists.\r\n{infoConstructed.FullName}\r\n{projTo.FileInfo!.FullName}");
                    }
                }
            }
            proj!.AddItem("ProjectReference", UtilityForPath.AbsToRelative(FileInfo!.Directory!, projTo.FileInfo.FullName));
            return true;
        }
        public void EnsureRefToRelative(InfoCsproj refProj)
        {
            foreach (var item in proj!.Items)
            {
                if (item.Include == refProj.RelativePath() || item.Include == refProj.AbsPath())
                {
                    item.Include = refProj.RelativePath();
                    proj.Save();
                    return;
                }
            }
            proj!.AddItem("ProjectReference", refProj.RelativePath());
            proj.Save();
        }
        public void EnsureRefTo(InfoAsmdef refAsmdef)
        {
            throw new NotImplementedException();
        }
        public async Task EnsureRefToAsync(InfoAsmdef refAsmdef, InfoCsproj refProj)
        {
            throw new System.NotImplementedException();
        }

        public void FixReferenceByFilename(InfoCsproj byFileName, InfoItem error)
        {
            var pathAbsError = error.pathToItemAbsolute;
            var pathAbs = byFileName.FileInfo!.FullName;
            var pathRel = UtilityForPath.AbsToRelative(FileInfo!.Directory!, pathAbs);
            var record = proj!.Items.First(x => x.ElementName == "ProjectReference" && x.Include.EndsWith(Path.GetFileName(pathAbsError)));
            record.Include = pathRel;
            record.AddMetadata(ConstantsForIziProjects.ForCsproj.EL_DEPENDECY_GUID, byFileName.guid);
            proj.Save();
            error.guid = byFileName.guid;
            error.pathToItemAbsolute = pathAbs;
            error.pathToItemRelative = pathRel;
        }

        public void ForeachProjectReference(Action<ProjectItemElement> value)
        {
            foreach (var item in proj!.Items)
            {
                if (item.ElementName == "ProjectReference" && item.Include.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase))
                {
                    value(item);
                }
            }
        }
        public void ForeachProjectReferenceItem(Action<InfoItem> value)
        {
            foreach (var item in Items)
            {
                if (item.refType == ERefType.NetSdkProjectReference)
                {
                    value(item);
                }
            }
        }

        public static bool IsValidExtension(FileInfo fileInfo)
        {
            return IsValidExtension(fileInfo.Extension);
        }
        public static bool IsValidExtension(string extension)
        {
            return string.Equals(EXTENSION, extension, StringComparison.InvariantCultureIgnoreCase);
        }

        public static async Task CreateDefault(DirectoryInfo directory, string name)
        {
            var pathTemplate = Config.PathTemplateCsproj;
            var pathTarget = Path.Combine(directory.FullName, name + EXTENSION);
            File.Copy(pathTemplate, pathTarget, false);
            var fi = new FileInfo(pathTarget);
            await SetProjectName(fi, name).ConfigureAwait(false);
            await SetGuid(fi, System.Guid.NewGuid()).ConfigureAwait(false);
        }

        public static Task SetGuid(FileInfo info, Guid guid)
        {
            ProjectRootElement proj = ProjectRootElement.Open(info!.FullName);
            proj.AddProperty(PROP_GUID, guid.ToString("D"));
            proj.Save();
            return Task.CompletedTask;
        }
        public static Task SetProjectName(FileInfo info, string name)
        {
            ProjectRootElement proj = ProjectRootElement.Open(info!.FullName);
            proj.AddProperty(PROP_PROJ_NAME, name);
            proj.Save();
            return Task.CompletedTask;
        }
    }
}