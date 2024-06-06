using System;

namespace IziHardGames.Projects
{
    [Flags]
    public enum ERelationsFlags : long
    {
        All = -1,
        None = 0,
        Nested = 1 << 0,
        /// <summary>
        /// Например .asmdef имеет соответствующий .csproj и наоборот.
        /// </summary>
        Correspond = 1 << 1,
        Associated = 1 << 2,
        Meta = 1 << 3,
    }
}