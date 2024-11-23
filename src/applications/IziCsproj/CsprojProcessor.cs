using System;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using IziHardGames.DotNetProjects.Extensions;
using IziHardGames.FileSystem.NetStd21;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace IziHardGames.DotNetProjects
{
    public class CsprojProcessor : ICsprojProcessor
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
    }
}
