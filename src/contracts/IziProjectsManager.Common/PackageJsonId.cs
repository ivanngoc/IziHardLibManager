using System;
using System.Text.Json.Serialization;
using IziHardGames.Projects.Common;

namespace IziHardGames.DotNetProjects
{
    [JsonConverter(typeof(GuidConverter<PackageJsonId>))]
    public struct PackageJsonId : IIdentityValueObject, IEquatable<PackageJsonId>
    {
        public Guid Guid { get; set; }
        public PackageJsonId(Guid guid)
        {
            Guid = guid;
        }

        public static explicit operator PackageJsonId(Guid guid) => new PackageJsonId(guid);
        public static explicit operator Guid(PackageJsonId guid) => guid.Guid;

        public static bool operator ==(PackageJsonId left, PackageJsonId right) => left.Guid == right.Guid;
        public static bool operator !=(PackageJsonId left, PackageJsonId right) => left.Guid != right.Guid;

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

        public bool Equals(PackageJsonId other)
        {
            return this == other;
        }

        public static PackageJsonId Create(Guid x)
        {
            return (PackageJsonId)x;
        }
        public static PackageJsonId? Create(Guid? x)
        {
            return x.HasValue ? (PackageJsonId?)x.Value : null;
        }
    }
}
