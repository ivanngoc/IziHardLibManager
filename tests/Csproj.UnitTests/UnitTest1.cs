using System;
using IziHardGames.DotNetProjects;
using IziHardGames.FileSystem.NetCore;
using Xunit.Abstractions;

namespace IziHardGames.DotNetProjects.UnitTests
{
    public class UnitTest1(ITestOutputHelper console)
    {
        [Fact]
        public async Task Test1()
        {
            var proc = new CsprojProcessor();
            var fi = new FileInfo(@"C:\Users\ngoc\Documents\[Projects] C#\IziProjectsManager\tests\Csproj.UnitTests\Csproj.UnitTests.csproj");
            var dirInfo = new DirectoryInfo(@"C:\Users\ngoc\Documents\[Projects] C#\IziProjectsManager");
            var files = UtilityForIteratingFileSystem.GetAllFilesWithExtension(dirInfo, ".csproj");

            foreach (var item in files)
            {
                console.WriteLine(item.FullName);
                var temp = new Csproj(item);
                await proc.EnsureRequiredMetasAsync(temp);
            }
            //var fi = new FileInfo(@"C:\Users\ngoc\Documents\[Projects] C#\IziHar\src\IziHAR.Application\IziHAR.Application.csproj");
            var csproj = new Csproj(fi);
            await proc.EnsureRequiredMetasAsync(csproj);
        }
    }
}