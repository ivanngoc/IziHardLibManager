using IziHardGames.Asmdefs;
using IziHardGames.DotNetProjects;
using IziHardGames.Projects.Common;
using System.Text.Json.Serialization;

namespace IziLibrary.Database.DataBase.EfCore
{
    public class EntityTag
    {
        public TagId? TagIdAlt { get; set; }
        public string TagId { get; set; }
        public ICollection<EntityMeta> Metas { get; set; }
        public ICollection<EntityCsproj> Csprojs { get; set; }
        public ICollection<EntityAsmdef> Asmdefs { get; set; }
    }

    [JsonConverter(typeof(GuidConverter<TagId>))]
    public struct TagId : IIdentityValueObject, IEquatable<TagId>
    {
        public Guid Guid { get; set; }
        public TagId(Guid guid)
        {
            Guid = guid;
        }
        public static explicit operator TagId(Guid guid) => new TagId(guid);
        public static explicit operator Guid(TagId guid) => guid.Guid;

        public static bool operator ==(TagId left, TagId right) => left.Guid == right.Guid;
        public static bool operator !=(TagId left, TagId right) => left.Guid != right.Guid;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }

        public override string ToString()
        {
            return Guid.ToString();
        }
        public string ToString(string format)
        {
            return Guid.ToString(format);
        }

        public bool Equals(TagId other)
        {
            return this == other;
        }

        public static TagId Create(Guid value)
        {
            return new TagId(value);
        }
    }
}
