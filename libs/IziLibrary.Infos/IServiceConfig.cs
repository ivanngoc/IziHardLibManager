using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IziHardGames.IziLibrary.Contracts;
using IziHardGames.IziLibrary.Metas.Factories;
using IziHardGames.IziLibrary.Metas.Factories.Contracts;
using IziHardGames.IziLibrary.Metas.ForAsmdef;
using IziHardGames.IziLibrary.Metas.Linked.Transformers;
using IziLibrary.Commands.FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace IziHardGames.Projects
{
    public class IServiceConfig
    {
        public static void Configure(IServiceCollection services)
        {
            AddClassesAsInterfacesSingletons(services, typeof(IAnalyzer<,>));

            services.AddSingleton<FileSystemScanConfig>(x => new FileSystemScanConfig()
            {
                dirs = new[]
                    {
                        Environment.GetEnvironmentVariable(ConstantsForIziLibrary.ENV_VAR_IZHG_ROOT) ?? throw new NotImplementedException(),
                        @"C:\Users\ngoc\Documents\[Projects] C#"
                    },
            });
            services.AddSingleton<FileSearcher>();
            services.AddSingleton<@Transformer, TransformMetaForAsmdefToModelAsmdef>();
            services.AddSingleton<IAnalyzer<MetaForAsmdef, MetaAnalyzForAsmdef>, AnalyzerForAsmdef>();
            services.AddSingleton<MetaProviderFromFileSystem>();
            services.AddSingleton<IMetaProvider<MetaAbstract>, MetaProviderFromFileSystem>();

            //services.AddSingleton<IFileDetector>()

            //services.AddSingleton<IEnumerable<IFileDetector<MetaAbstract>>>
            //    (x => x.GetServices(typeof(IFileDetector<>))
            //    .Select(x => (IFileDetector<MetaAbstract>)x)
            //    .Where(x => x != null));

            //services.AddSingleton(typeof(IFileDetector<MetaForAsmdef>), typeof(DetectorForAsmdef));
            services.AddSingleton(typeof(IFileDetector<MetaAbstract>), typeof(DetectorForAsmdef));

            services.AddTransient(typeof(ISource<FileInfo>), typeof(SourceForFiles));
        }

        private static void AddClassesAsInterfacesSingletons(IServiceCollection collection, Type interfaceTypeGenericDefenition)
        {
            if (!interfaceTypeGenericDefenition.IsGenericTypeDefinition) throw new ArgumentException();

            foreach (var item in FindType((x) => x.IsClass && HasGenericDefenitionInterface(x, interfaceTypeGenericDefenition)))
            {
                var interfaceType = item.GetInterface(typeof(IAnalyzer<,>).Name);
                var genType = interfaceTypeGenericDefenition.MakeGenericType(interfaceType.GetGenericArguments());
                collection.AddSingleton(interfaceType, item);
            }
        }

        private static IEnumerable<Type> FindType(Func<Type, bool> predictate)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (predictate(type))
                    {
                        yield return type;
                    }
                }
            }
        }

        private static bool HasGenericDefenitionInterface(Type target, Type interfaceType)
        {
            if (!interfaceType.IsGenericTypeDefinition) throw new ArgumentException();
            return target.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType);
        }
    }
}
