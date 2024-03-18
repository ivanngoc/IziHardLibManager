using System;
using System.IO;
using System.Threading.Tasks;
using IziHardGames.Projects.DataBase;

namespace IziHardGames.Projects
{
    /// <summary>
    /// third party dependecy
    /// </summary>
    public class InfoDependecy : InfoBase
    {
        public EModuleType ModuleType { get; set; }
        public InfoDependecy(FileInfo info) : base(info)
        {

        }

        /// <summary>
        /// Each Dependecy filename must be Uniq
        /// </summary>
        /// <param name="refed"></param>
        /// <returns></returns>
        internal bool SameSource(InfoDependecy refed)
        {
            return this.FileInfo!.Name == refed.FileInfo!.Name;
        }

        internal static InfoDependecy MergeMainWithSub(InfoDependecy main, InfoDependecy sub)
        {
            if (sub.IsGuidFounded && !main.isGuidFounded)
            {
                main.SetGuid(sub.guid);
            }
            return main;
        }

        public override Task ExecuteAsync()
        {
            return Task.CompletedTask;
        }
    }
}