using System.Collections.Generic;

namespace IziHardGames.Projects
{
	internal class Registry
    {
        public readonly Dictionary<string, InfoAsmdef> asmdefs = new Dictionary<string, InfoAsmdef>();
        public readonly Dictionary<string, InfoCsproj> csprojs = new Dictionary<string, InfoCsproj>();
    }
}