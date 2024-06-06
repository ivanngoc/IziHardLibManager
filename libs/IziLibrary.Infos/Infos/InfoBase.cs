using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;

namespace IziHardGames.Projects
{
    public abstract class InfoBase
    {
        public string? idUnified;
        protected bool isPaired;
        protected bool isGuidFounded;
        protected bool isGuidGenerated;
        protected string guid = string.Empty;
        public FileInfo info { get; private set; }
        private InfoBase? pair;
        protected string? targetExtension;

        private bool isMoveToDirection;
        protected DirectoryInfo targetDirection;

        public bool IsSkipped { get; set; }
        public bool IsExecuted { get; set; }
        public bool IsPaired => isPaired;
        public bool IsGuidFounded => isGuidFounded;
        public bool IsGuidGenerated => isGuidGenerated;
        public bool IsThirdParty { get; set; }
        public string Guid => guid;
        public System.Guid GuidStruct => System.Guid.Parse(guid);
        public FileInfo FileInfo => info;
        public DirectoryInfo DirectoryInfo => info.Directory!;
        public string? Description { get; set; }
        public DateTime DateTimeCreate { get; set; }
        public DateTime DateTimeModify { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRoot { get; set; }

        public InfoBase(FileInfo info)
        {
            this.info = info;
            if (info.Exists)
            {
                this.DateTimeCreate = info.CreationTimeUtc;
                this.DateTimeModify = info.LastWriteTimeUtc;
            }
        }
        public abstract Task ExecuteAsync();
        public virtual Task FindDependeciesInFileSystem() { return Task.CompletedTask; }
        public async Task EnsureExecuted()
        {
            if (!IsExecuted) await ExecuteAsync().ConfigureAwait(false);
            IsExecuted = true;
        }
        protected void SetPaired(InfoBase infoBase)
        {
            isPaired = true;
            this.pair = infoBase;
        }
        public string ToStringInfo()
        {
            if (isPaired)
            {
                return $"Paired: {info!.FullName} with: {pair!.FileInfo!.FullName}";
            }
            return $"Not Paired: {info!.FullName}";
        }

        public void SetMoveDirection(DirectoryInfo target)
        {
            isMoveToDirection = true;
            this.targetDirection = target;
        }

        public bool TryMove()
        {
            if (isMoveToDirection)
            {
                var dirCurrent = FileInfo!.Directory!;
                string destDirBase = UtilityForPath.Combine(targetDirection, dirCurrent.Name, Path.DirectorySeparatorChar);
                string destDir = destDirBase;
                int counter = 0;
                while (Directory.Exists(destDir))
                {
                    destDir = destDirBase + $" ({counter})";
                    counter++;
                }
                dirCurrent.MoveTo(destDir);
                Console.WriteLine($"Moved:\r\n{FileInfo.DirectoryName}\r\n{destDir}");
                Console.WriteLine();
                //TODO: Move meta also
                return true;
            }
            return false;
        }

        public void SetGuidFounded(System.Guid guid)
        {
            SetGuidFounded(guid.ToString());
        }
        public void SetGuidFounded(string guid)
        {
            SetGuid(guid);
            isGuidFounded = true;
        }

        public virtual void SetGuidGenerated(Guid guid)
        {
            SetGuid(guid);
            isGuidGenerated = true;
        }

        public void SetGuid(string guid)
        {
            this.guid = guid;
        }
        public void SetGuid(Guid guid)
        {
            this.guid = guid.ToString();
        }
    }
}