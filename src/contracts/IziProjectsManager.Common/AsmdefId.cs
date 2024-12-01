using System;
using IziHardGames.DotNetProjects;
using IziHardGames.Projects.Common;

namespace IziHardGames.Asmdefs
{
    public struct AsmdefId : IIdentityValueObject, IEquatable<AsmdefId>
    {
        public Guid Guid { get; set; }
        public AsmdefId(Guid guid)
        {
            Guid = guid;
        }
        public static explicit operator AsmdefId(Guid guid) => new AsmdefId(guid);
        public static explicit operator Guid(AsmdefId guid) => guid.Guid;

        public static bool operator ==(AsmdefId left, AsmdefId right) => left.Guid == right.Guid;
        public static bool operator !=(AsmdefId left, AsmdefId right) => left.Guid != right.Guid;

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

        public bool Equals(AsmdefId other)
        {
            return this == other;
        }

        public static AsmdefId Create(Guid x)
        {
            return (AsmdefId)x;
        }
        public static AsmdefId? Create(Guid? x)
        {
            return x.HasValue ? (AsmdefId?)x.Value : null;
        }
    }
}
