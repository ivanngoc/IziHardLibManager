using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.IziLibrary.ForCsproj
{
    public struct FileInfoLogicForCsproj(FileInfo fileInfo)
    {
        public async ValueTask<Guid?> FindGuidAsync()
        {
            var fi = fileInfo;
            return null;
        }
    }
}
