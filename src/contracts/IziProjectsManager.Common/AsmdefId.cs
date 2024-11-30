using System;
using IziHardGames.DotNetProjects;
using IziHardGames.Projects.Common;

namespace IziHardGames.Asmdefs
{
    public struct AsmdefId : IIdentityValueObject, IEquatable<AsmdefId>
    {
        public AsmdefId(Guid guid)
        {
            Guid = guid;
        }

        public Guid Guid { get; set; }

        public bool Equals(AsmdefId other)
        {
            return Guid != other.Guid;
        }

        public static AsmdefId Create(Guid x)
        {
            return (AsmdefId)x;
        }

        public static AsmdefId? Create(Guid? x)
        {
            return x.HasValue ? (AsmdefId?)x.Value : null;
        }

        public static explicit operator AsmdefId(Guid guid) => new AsmdefId(guid);
        public static explicit operator Guid(AsmdefId guid) => guid.Guid;

        public static bool operator ==(AsmdefId left, AsmdefId right) => left.Guid == right.Guid;
        public static bool operator !=(AsmdefId left, AsmdefId right) => left.Guid != right.Guid;
    }
}
