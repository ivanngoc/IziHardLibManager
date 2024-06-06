using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;
using IziHardGames.Projects;

namespace IziLibrary.Database
{
    public static class Extensions
    {

        public static void FindConnections(this InfoCsproj infoCsproj, List<InfoRelation> result, Func<InfoItem, InfoBase> searcher)
        {
            if (infoCsproj.IsNestingSearched) throw new InvalidOperationException($"You must perform nested search! see {nameof(IziProjectsFinding.FindCsprojWithNestedOptionAsync)}()");
            foreach (var nest in infoCsproj.Nested)
            {
                var connection = new InfoRelation()
                {
                    flags = ERelationsFlags.Nested,
                    from = infoCsproj,
                    to = nest,
                };
                result.Add(connection);
            }
            if (!infoCsproj.IsExecuted) throw new InvalidOperationException($"You must perform nested search! see {nameof(infoCsproj.ExecuteAsync)}()");

            foreach (var item in infoCsproj.Items)
            {
                var connection = new InfoRelation()
                {
                    flags = ERelationsFlags.None,
                    from = infoCsproj,
                    to = searcher.Invoke(item),
                };
                result.Add(connection);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// <see langword="true"/> - в файл нужно внести сохранения
        /// </returns>
        public static async Task<bool> SearchForChangesAsync(this InfoIziProjectsMeta infoIziProjectsMeta)
        {
            List<InfoBase> infos = new List<InfoBase>();
            var relations = new List<InfoRelation>();
            bool isChanges = false;
            await IziProjectsFinding.MainSearchAsync(infoIziProjectsMeta.DirectoryInfo!, infos, 256, relations).ConfigureAwait(false);

            foreach (var item in infos)
            {
                if (item is InfoAsmdef || item is InfoCsproj || item is InfoPackageJson)
                {
                    if (item.IsGuidGenerated) throw new InvalidOperationException($"You must initilize item: {item!.FileInfo!.FullName}");
                    var pathRelative = UtilityForPath.AbsToRelative(infoIziProjectsMeta.DirectoryInfo!, item.FileInfo.FullName);
                    var filename = item.FileInfo.Name;
                    if (item is InfoCsproj infoCsproj)
                    {
                        isChanges |= infoIziProjectsMeta.Ensure(infoIziProjectsMeta.csprojs, item.GuidStruct, filename, pathRelative);
                    }
                    else if (item is InfoAsmdef infoAsmdef)
                    {
                        isChanges |= infoIziProjectsMeta.Ensure(infoIziProjectsMeta.asmdefs, item.GuidStruct, filename, pathRelative);
                    }
                    else if (item is InfoPackageJson packageJson)
                    {
                        isChanges |= infoIziProjectsMeta.Ensure(infoIziProjectsMeta.packageJsons, item.GuidStruct, filename, pathRelative);
                    }
                }
            }
            return isChanges;
        }



        public static async Task ExecuteAsyncV2(this InfoIziProjectsMeta meta)
        {
            var search = meta.Discover(meta.DirectoryInfo ?? throw new NullReferenceException());
            var files = search.files;
            meta.files = files;
            meta.Content = await File.ReadAllTextAsync(meta.info!.FullName).ConfigureAwait(false);

            JsonObject? jObj = JsonNode.Parse(meta.Content)?.AsObject();

            if (!meta.EnsureGuid(jObj))
            {
                await IziProjectsFinding.CreateDefaultFileAsync(meta.DirectoryInfo).ConfigureAwait(false);
                meta.Content = await File.ReadAllTextAsync(meta.info!.FullName).ConfigureAwait(false);
                jObj = JsonNode.Parse(meta.Content)?.AsObject();
            }

            var nodesCsprojs = jObj![InfoIziProjectsMeta.PROP_CSPROJS]!.AsArray();
            var modesAsmdefs = jObj![InfoIziProjectsMeta.PROP_ASMDEFS]!.AsArray();
            var nodesUnitypacks = jObj![InfoIziProjectsMeta.PROP_UNITYPACKS]!.AsArray();

            foreach (var node in nodesCsprojs)
            {
                var meta1 = new IziMetaItem(node!.AsObject());
                meta. csprojs.Add(meta1.guid, meta1);
            }
            foreach (var node in modesAsmdefs)
            {
                var meta1 = new IziMetaItem(node!.AsObject());
                meta.asmdefs.Add(meta1.guid, meta1);
            }
            foreach (var node in nodesUnitypacks)
            {
                var meta1 = new IziMetaItem(node!.AsObject());
                meta.packageJsons.Add(meta1.guid, meta1);
            }
        }
    }
}
