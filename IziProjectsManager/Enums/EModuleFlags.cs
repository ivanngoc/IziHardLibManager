using System;

namespace IziHardGames.Projects.DataBase
{
    [Flags]
    public enum EModuleFlags : long
    {
        Reserved = -1,
        None = 0,
        UnityEngine = 1 << 0,
        NetStd21 = 1 << 1,
        NetCore = 1 << 2,
        Root = 1 << 3,
        Error = 1 << 4,
    }
}
