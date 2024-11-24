using System;

namespace IziHardGames.Environments
{
    public static class IziEnvironments
    {
        public const string IZHG_ROOT = "IZHG_ROOT";
        public const string IZHG_REFS = "IZHG_REFS";    
        public const string IZHG_MODULES = "IZHG_MODULES";
        public const string IZHG_DLLS_UNITY = "IZHG_DLLS_UNITY";    
        public const string IZHG_CONFIG_PATH = "IZHG_CONFIG_PATH";    
        public const string PATH_DEVTOOLS_CHAN_IVAN = "PATH_DEVTOOLS_CHAN_IVAN";    
        public const string IZHG_BUILDS_ROOT = "IZHG_BUILDS_ROOT";    
        public const string MY_NUGET_REGISTRY = "MY_NUGET_REGISTRY";    
        /// <summary>
        /// рефы для проекта управления библиотеками
        /// </summary>
        public const string IZHG_LIB_CONTROL_DIR_FOR_REFS = "IZHG_LIB_CONTROL_DIR_FOR_REFS";
        /// <summary>
        /// <see cref="Guid"/> устройства. Например дли идентификации пути проекта в БД в зависимости от утсройства.
        /// <see cref="IziDevices"/>
        /// </summary>
        public const string IZHG_DEVICE_GUID = "IZHG_DEVICE_GUID";
    }
}
