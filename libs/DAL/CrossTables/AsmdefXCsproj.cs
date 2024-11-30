using IziHardGames.Asmdefs;
using IziHardGames.DotNetProjects;

namespace IziHardGames.CrossTables
{
    public class AsmdefXCsproj
    {
        public CsprojId CsprojId { get; set; }
        public AsmdefId AsmdefId { get; set; }

        public EntityCsproj Csproj { get; set; } = null!;
        public EntityAsmdef Asmdef { get; set; } = null!;

    }
}
