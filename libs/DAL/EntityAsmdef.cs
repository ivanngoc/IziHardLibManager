using IziHardGames.CrossTables;
using IziHardGames.DotNetProjects;

namespace IziHardGames.Asmdefs
{
    public class EntityAsmdef
    {
        public AsmdefId EntityAsmdefId { get; set; }
        public string? Description { get; set; }

        public ICollection<RelationAsmdef> AsParent { get; set; } = null!;
        public ICollection<RelationAsmdef> AsChild { get; set; } = null!;
        public ICollection<EntityAsmdefAtDevice> AsmdefsAtDevice { get; set; } = null!;
        public ICollection<AsmdefXCsproj> AsmdefXCsprojs { get; set; } = null!;
    }

    public class EntityAsmdefAtDevice
    {
        public Guid DeviceId { get; set; }
        public AsmdefId AsmdefId { get; set; }
        public EntityAsmdef Asmdef { get; set; } = null!;
    }

    public class RelationAsmdef
    {
        public int Id { get; set; }
        public AsmdefId? FromId { get; set; }
        public AsmdefId? ToId { get; set; }
        public DateTimeOffset? CheckDateTime { get; set; }
        public ERelationType RelationType { get; set; }
        public EntityAsmdef? From { get; set; }
        public EntityAsmdef? To { get; set; }
        public ICollection<RelationAsmdefAtDevice> RelationsAtDevice { get; set; } = null!;
    }

    public class RelationAsmdefAtDevice
    {
        public Guid DeviceId { get; set; }
        public int RelationId { get; set; }
        public RelationAsmdef Relation { get; set; } = null!;
    }
}
