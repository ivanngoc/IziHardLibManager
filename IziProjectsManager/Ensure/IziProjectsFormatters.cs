using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.FileSystem.NetStd21;

namespace IziHardGames.Projects
{
    public static class IziProjectsFormatters
    {
        public static async Task FormatProjectDir(DirectoryInfo dir)
        {
            FileInfo iziMetaInfo = new FileInfo(Path.Combine(dir.FullName, InfoIziProjectsMeta.META_NAME));
            InfoIziProjectsMeta? meta = default;

            // создать izi-meta файл
            if (iziMetaInfo.Exists)
            {
                meta = new InfoIziProjectsMeta(iziMetaInfo);
            }
            else
            {
                meta = await InfoIziProjectsMeta.CreateDefaultFileAsync(dir).ConfigureAwait(false);
            }

            await meta!.ExecuteAsync().ConfigureAwait(false);

            await FormatInner(meta.files).ConfigureAwait(false);

            if (await meta.SearchForChangesAsync().ConfigureAwait(false))
            {
                await meta.SaveAsync().ConfigureAwait(false);
            }

            await FormatOuter(meta.files).ConfigureAwait(false);
        }
        /// <summary>
        /// Зависимые операции
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static async Task FormatOuter(IEnumerable<FileInfo> files)
        {
            // OUTER_LOOP
            //check dependecies
            await IziEnsureAsmdef.EnsureAsmdefDependeciesInCsproj().ConfigureAwait(false);

            foreach (var file in files)
            {
                if (InfoCsproj.IsValidExtension(file.Extension))
                {
                    await IziEnsureCsproj.FormatDepencdecies(file).ConfigureAwait(false);
                }
            }
        }
        /// <summary>
        /// Независимые операции
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static async Task FormatInner(IEnumerable<FileInfo> files)
        {
            // INNER_LOOP. Форматирование файлов проектов. без обращений к связям или БД
            // привести *.csproj к соответствию
            foreach (var file in files)
            {
                if (InfoCsproj.IsValidExtension(file.Extension))
                {
                    await IziEnsureCsproj.FormatProjectSingle(file).ConfigureAwait(false);
                }
            }

            foreach (var file in files)
            {
                if (InfoSln.IsValidExtension(file.Extension))
                {
                    await IziEnsureSln.FormatProjectSingle(file).ConfigureAwait(false);
                }
            }

            foreach (var file in files)
            {
                if (InfoAsmdef.IsValidExtension(file.Extension))
                {
                    await IziEnsureAsmdef.EnsureAsmdefFormatSingle(file).ConfigureAwait(false);
                }
            }
        }

        internal static async Task FormatAsmdefsNestedAsync(DirectoryInfo directory)
        {
            var asmdefs = IziProjectsFinding.SearchAsmdefsNested(directory);
            await FormatAsmdefsAsync(asmdefs).ConfigureAwait(false);
        }
        internal static async Task FormatAsmdefsAsync(IEnumerable<FileInfo> files)
        {
            var asmdefs = files.Where(x => string.Equals(x.Extension, InfoAsmdef.EXTENSION, StringComparison.InvariantCultureIgnoreCase));
            foreach (var item in asmdefs)
            {
                InfoAsmdef asmdef = new InfoAsmdef(item);
                await asmdef.ExecuteAsync().ConfigureAwait(false);
                string guidFile = Path.Combine(asmdef.DirectoryInfo.FullName, $"guid.asmdef.{asmdef.GuidStruct.ToString("N")}.tag");
                FileInfo fileInfo = new FileInfo(guidFile);
                await File.AppendAllLinesAsync(guidFile, new string[] { DateTime.Now.ToString() });
            }
            foreach (var item in asmdefs)
            {
                //await IziEnsureAsmdef.EnsureFormatHrf(item).ConfigureAwait(false);
            }
        }
        internal static async Task FormatAsmdefsAsync(DirectoryInfo directory)
        {
            var files = directory.GetFiles();
            await FormatAsmdefsAsync(files).ConfigureAwait(false);
        }

        /// <summary>
        /// <see cref="InfoPackageJson"/>
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        internal static async Task FormatJsonPackageAsync(FileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                var meta = new FileInfo(Path.Combine(fileInfo.Directory!.FullName, InfoPackageJson.FILE_NAME_META));
                if (meta.Exists)
                {
                    Guid guidMeta = await InfoUnityMeta.GetGuidAsync(meta).ConfigureAwait(false);
                    Guid guid = await InfoPackageJson.GetGuidAsync(fileInfo).ConfigureAwait(false);
                    if (guid != guidMeta)
                    {
                        await InfoPackageJson.OverrideGuidAsync(fileInfo, guidMeta).ConfigureAwait(false);
                    }
                }
                else
                {
                    throw new System.NotImplementedException();
                }
                InfoPackageJson info = new InfoPackageJson(fileInfo);
                await info.ExecuteAsync().ConfigureAwait(false);
                bool isModififed = false;

                var jobj = info.Value;
                var displayName = jobj[InfoPackageJson.PROP_DISPLAY_NAME];
                var version = jobj[InfoPackageJson.PROP_VERSION];
                if (displayName == null || string.IsNullOrEmpty((string)displayName!))
                {
                    jobj[InfoPackageJson.PROP_DISPLAY_NAME] = (string)jobj[InfoPackageJson.PROP_NAME]!;
                    isModififed = true;
                }
                if (version == null || string.IsNullOrEmpty((string)version!))
                {
                    jobj[InfoPackageJson.PROP_VERSION] = InfoPackageJson.DEFAULT_VERSION;
                    isModififed = true;
                }

                var author = jobj[InfoPackageJson.PROP_AUTHOR];
                if (author == null)
                {
                    JsonObject authorProp = new JsonObject();
                    authorProp[InfoPackageJson.PROP_AUTHOR_NAME] = InfoPackageJson.DEFAULT_AUTHOR_NAME;
                    authorProp[InfoPackageJson.PROP_AUTHOR_EMAIL] = InfoPackageJson.DEFAULT_AUTHOR_EMAIL;
                    authorProp[InfoPackageJson.PROP_AUTHOR_URL] = InfoPackageJson.DEFAULT_AUTHOR_URL;
                    jobj[InfoPackageJson.PROP_AUTHOR] = authorProp;
                    isModififed = true;
                }

                var rootNamespaceProp = jobj[InfoPackageJson.PROP_ROOT_NAMESPACE];
                if (rootNamespaceProp == null || string.IsNullOrEmpty((string)rootNamespaceProp!))
                {
                    jobj[InfoPackageJson.PROP_ROOT_NAMESPACE] = InfoPackageJson.DEFAULT_ROOT_NAMESAPCE;
                    isModififed = true;
                }
                if (isModififed)
                {
                    Console.WriteLine($"Format package.json: overrided {fileInfo.FullName}");
                    await File.WriteAllTextAsync(fileInfo.FullName, jobj.ToJsonString(Shared.jOptions)).ConfigureAwait(false);
                }
            }
            else
            {
                await IziEnsurePackageJson.EnsurePackageJsonAsync(fileInfo.Directory!).ConfigureAwait(false);
            }
        }
        internal static async Task FormatJsonPackagesAsync(DirectoryInfo dir)
        {
            var packs = dir.SelectAllFilesBeneath().Where(x => InfoPackageJson.IsValid(x));

            foreach (var item in packs)
            {
                await FormatJsonPackageAsync(item).ConfigureAwait(false);
            }
        }
    }
}