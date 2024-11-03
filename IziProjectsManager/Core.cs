using System;
using System.Threading.Tasks;
using CommandLine;
using IziHardGames.IziLibrary.Metas.Linked;
using Microsoft.Extensions.DependencyInjection;

namespace IziHardGames.Projects
{

    internal class Core
    {
        public Task<IServiceProvider> Run()
        {
            return Task.Run<IServiceProvider>(async () =>
            {
                await Task.CompletedTask;
                var serviceCollection = new ServiceCollection();
                IServiceConfig. Configure(serviceCollection);
                return serviceCollection.BuildServiceProvider();
            });
        }

       

     
    }
}
