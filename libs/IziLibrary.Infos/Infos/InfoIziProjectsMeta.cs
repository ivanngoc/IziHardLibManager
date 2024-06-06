using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects.DataBase;

namespace IziHardGames.Projects
{
    /// <summary>
    /// <see cref="IziModelMeta"/>
    /// </summary>
    public class InfoIziProjectsMeta : InfoBase
    {    
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

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jObj"></param>
        /// <returns>
        /// <see langword="false"/> - guid was not founded. Generated New.<br/>
        /// </returns>
        public bool EnsureGuid(JsonObject? jObj)
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

      

        public bool Ensure(Dictionary<Guid, IziMetaItem> dict, Guid guidStruct, string filename, string pathRelative)
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

      

        public void Add(InfoPackageJson item)
        {
            var meta = new IziMetaItem()
            {
                guid = item.GuidStruct,
                fileName = item.FileInfo!.Name,
                pathRelative = UtilityForPath.AbsToRelative(this.FileInfo!.Directory!, item.FileInfo.FullName),
            };
            packageJsons.Add(meta.guid, meta);
        }

        public void Add(InfoAsmdef item)
        {
            var meta = new IziMetaItem()
            {
                guid = item.GuidStruct,
                fileName = item.FileInfo!.Name,
                pathRelative = UtilityForPath.AbsToRelative(this.FileInfo!.Directory!, item.FileInfo.FullName),
            };
            asmdefs.Add(meta.guid, meta);
        }

        public void Add(InfoCsproj item)
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

        public DiscoverResult Discover(DirectoryInfo dir)
        {
            List<FileInfo> infos = new List<FileInfo>();
            var dirs = new List<DirectoryInfo>() { dir };
            dir.FindDirBeneathExceptLinks(dirs);
            var filteredDirs = dirs.Where(x => x.Name != ConstantsForIziProjects.DEPENDECIES_FOLDER);
            var files = filteredDirs.SelectMany(x => x.GetFiles());
            return new DiscoverResult() { files = files };
        }

        public async Task SaveAsync()
        {
            await File.WriteAllTextAsync(info!.FullName, ToString());
        }

        public struct DiscoverResult
        {
            public IEnumerable<FileInfo> files;
        }
    }
}