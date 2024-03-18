using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace IziHardGames.Projects
{
    internal class CollectInfo
    {
        public CollectInfo()
        {

        }

        internal async Task ExecuteAsync()
        {
            var jsonStirng = await File.ReadAllTextAsync("config.json");
            JsonObject config = JsonNode.Parse(jsonStirng)!.AsObject();

            var path = (string)config["path_pack"]!;

            DirectoryInfo dir = new DirectoryInfo(path);

            List<InfoBase> infos = new List<InfoBase>();
            List<InfoAsmdef> unityCache = new List<InfoAsmdef>();

            var relations = new List<InfoRelation>();
            await IziProjectsFinding.MainSearchAsync(dir, infos, 0, relations).ConfigureAwait(false);

            await IziProjectsValidations.FixPackageName(infos);

            var binds = Analyz(infos);

            if (true)
            {
                await IziProjectsFinding.CollectAsmdefsFromCache(config, unityCache);

                var usings = infos.Select(x => x as InfoAsmdef).Where(x => x != null);

                var intersects = unityCache.Where(x => usings.Any(y => y!.Refs.Contains(x.Guid)));

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("=====Begin Usage=========");
                Console.WriteLine(intersects.Select(x => $"{x.Guid}\t{x!.FileInfo!.FullName}").Aggregate((x, y) => x + Environment.NewLine + y));
                Console.WriteLine("=====End   Usage=========");
            }

            await IziProjectsFinding.FindDependeciesForCsproj(dir, config, infos, unityCache);
            await IziProjectsOperations.InsertCsprojToPackSln(dir, infos);

            if (false) await IziProjectsOperations.OrginizeForNowAsync(dir, binds, infos);

            var pathView = (string)config["path_view"]!;
            await BuildView(pathView, binds);
        }



        private List<ProjectInfo> Analyz(List<InfoBase> input)
        {
            Dictionary<string, InfoAsmdef> asmdefs = new Dictionary<string, InfoAsmdef>();
            Dictionary<string, InfoCsproj> csporjs = new Dictionary<string, InfoCsproj>();
            List<ProjectInfo> result = new List<ProjectInfo>();

            foreach (var item in input)
            {
                if (item.IsGuidFounded)
                {
                    if (item is InfoAsmdef asmdef)
                    {
                        if (asmdef.IsGuidFounded)
                        {
                            asmdefs.Add(asmdef.Guid, asmdef);
                        }
                    }
                    else if (item is InfoCsproj csproj)
                    {
                        if (csporjs.TryGetValue(csproj.Guid, out var existed))
                        {
                            throw new FormatException($"Duplicate guid: {Environment.NewLine}{csproj.FileInfo!.FullName}{Environment.NewLine}{existed.FileInfo!.FullName}");
                        }
                        csporjs.Add(csproj.Guid, csproj);
                    }
                }
            }

            foreach (var item in asmdefs.Values)
            {
                foreach (var proj in csporjs.Values)
                {
                    if (item.FileInfo!.DirectoryName == proj.FileInfo!.DirectoryName)
                    {
                        proj.SetPairAsmdef(item);
                        item.SetPairCsproj(proj);
                        ProjectInfo projectInfo = new ProjectInfo(proj, item);
                        result.Add(projectInfo);
                        break;
                    }
                }
            }

            foreach (var item in input)
            {
                if (!item.IsPaired)
                {
                    Console.WriteLine(item.ToStringInfo());
                }
            }
            foreach (var item in input)
            {
                if (item.IsPaired)
                {
                    Console.WriteLine(item.ToStringInfo());
                }
            }
            foreach (var item in input)
            {
                if (!item.IsGuidFounded)
                {
                    Console.WriteLine($"No Guid: {item.FileInfo!.FullName}");
                }
            }
            return result;
        }



        private Task BuildView(string pathView, List<ProjectInfo> binds)
        {
            throw new NotImplementedException();
        }
    }
}