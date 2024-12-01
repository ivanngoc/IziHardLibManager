using IziHardGames.CrossTables;
using IziHardGames.DotNetProjects;
using IziLibrary.Database.DataBase.EfCore;

namespace IziHardGames.Asmdefs
{
    public class EntityAsmdef
    {
        public AsmdefId EntityAsmdefId { get; set; }
        public MetaId? MetaId { get; set; }
        public string? Description { get; set; }
        public EntityMeta? Meta { get; set; }

        public ICollection<RelationAsmdef> AsParent { get; set; }
        public ICollection<RelationAsmdef> AsChild { get; set; }
        public ICollection<EntityAsmdefAtDevice> AsmdefsAtDevice { get; set; }
        public ICollection<AsmdefXCsproj> AsmdefXCsprojs { get; set; }
    }

    public class EntityAsmdefAtDevice
    {
        public Guid DeviceId { get; set; }
        public AsmdefId AsmdefId { get; set; }
        public string PathAbs { get; set; } = null!;
        public EntityAsmdef Asmdef { get; }
        public Device Device { get; }
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
        public Device Device { get; set; }
    }
}
