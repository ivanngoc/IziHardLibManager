using IziLibrary.Database.DataBase.EfCore;

namespace IziHardGames.Asmdefs
{
    public class EntityMeta
    {
        public MetaId MetaId { get; set; }
        public EntityAsmdef? Asmdef { get; set; }
        public ICollection<EntityMetaAtDevice> EntityMetaAtDevices { get; set; }
        public ICollection<EntityTag> Tags { get; set; }
    }

    public class EntityMetaAtDevice
    {
        public MetaId MetaId { get; set; }
        public Guid DeviceId { get; set; }
        public string PathAbs { get; set; }
        public EntityMeta Meta { get; set; }
        public Device Device { get; set; }
    }
}
