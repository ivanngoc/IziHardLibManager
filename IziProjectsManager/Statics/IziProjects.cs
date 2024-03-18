using System;
using IziHardGames.Projects.DataBase;
using IziHardGames.Projects.DataBase.Models;

namespace IziHardGames.Projects
{
    public static class IziProjects
    {
        public static uint GetType<T>()
        {
            return GetType(typeof(T));
        }

        internal static uint GetType(Type type)
        {
            if (type == typeof(InfoCsproj)) return (uint)EModuleType.Csproj;
            if (type == typeof(IziModelCsproj)) return (uint)EModuleType.Csproj;
            if (type == typeof(IziModelSln)) return (uint)EModuleType.Sln;
            if (type == typeof(InfoSln)) return (uint)EModuleType.Sln;
            if (type == typeof(IziModelUnityAsmdef)) return (uint)EModuleType.UnityAsmdef;
            if (type == typeof(InfoAsmdef)) return (uint)EModuleType.UnityAsmdef;
            if (type == typeof(IziModelUnityPackageJson)) return (uint)EModuleType.UnityPackageJson;
            if (type == typeof(InfoPackageJson)) return (uint)EModuleType.UnityPackageJson;
            if (type == typeof(IziModelUnityMeta)) return (uint)EModuleType.UnityMeta;
            if (type == typeof(InfoUnityMeta)) return (uint)EModuleType.UnityMeta;

            if (type == typeof(IziModelDependecy)) return (uint)EModuleType.ForeignDependecy;
            if (type == typeof(InfoDependecy)) return (uint)EModuleType.ForeignDependecy;

            if (type == typeof(InfoDll)) return (uint)EModuleType.Dll;
            if (type == typeof(IziModelInfoDll)) return (uint)EModuleType.Dll;

            if (type == typeof(IziModelDll)) return (uint)EModuleType.Dll;
            if (type == typeof(DllRecord)) return (uint)EModuleType.Dll;

            if (type == typeof(IziModelMeta)) return (uint)EModuleType.MetaForProjects;
            if (type == typeof(InfoIziProjectsMeta)) return (uint)EModuleType.MetaForProjects;


            throw new System.NotImplementedException(type.FullName);
        }
    }
}
