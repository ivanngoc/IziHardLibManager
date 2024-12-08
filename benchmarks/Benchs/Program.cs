using BenchmarkDotNet.Running;

namespace Benchs
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

#if DEBUG
            var d = new BechCsproj();
            d.Setup();
            d.Single();
#else
            BenchmarkRunner.Run<BechCsproj>();
#endif
        }
    }
}
