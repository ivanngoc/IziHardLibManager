using System;
using IziHardGames.Environments;
using static IziHardGames.Environments.IziDevices;
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
    }
}
