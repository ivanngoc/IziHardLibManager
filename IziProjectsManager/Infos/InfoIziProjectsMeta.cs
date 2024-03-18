using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects.DataBase;
using IziHardGames.Projects.DataBase.Models;

namespace IziHardGames.Projects
{
    /// <summary>
    /// <see cref="IziModelMeta"/>
    /// </summary>
    public class InfoIziProjectsMeta : InfoBase
    {
        public const string META_NAME = "iziprojects.json";
        public const string PROP_GUID = "guid";
        public const string PROP_CSPROJS = "csprojs";
        public const string PROP_ASMDEFS = "asmdefs";
        public const string PROP_UNITYPACKS = "unitypacks";

        public Dictionary<Guid, IziMetaItem> csprojs = new Dictionary<Guid, IziMetaItem>();
        public Dictionary<Guid, IziMetaItem> asmdefs = new Dictionary<Guid, IziMetaItem>();
        public Dictionary<Guid, IziMetaItem> packageJsons = new Dictionary<Guid, IziMetaItem>();
        public IEnumerable<FileInfo> files = Enumerable.Empty<FileInfo>();



        public InfoIziProjectsMeta(FileInfo info) : base(info)
        {

        }

        public override async Task ExecuteAsync()
        {
            var search = Discover(DirectoryInfo ?? throw new NullReferenceException());
            var files = search.files;
            this.files = files;
            this.Content = await File.ReadAllTextAsync(info!.FullName).ConfigureAwait(false);

            JsonObject? jObj = JsonNode.Parse(Content)?.AsObject();

            if (!EnsureGuid(jObj))
            {
                await CreateDefaultFileAsync(DirectoryInfo).ConfigureAwait(false);
                this.Content = await File.ReadAllTextAsync(info!.FullName).ConfigureAwait(false);
                jObj = JsonNode.Parse(Content)?.AsObject();
            }

            var nodesCsprojs = jObj![PROP_CSPROJS]!.AsArray();
            var modesAsmdefs = jObj![PROP_ASMDEFS]!.AsArray();
            var nodesUnitypacks = jObj![PROP_UNITYPACKS]!.AsArray();

            foreach (var node in nodesCsprojs)
            {
                var meta = new IziMetaItem(node!.AsObject());
                csprojs.Add(meta.guid, meta);
            }
            foreach (var node in modesAsmdefs)
            {
                var meta = new IziMetaItem(node!.AsObject());
                asmdefs.Add(meta.guid, meta);
            }
            foreach (var node in nodesUnitypacks)
            {
                var meta = new IziMetaItem(node!.AsObject());
                packageJsons.Add(meta.guid, meta);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns>
        /// <see langword="false"/> - guid was not founded. Generated New.<br/>
        /// </returns>
        private bool EnsureGuid(JsonObject? jObj)
        {
            if (jObj == null) return false;

            var guidNode = jObj[PROP_GUID];
            if (guidNode == null) return false;

            if (System.Guid.TryParse((string)guidNode! ?? string.Empty, out Guid guid))
            {
                this.SetGuidFounded(guid);
            }
            else
            {
                this.SetGuidGenerated(System.Guid.NewGuid());
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// <see langword="true"/> - в файл нужно внести сохранения
        /// </returns>
        public async Task<bool> SearchForChangesAsync()
        {
            List<InfoBase> infos = new List<InfoBase>();
            var relations = new List<InfoRelation>();
            bool isChanges = false;
            await IziProjectsFinding.MainSearchAsync(DirectoryInfo!, infos, 256, relations).ConfigureAwait(false);

            foreach (var item in infos)
            {
                if (item is InfoAsmdef || item is InfoCsproj || item is InfoPackageJson)
                {
                    if (item.IsGuidGenerated) throw new InvalidOperationException($"You must initilize item: {item!.FileInfo!.FullName}");
                    var pathRelative = UtilityForPath.AbsToRelative(DirectoryInfo!, item.FileInfo.FullName);
                    var filename = item.FileInfo.Name;
                    if (item is InfoCsproj infoCsproj)
                    {
                        isChanges |= Ensure(csprojs, item.GuidStruct, filename, pathRelative);
                    }
                    else if (item is InfoAsmdef infoAsmdef)
                    {
                        isChanges |= Ensure(asmdefs, item.GuidStruct, filename, pathRelative);
                    }
                    else if (item is InfoPackageJson packageJson)
                    {
                        isChanges |= Ensure(packageJsons, item.GuidStruct, filename, pathRelative);
                    }
                }
            }
            return isChanges;
        }

        private bool Ensure(Dictionary<Guid, IziMetaItem> dict, Guid guidStruct, string filename, string pathRelative)
        {
            bool isChanges = false;

            if (dict.TryGetValue(guidStruct, out var meta))
            {
                if (!meta.Ensure(guidStruct, filename, pathRelative))
                {
                    isChanges = true;
                }
            }
            else
            {
                meta = new IziMetaItem(guidStruct, filename, pathRelative);
                dict.Add(guidStruct, meta);
                isChanges = true;
            }
            return isChanges;
        }

        internal static async ValueTask<InfoIziProjectsMeta> CreateDefaultFileAsync(DirectoryInfo directory)
        {
            Guid guid = System.Guid.NewGuid();
            var infos = new List<InfoBase>();
            FileInfo fileInfo = new FileInfo(Path.Combine(directory.FullName, META_NAME));
            InfoIziProjectsMeta meta = new InfoIziProjectsMeta(fileInfo);
            meta.SetGuidGenerated(guid);

            var pairs = new List<ItemsInFolder>();

            await IziProjectsFinding.SearchOnly(directory, infos, 256, pairs).ConfigureAwait(false);

            foreach (var item in infos)
            {
                if (item is InfoCsproj infoCsproj)
                {
                    await item.ExecuteAsync().ConfigureAwait(false);
                    meta.Add(infoCsproj);
                }
                else if (item is InfoAsmdef infoAsmdef)
                {
                    await item.ExecuteAsync().ConfigureAwait(false);
                    meta.Add(infoAsmdef);
                }
                else if (item is InfoPackageJson packageJson)
                {
                    await item.ExecuteAsync().ConfigureAwait(false);
                    meta.Add(packageJson);
                }
            }
            await File.WriteAllTextAsync(fileInfo.FullName, meta.ToString()).ConfigureAwait(false);
            return meta;
        }

        private void Add(InfoPackageJson item)
        {
            var meta = new IziMetaItem()
            {
                guid = item.GuidStruct,
                fileName = item.FileInfo!.Name,
                pathRelative = UtilityForPath.AbsToRelative(this.FileInfo!.Directory!, item.FileInfo.FullName),
            };
            packageJsons.Add(meta.guid, meta);
        }

        private void Add(InfoAsmdef item)
        {
            var meta = new IziMetaItem()
            {
                guid = item.GuidStruct,
                fileName = item.FileInfo!.Name,
                pathRelative = UtilityForPath.AbsToRelative(this.FileInfo!.Directory!, item.FileInfo.FullName),
            };
            asmdefs.Add(meta.guid, meta);
        }

        private void Add(InfoCsproj item)
        {
            var meta = new IziMetaItem()
            {
                guid = item.GuidStruct,
                fileName = item.FileInfo!.Name,
                pathRelative = UtilityForPath.AbsToRelative(this.FileInfo!.Directory!, item.FileInfo.FullName),
            };
            csprojs.Add(meta.guid, meta);
        }

        public override string ToString()
        {
            JsonArray csprojs = new JsonArray();
            foreach (var item in this.csprojs)
            {
                csprojs.Add(item.Value.AsJsonObject());
            }
            JsonArray asmdefs = new JsonArray();
            foreach (var item in this.asmdefs)
            {
                asmdefs.Add(item.Value.AsJsonObject());
            }
            JsonArray jsonPackages = new JsonArray();
            foreach (var item in this.packageJsons)
            {
                jsonPackages.Add(item.Value.AsJsonObject());
            }

            JsonObject jObj = new JsonObject();
            jObj[PROP_GUID] = GuidStruct.ToString("D");
            jObj[PROP_CSPROJS] = csprojs;
            jObj[PROP_ASMDEFS] = asmdefs;
            jObj[PROP_UNITYPACKS] = jsonPackages;
            return jObj.ToJsonString(Shared.jOptions);
        }

        internal DiscoverResult Discover(DirectoryInfo dir)
        {
            List<FileInfo> infos = new List<FileInfo>();
            var dirs = new List<DirectoryInfo>() { dir };
            dir.FindDirBeneathExceptLinks(dirs);
            var filteredDirs = dirs.Where(x => x.Name != ConstantsForIziProjects.DEPENDECIES_FOLDER);
            var files = filteredDirs.SelectMany(x => x.GetFiles());
            return new DiscoverResult() { files = files };
        }

        internal async Task SaveAsync()
        {
            await File.WriteAllTextAsync(info!.FullName, ToString());
        }

        internal struct DiscoverResult
        {
            internal IEnumerable<FileInfo> files;
        }
    }
}