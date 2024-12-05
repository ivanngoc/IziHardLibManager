using System;
using System.Text.Json.Serialization;
using IziHardGames.Projects.Common;

namespace IziHardGames.DotNetProjects
{
    [JsonConverter(typeof(GuidConverter<CsprojId>))]
    public struct CsprojId : IIdentityValueObject, IEquatable<CsprojId>
    {
        public Guid Guid { get; set; }
        public CsprojId(Guid guid)
        {
            Guid = guid;
        }
        public static explicit operator CsprojId(Guid guid) => new CsprojId(guid);
        public static explicit operator Guid(CsprojId guid) => guid.Guid;

        public static bool operator ==(CsprojId left, CsprojId right) => left.Guid == right.Guid;
        public static bool operator !=(CsprojId left, CsprojId right) => left.Guid != right.Guid;

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

        public bool Equals(CsprojId other)
        {
            return this == other;
        }

        public static CsprojId Create(Guid x)
        {
            return (CsprojId)x;
        }
        public static CsprojId? Create(Guid? x)
        {
            return x.HasValue ? (CsprojId?)x.Value : null;
        }
    }
}
