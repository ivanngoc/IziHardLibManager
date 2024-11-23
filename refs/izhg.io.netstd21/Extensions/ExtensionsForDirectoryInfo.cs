using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IziHardGames.FileSystem.NetStd21
{
    public static class ExtensionsForDirectoryInfo
    {
        public static async ValueTask<int> CountNestedAsync(this DirectoryInfo directory, Func<DirectoryInfo, Task<bool>> predictate)
        {
            int result = await predictate(directory).ConfigureAwait(false) ? 1 : 0;
            var dirs = directory.GetDirectories();
            foreach (var item in dirs)
            {
                result += await item.CountNestedAsync(predictate).ConfigureAwait(false);
            }
            return result;
        }
        public static int CountNested(this DirectoryInfo directory, Func<DirectoryInfo, bool> predictate)
        {
            int result = predictate(directory) ? 1 : 0;
            return directory.GetDirectories().Select(x => x.CountNested(predictate)).Sum() + result;
        }
        public static async Task ForeachSubdirAsync(this DirectoryInfo info, Func<DirectoryInfo, Task> func)
        {
            var subdirs = info.GetDirectories();
            foreach (var subdir in subdirs)
            {
                await func(subdir).ConfigureAwait(false);
                await subdir.ForeachSubdirAsync(func).ConfigureAwait(false);
            }
        }
        public static void FindAllFilesBeneath(this DirectoryInfo info, List<FileInfo> result)
        {
            info.ForeachDirBeneath(x =>
            {
                var files = x.GetFiles();
                result.AddRange(files);
            });
        }

        public static void FindDirBeneathExceptLinks(this DirectoryInfo info, List<DirectoryInfo> result)
        {
            info.ForeachDirBeneath(x => result.Add(x), x => !x.IsJunction());
        }
        public static void FindDirBeneath(this DirectoryInfo info, List<DirectoryInfo> result)
        {
            info.ForeachDirBeneath(x => result.Add(x));
        }


        public static IEnumerable<FileInfo> SelectAllFilesBeneath(this DirectoryInfo info)
        {
            var e = info.SelectDirBeneath().SelectMany(x => x.GetFiles());
            return e;
        }

        public static IEnumerable<FileInfo> SelectAllFilesBeneathExceptLinksDir(this DirectoryInfo info)
        {
            return info.SelectDirsBeneathFiltered(x => !x.IsJunction()).SelectMany(x => x.GetFiles());
        }
        public static IEnumerable<DirectoryInfo> SelectDirsBeneathFiltered(this DirectoryInfo info, Func<DirectoryInfo, bool> filter)
        {
            var dirs = info.GetDirectories().Where(filter);
            var e = dirs;
            var selects = dirs.SelectMany(x => x.SelectDirsBeneathFiltered(filter));
            e = e.Concat(selects);
            return e;
        }
        public static IEnumerable<DirectoryInfo> SelectDirBeneath(this DirectoryInfo info)
        {
            var dirs = info.GetDirectories();
            var e = dirs.AsEnumerable();
            var selects = dirs.SelectMany(x => x.SelectDirBeneath());
            e = e.Concat(selects);
            return e;
        }
        public static void ForeachDirBeneath(this DirectoryInfo info, Action<DirectoryInfo> action, Func<DirectoryInfo, bool> filter)
        {
            var subdirs = info.GetDirectories().Where(x => filter(x));
            foreach (var subdir in subdirs)
            {
                action(subdir);
                subdir.ForeachDirBeneath(action, filter);
            }
        }
        public static void ForeachDirBeneath(this DirectoryInfo info, Action<DirectoryInfo> action)
        {
            var subdirs = info.GetDirectories();
            foreach (var subdir in subdirs)
            {
                action(subdir);
                subdir.ForeachDirBeneath(action);
            }
        }
        public static string GetRelativePath(this DirectoryInfo info, DirectoryInfo subdir)
        {
            if (info.IsNested(subdir))
            {

            }
            throw new System.NotImplementedException();
        }
        public static bool IsNested(this DirectoryInfo info, DirectoryInfo subdir)
        {
            throw new System.NotImplementedException();
        }
        public static bool IsJunction(this DirectoryInfo dir)
        {
            return dir.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }
#if WINDOWS
#endif
    }
}
