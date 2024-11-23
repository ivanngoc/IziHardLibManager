using System;
using System.IO;

namespace IziHardGames.FileSystem.NetCore
{
    public static class UtilityForMklink
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirSource">Directory на которую будет ссылаться ссылка</param>
        /// <param name="target">Directory в которой будет ссылка</param>
        /// <exception cref="NotImplementedException"></exception>
        public static void JunctionTargetDirToDir(DirectoryInfo dirSource, DirectoryInfo target)
        {
            JunctionTargetDirToDir(dirSource, target, dirSource.Name);
        }
        public static void JunctionTargetDirToDir(DirectoryInfo dirSource, DirectoryInfo target, string junctionName)
        {
            Directory.CreateSymbolicLink(Path.Combine(target.FullName, junctionName), dirSource.FullName);
        }
    }
}
