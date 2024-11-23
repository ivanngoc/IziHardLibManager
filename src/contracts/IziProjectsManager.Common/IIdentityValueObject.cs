using System;

namespace IziHardGames.DotNetProjects
{
    public static class IziProjectsConstants
    {
        public const string GUID_FORMAT = "D";
    }
    public interface IIdentityValueObject
    {
        Guid Guid { get; set; }
    }
}
