namespace IziHardGames.Projects.DataBase
{
    public enum EModuleType : uint
    {

        None = 0,
        /// <summary>
        /// <see cref="IziModelCsproj"/>
        /// <see cref="InfoCsproj"/>
        /// </summary>
        Csproj = 1,
        /// <summary>
        /// <see cref="IziModelSln"/>
        /// <see cref="InfoSln"/>
        /// </summary>
        Sln = 2,
        /// <summary>
        /// <see cref="IziModelUnityAsmdef"/>
        /// <see cref="OldInfoAsmdef"/>
        /// </summary>
        UnityAsmdef = 3,

        UnityAsmdefThirdParty = 4,
        UnityPackageJson = 5,
        UnityMeta = 6,
        ForeignDependecy = 7,
        /// <summary>
        /// <see cref="IziModelDll"/>
        /// <see cref="InfoDll"/>
        /// </summary>
        Dll = 8,
        DllInfo = 9,

        Unknown = 10,
        MetaForProjects = 11,
    }
}
