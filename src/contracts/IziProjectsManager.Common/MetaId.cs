using System;
using IziHardGames.Projects.Common;

namespace IziHardGames.Asmdefs
{
    public struct MetaId : IIdentityValueObject, IEquatable<MetaId>
    {
        public Guid Guid { get; set; }
        public MetaId(Guid guid)
        {
            Guid = guid;
        }
        public static explicit operator MetaId(Guid guid) => new MetaId(guid);
        public static explicit operator Guid(MetaId guid) => guid.Guid;

        public static bool operator ==(MetaId left, MetaId right) => left.Guid == right.Guid;
        public static bool operator !=(MetaId left, MetaId right) => left.Guid != right.Guid;

        public static bool operator ==(MetaId left, Guid right) => left.Guid == right;
        public static bool operator !=(MetaId left, Guid right) => left.Guid != right;

        public static bool operator ==(Guid left, MetaId right) => left == right.Guid;
        public static bool operator !=(Guid left, MetaId right) => left != right.Guid;


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

        public bool Equals(MetaId other)
        {
            return this == other;
        }

        public static MetaId Create(Guid x)
        {
            return (MetaId)x;
        }
        public static MetaId? Create(Guid? x)
        {
            return x.HasValue ? (MetaId?)x.Value : null;
        }
    }
}
