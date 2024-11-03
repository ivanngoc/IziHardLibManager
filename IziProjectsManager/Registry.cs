using System.Collections.Generic;

namespace IziHardGames.Projects
{
	internal class Registry
    {
        public readonly Dictionary<string, OldInfoAsmdef> asmdefs = new Dictionary<string, OldInfoAsmdef>();
        public readonly Dictionary<string, InfoCsproj> csprojs = new Dictionary<string, InfoCsproj>();
    }
}