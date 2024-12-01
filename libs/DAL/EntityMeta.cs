namespace IziHardGames.Asmdefs
{
    public class EntityMeta
    {
        public MetaId MetaId { get; set; }
        public EntityAsmdef? Asmdef { get; set; }
    }

    public class EntityMetaAtDevice
    {
        public MetaId MetaId { get; set; }
        public Guid DeviceId { get; set; }
        public EntityMeta Meta { get; set; }
    }
}
