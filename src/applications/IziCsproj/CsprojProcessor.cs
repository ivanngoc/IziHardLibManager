using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IziHardGames.DotNetProjects.Extensions;
using IziHardGames.FileSystem.NetStd21;
using IziLibrary.Database.DataBase.EfCore;
using Microsoft.Build.Construction;
using Microsoft.EntityFrameworkCore;

namespace IziHardGames.DotNetProjects
{
    public class CsprojProcessor(IziProjectsDbContext context) : ICsprojProcessor
    {
        public async Task EnsureRequiredMetasAsync(ICsproj csproj)
        {
            ProjectRootElement root = global::Microsoft.Build.Construction.ProjectRootElement.Open(csproj.FilePathAbsolute);
            var props = root.Properties;

            var (isCreated, guid) = root.EnsureGuid();
            //  <Authors>Tran Ngoc Anh</Authors>
            root.EnsureAuthor("Tran Ngoc Anh");
            // <Company>IziHardGames</Company>
            root.EnsureCompany("IziHardGames");

            //foreach (var item in root.GetProjectReferences())
            //{
            //    item.AddMetadata("MetaA", "MetaA-2");
            //    item.AddMetadata("Meta3", "MetaA-3", false);
            //}

            //var items = root.GetProjectReferences();
            //var metas = root.GetProjectReferences().Select(x=>x.EnsureMetas());
            //var paths = metas.Select(x=> UtilityForPath.GetActualAbsolutePath(x.Include, csproj.FileInfo.Directory?.FullName)).ToArray();

            //foreach (var item in metas)
            //{

            //}

            //foreach (var projectReference in items)
            //{
            //    var meta = projectReference.EnsureMetas();
            //    var pathAbs = UtilityForPath.GetActualAbsolutePath(meta.Include, csproj.FileInfo.Directory?.FullName);
            //}

            //foreach (var prop in props)
            //{
            //    if (prop.IsTag(ECsprojTag.Description))
            //    {
            //        this.Description = prop.Value;
            //    }
            //    else if (prop.IsTag(ECsprojTag.ProjectName))
            //    {
            //        ProjectName = prop.Value.Trim();
            //    }
            //}

            //if (string.IsNullOrEmpty(ProjectName))
            //{
            //    ProjectName = FileInfo!.FileNameWithoutExtension();
            //    root.AddProperty("ProjectName", ProjectName);
            //    root.Save();
            //}

            //if (!this.isGuidFounded)
            //{
            //    var guid = global::System.Guid.NewGuid();
            //    root.AddProperty("ProjectGuid", guid.ToString());
            //    SetGuidGenerated(guid);
            //}
            //IsExecuted = true;

            root.Save(System.Text.Encoding.UTF8);
            //throw new System.NotImplementedException();
        }

        public Task BeautifyAsync(ICsproj csproj)
        {


            throw new System.NotImplementedException();
        }

        public async Task<int> FillRelationsAsParentsAsync()
        {
            try
            {
                var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
                var csprojs = await context.Csprojs.Include(x => x.CsProjectAtDevices).Include(x => x.AsParent).ThenInclude(x => x.RelationsAtDevice).ToArrayAsync();
                var dictinary = new Dictionary<string, CsprojRelationAtDevice>();
                var checkoutDate = DateTime.Now;

                foreach (var csproj in csprojs)
                {
                    var atThisDevice = csproj.CsProjectAtDevices.FirstOrDefault(x => x.DeviceId == idDevice);
                    if (atThisDevice != null)
                    {
                        var path = atThisDevice.PathAbs;
                        var fi = new FileInfo(path);
                        if (fi.Exists)
                        {
                            var csprojMeta = new Csproj(fi);
                            var projectReferences = csprojMeta.GetProjectReferences().ToArray();
                            var refCount = projectReferences.Length;
                            csproj.CountReferencesToProjects = refCount;

                            var countMissingChilds = 0;
                            foreach (var projectReference in projectReferences)
                            {
                                var includeAsAbs = projectReference.GetIncludePathAsAbs(fi);
                                var refMeta = projectReference.GetMetas();
                                if (string.IsNullOrEmpty(includeAsAbs))
                                {
                                    throw new Exception($"include is null or empty: {includeAsAbs}");
                                }
                                if (dictinary.TryGetValue(includeAsAbs, out var existed))
                                {
                                    throw new Exception($"Duplicate: {projectReference}. {JsonSerializer.Serialize(existed)}");
                                }

                                CsprojRelation? existedRelation = null;

                                if (refMeta.CsprojId.HasValue)
                                {
                                    existedRelation = context.Relations.FirstOrDefault(x => x.ParentId == csproj.EntityCsprojId && x.ChildId == refMeta.CsprojId);
                                }
                                else
                                {
                                    existedRelation = context.Relations.Where(x => x.ParentId == csproj.EntityCsprojId)
                                                                       .AsEnumerable()
                                                                       .Where(x => x.ChildId.HasValue)
                                                                       .FirstOrDefault(x => refMeta.IsDefault() ? false : refMeta.CsprojId == x.ChildId!.Value || x.RelationsAtDevice.Any(y => y.Include == includeAsAbs));
                                }

                                var relationType = ERelationType.ParentMissingChild;

                                if (refMeta.CsprojId.HasValue)
                                {
                                    relationType = ERelationType.ParentChild;
                                }
                                else
                                {
                                    countMissingChilds++;
                                }

                                if (existedRelation == null)
                                {
                                    // если есть старая запись с потеряным чилдом, берем его и обновляем
                                    existedRelation = context.Relations.Include(x => x.RelationsAtDevice)
                                                                       .Where(x => x.ParentId == csproj.EntityCsprojId)
                                                                       .FirstOrDefault(x => x.RelationType != ERelationType.ParentChild && x.CheckDateTime < checkoutDate);

                                    if (existedRelation == null)
                                    {
                                        existedRelation = new CsprojRelation()
                                        {
                                            Id = default,
                                            ParentId = csproj.EntityCsprojId,
                                            Parent = csproj,
                                            ChildId = refMeta.CsprojId,
                                            Child = default!,
                                            RelationsAtDevice = new List<CsprojRelationAtDevice>()
                                        };
                                        context.Relations.Add(existedRelation);
                                    }
                                    else
                                    {
                                        // если можно переиспользовать, то вычищаем старые записи
                                        var toCleanup = existedRelation.RelationsAtDevice.Where(x => x.DeviceId == idDevice).ToArray();
                                        context.RelationsAtDevice.RemoveRange(toCleanup);
                                    }
                                }

                                existedRelation.CheckDateTime = checkoutDate;
                                existedRelation.RelationType = relationType;

                                CsprojRelationAtDevice? relationAtDevice = existedRelation.RelationsAtDevice.FirstOrDefault(x => (x.RelationId != default && (x.DeviceId == idDevice && x.RelationId == existedRelation.Id)) || x.Include == includeAsAbs);

                                if (relationAtDevice == null)
                                {
                                    relationAtDevice = new CsprojRelationAtDevice()
                                    {
                                        Id = default,
                                        DeviceId = idDevice,
                                        RelationId = existedRelation.Id,
                                        Relation = existedRelation,
                                        Device = default!,
                                    };
                                    existedRelation.RelationsAtDevice.Add(relationAtDevice);
                                    context.RelationsAtDevice.Add(relationAtDevice);
                                }

                                relationAtDevice.Include = includeAsAbs;
                                dictinary.Add(includeAsAbs, relationAtDevice);
                            }
                        }
                    }
                    dictinary.Clear();
                }
                return await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<int> FillRelationsAsChildsByIncludeFileExistingAsync()
        {
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            var relationsAtDevice = await context.RelationsAtDevice.Where(x => x.DeviceId == idDevice)
                                                                   .Where(x => x.Relation.RelationType != ERelationType.ParentChild)
                                                                   .Include(x => x.Relation)
                                                                   .ThenInclude(x => x.Parent)
                                                                   .ToArrayAsync();

            var checkDate = DateTimeOffset.Now;
            int count = 0;
            foreach (var relationAtDevice in relationsAtDevice)
            {
                var pathAbs = relationAtDevice.Include;
                if (UtilityForPath.IsRelative(pathAbs))
                {
                    throw new FormatException($"relationAtDevice: {relationAtDevice.Id}, Path is relative: {pathAbs}");
                }
                var fi = new FileInfo(pathAbs);
                if (fi.Exists)
                {
                    var csproj = new Csproj(fi);
                    if (csproj.TryGetGuid(out var id))
                    {
                        relationAtDevice.Relation.ChildId = id;
                        relationAtDevice.Relation.RelationType = ERelationType.ParentChild;
                        relationAtDevice.Relation.CheckDateTime = checkDate;
                    }
                }
            }
            count += await context.SaveChangesAsync();

            var projects = await context.ProjectsAtDevice.Where(x => x.DeviceId == idDevice).ToArrayAsync();
            foreach (var proj in projects)
            {
                var fi = new FileInfo(proj.PathAbs);
                var matches = await context.RelationsAtDevice.Where(x => x.DeviceId == idDevice)
                                                             .Include(x => x.Relation)
                                                             .Where(x => x.Relation.RelationType != ERelationType.ParentChild)
                                                             .Where(x => x.Include.Contains(fi.Name)).ToArrayAsync();
                foreach (var match in matches)
                {
                    match.Include = fi.FullName;
                    match.Relation.ChildId = proj.EntityCsprojId;
                    match.Relation.RelationType = ERelationType.ParentChild;
                    match.Relation.CheckDateTime = checkDate;
                }
            }
            count += await context.SaveChangesAsync();
            return count;
        }

        public async Task<int> FillRelationsAsChildsByCsprojFileNameAsync()
        {
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            var relationsAtDevice = await context.RelationsAtDevice.Include(x => x.Relation).Where(x => x.Relation.ChildId == null).Where(x => x.DeviceId == idDevice).ToArrayAsync();

            foreach (var relationAtDevice in relationsAtDevice)
            {
                var fi = new FileInfo(relationAtDevice.Include);
                var name = fi.FileNameWithoutExtension();
                var founded = await context.ProjectsAtDevice.Where(x => EF.Functions.ILike(x.PathAbs, $"%{name}%")).ToArrayAsync();
                foreach (var projectAtDevice in founded)
                {
                    var fiTarget = new FileInfo(projectAtDevice.PathAbs);
                    if (fiTarget.Name == fi.Name)
                    {
                        relationAtDevice.Relation.ChildId = projectAtDevice.EntityCsprojId;
                        break;
                    }
                }
            }
            return await context.SaveChangesAsync();
        }

        public async Task<int> ReplaceChildIncludeAsync(string find, string replace)
        {
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            var relationsAtDevice = await context.RelationsAtDevice.Include(x => x.Relation).Where(x => x.DeviceId == idDevice).Where(x => x.Include == find).ToArrayAsync();

            foreach (var relationAtDevice in relationsAtDevice)
            {
                var fi = new FileInfo(replace);
                var csProjTarget = new Csproj(fi);
                if (csProjTarget.TryGetGuid(out var id))
                {
                    relationAtDevice.Include = replace;
                    relationAtDevice.Relation.ChildId = id;
                }
            }
            return await context.SaveChangesAsync();
        }

        public async Task<int> FormatDependecies()
        {
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            //var relationsAtDevice = await context.RelationsAtDevice.Include(x => x.Relation)
            //                                                       .ThenInclude(x => x.Child)
            //                                                       .Include(x => x.Relation)
            //                                                       .ThenInclude(x => x.Parent)
            //                                                       .Where(x => x.DeviceId == idDevice)
            //                                                       .Where(x => x.Relation.ChildId != null)
            //                                                       .ToArrayAsync();
            //foreach (var relationAtDevice in relationsAtDevice)
            //{
            //    var parent = relationAtDevice.Relation.Parent;
            //    var child = relationAtDevice.Relation.Child;
            //    var childCsproj = await context.ProjectsAtDevice.Where(x => x.DeviceId == idDevice).Where(x => x.EntityCsprojId == child.EntityCsprojId).FirstOrDefaultAsync();
            //    var fi = new FileInfo(childCsproj.PathAbs);
            //    var csproj = new Csproj(fi);
            //    csproj.
            //    relationAtDevice.Relation.
            //}

            var projects = await context.ProjectsAtDevice.Where(x => x.DeviceId == idDevice).ToArrayAsync();
            int changes = 0;
            foreach (var proj in projects)
            {
                var childs = await context.RelationsAtDevice.Include(x => x.Relation)
                                                            .ThenInclude(x => x.Child)
                                                            .ThenInclude(x => x.CsProjectAtDevices)
                                                            .Where(x => x.Relation.ParentId == proj.EntityCsprojId)
                                                            .ToArrayAsync();
                var fi = new FileInfo(proj.PathAbs);
                if (fi.Exists)
                {
                    var csproj = new Csproj(fi);
                    if (childs.Any())
                    {
                        changes++;
                        await csproj.ReSetChilds(childs);
                    }
                }
            }
            return changes;
        }

        public async Task<int> DistinctRelationsAsync()
        {
            var relations = await context.Relations.ToArrayAsync();
            var dic = new Dictionary<(CsprojId, CsprojId), CsprojRelation>();
            var dicParents = new Dictionary<CsprojId, CsprojRelation>();
            var includes = new Dictionary<string, CsprojRelation>();
            foreach (var relation in relations)
            {
                if (relation.ParentId.HasValue && relation.ChildId.HasValue)
                {
                    var key = (relation.ParentId.Value, relation.ChildId.Value);
                    if (dic.TryGetValue(key, out var relationExisted))
                    {
                        context.Relations.Remove(relation);
                    }
                    else
                    {
                        dic.Add(key, relation);
                    }
                }
                else if (relation.ParentId.HasValue)
                {
                    if (dicParents.TryGetValue(relation.ParentId.Value, out var existedVal))
                    {
                        context.Relations.Remove(relation);
                    }
                    else
                    {
                        dicParents.Add(relation.ParentId.Value, relation);
                    }
                }
            }
            return await context.SaveChangesAsync();
        }

        public async Task<int> FormatIncludePathToEnvVarBasedPathAsync()
        {
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            var projs = await context.ProjectsAtDevice.AsNoTracking().Where(x => x.DeviceId == idDevice).ToArrayAsync();
            var count = 0;
            foreach (var projAtDevice in projs)
            {
                var fi = new FileInfo(projAtDevice.PathAbs);
                var proj = new Csproj(fi);
                await proj.FormatAllProjectReferencesPathToRelativeAsync();
                count++;
            }
            return count;
        }

        public async Task<int> ReplaceAbsIncludesWithRelativeAsync()
        {
            var es = await context.Csprojs.Include(x => x.CsProjectAtDevices).ToArrayAsync();
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            int replace = default;
            foreach (var e in es)
            {
                var atDevice = e.CsProjectAtDevices.Where(x => x.DeviceId == idDevice).FirstOrDefault();
                if (atDevice != null)
                {
#if DEBUG
                    if (atDevice.EntityCsprojId.Guid == Guid.Parse("91eae8b6-897f-4229-bb3a-8893e171ee15"))
                    {
                        Console.WriteLine("Hit");
                    }
#endif
                    var path = atDevice.PathAbs;
                    var fi = new FileInfo(path);
                    if (fi.Exists)
                    {
                        var csproj = new Csproj(fi);
                        if (await csproj.ReplaceIncludesOfProjectReferencesWherePathIsAbsAsync(context, idDevice))
                        {
                            replace++;
                        }
                    }
                }
            }
            return replace;
        }

        public async Task<int> ReplaceAbsIncludesWithEnvBasedAsync()
        {
            var idDevice = IziEnvironmentsHelper.GetCurrentDeviceGuid();
            var projs = await context.ProjectsAtDevice.Include(x => x.EntityCsproj).Where(x => x.DeviceId == idDevice).ToArrayAsync();
            int count = default;
            foreach (var proj in projs)
            {
                var fi = new FileInfo(proj.PathAbs);
                if (fi.Exists)
                {
                    var csproj = new Csproj(fi);
                    var refs = csproj.GetProjectReferences();

                    foreach (var refEl in refs)
                    {
                        var include = refEl.GetIncludePathAsIs();
                        if (UtilityForPath.IsAbsolute(include))
                        {
                            if (IziEnvironmentsHelper.TryReplacePathWithEnvVariables(include, out var result))
                            {
                                refEl.SetIncludePath(result);
                                count++;
                            }
                        }
                    }
                }
            }
            return count;
        }
    }
}
