using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IziHardGames.Projects.Sln;

namespace IziHardGames.Projects
{
    public class InfoSln : InfoBase
    {
        public const string EXTENSION = ".sln";
        private SlnMappedFile? slnMap;
        private readonly List<InfoItem> infoItems = new List<InfoItem>();
        public IEnumerable<InfoItem> Items => infoItems;

        public InfoSln(FileInfo info) : base(info)
        {
            targetExtension = EXTENSION;
        }
        public void EnsureDependecy(InfoCsproj csproj)
        {
            throw new System.NotImplementedException();
        }
        public void AddOrUpdateDirty(InfoCsproj csproj)
        {
            throw new NotImplementedException();
        }

        public override async Task ExecuteAsync()
        {
            this.slnMap = new SlnMappedFile(FileInfo!);
            slnMap.FindDependecies(infoItems);
            await slnMap.ExecuteAsync();

            if (slnMap.SolutionGuid != default)
            {
                SetGuidFounded(slnMap.SolutionGuid);
            }
            else
            {
                SetGuidGenerated(System.Guid.NewGuid());
                await slnMap.SaveToFileAsync();
            }
            this.Content = await File.ReadAllTextAsync(FileInfo!.FullName);

            IsExecuted = true;
        }

        public override void SetGuidGenerated(Guid guid)
        {
            base.SetGuidGenerated(guid);
            slnMap!.OverrideGuid(guid);
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void FindConnections(List<InfoRelation> result, Func<InfoItem, InfoCsproj> finder)
        {
            if (!IsExecuted) throw new InvalidOperationException($"You mast call {nameof(ExecuteAsync)} before that moment");
            foreach (var item in Items)
            {
                var dep = finder.Invoke(item);
                var connection = new InfoRelation()
                {
                    from = this,
                    to = dep,
                    flags = ERelationsFlags.None,
                };
                result.Add(connection);
            }
        }

        public static bool IsValidExtension(string extension)
        {
            return string.Equals(extension, EXTENSION, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}