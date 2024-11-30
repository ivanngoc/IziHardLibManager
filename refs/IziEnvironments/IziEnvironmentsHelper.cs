using System;
using IziHardGames.Environments;
using static IziHardGames.Environments.IziDevices;
using static IziHardGames.Environments.IziEnvironments;
using IziHardGames.FileSystem.NetStd21;
using System.Diagnostics.CodeAnalysis;

namespace IziHardGames.DotNetProjects
{
    public static class IziEnvironmentsHelper
    {
        public static bool IsMyLaptop()
        {
            if (Guid.TryParse(Environment.GetEnvironmentVariable(IziEnvironments.IZHG_DEVICE_GUID), out var guid))
            {
                return guid == LAPTOP_ROG_G17_2023;
            }
            return false;
        }
        public static bool IsMyPcVnn()
        {
            if (Guid.TryParse(Environment.GetEnvironmentVariable(IziEnvironments.IZHG_DEVICE_GUID), out var guid))
            {
                return guid == HOME_PC_VNN;
            }
            return false;
        }

        public static Guid GetCurrentDeviceGuid()
        {
            var guid = Guid.Parse(Environment.GetEnvironmentVariable(IziEnvironments.IZHG_DEVICE_GUID));
            return guid;
        }

        public static bool TryReplacePathWithEnvVariables(string include, out string result)
        {
            result = string.Empty;
            if (UtilityForPath.IsRelative(include))
            {
                return false;
            }
            var v1 = GetEnvVariable(IziEnvironments.IZHG_LIB_CONTROL_DIR_FOR_REFS);
            var v2 = GetEnvVariable(IziEnvironments.IZHG_MODULES);
            var v3 = GetEnvVariable(IziEnvironments.IZHG_REFS);
            var v4 = GetEnvVariable(IziEnvironments.IZHG_ROOT);
            var v5 = GetEnvVariable(IziEnvironments.IZHG_CSHARP_PROJECTS);

            if (include.StartsWith(v1))
            {
                result = include.Replace(v1, $"$({IziEnvironments.IZHG_LIB_CONTROL_DIR_FOR_REFS})");
                return true;
            }
            else if (include.StartsWith(v2))
            {
                result = include.Replace(v2, $"$({IziEnvironments.IZHG_MODULES})");
                return true;
            }
            else if (include.StartsWith(v3))
            {
                result = include.Replace(v3, $"$({IziEnvironments.IZHG_REFS})");
                return true;
            }
            else if (include.StartsWith(v4))
            {
                result = include.Replace(v4, $"$({IziEnvironments.IZHG_ROOT})");
                return true;
            } 
            //else if (include.StartsWith(v5))
            //{
            //    result = include.Replace(v5, $"$({IziEnvironments.IZHG_CSHARP_PROJECTS})");
            //    return true;
            //}
            return false;
        }

        /// <summary>
        /// C:\buildstemp\$(ProjectName)\$(Configuration)\bin, где $(Configuration) - переменная среды <br/>
        /// $(IZHG_LIB_CONTROL_DIR_FOR_REFS)\izhg.FileSystem.NetCore\izhg.FileSystem.NetCore.csproj
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetActualAbsolutePath(string path, [NotNull] string? basePath)
        {
            if (string.IsNullOrEmpty(basePath)) throw new ArgumentNullException(nameof(basePath));
            // replace %ENV_NAME% but not $(ENV_NAME)
            var result = Environment.ExpandEnvironmentVariables(path);
            result = path.Replace(@$"$({IZHG_LIB_CONTROL_DIR_FOR_REFS})", Environment.GetEnvironmentVariable(IZHG_LIB_CONTROL_DIR_FOR_REFS));
            result = path.Replace(@$"$({IZHG_MODULES})", Environment.GetEnvironmentVariable(IZHG_LIB_CONTROL_DIR_FOR_REFS));
            if (UtilityForPath.IsRelative(result))
            {
                return UtilityForPath.RelativeToAbsolute(basePath, result);
            }
            return path;
        }

        public static string GetEnvVariable(string constant)
        {
            var res = Environment.GetEnvironmentVariable(constant) ?? "+300";
            return res;
        }
    }
}
