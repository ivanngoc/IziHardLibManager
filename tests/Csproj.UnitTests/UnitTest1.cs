using System;
using IziHardGames.DotNetProjects;
using IziHardGames.FileSystem.NetCore;
using IziLibrary.Database.DataBase.EfCore;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace IziHardGames.DotNetProjects.UnitTests
{
    public class UnitTest1(ITestOutputHelper console)
    {
        [Fact]
        public async Task Test1()
        {
            var uid = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_USER_DEV");
            var pwd = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PASSWORD_DEV");
            var server = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_SERVER_DEV");
            var port = Environment.GetEnvironmentVariable("IZHG_DB_POSTGRES_PORT_DEV");
            var portVal = $";port={port}";

            var cs = $"server={server};uid={uid};pwd={pwd}{(port is null ? string.Empty : portVal)};database={nameof(IziProjectsDbContext)}; Include Error Detail=true";

            var builder = new DbContextOptionsBuilder<IziProjectsDbContext>();
            builder.UseNpgsql(cs);
            var context = new IziProjectsDbContext(builder.Options);
            var proc = new CsprojProcessor(context);

            var fi = new FileInfo(@"C:\Users\ngoc\Documents\[Projects] C#\IziProjectsManager\tests\Csproj.UnitTests\Csproj.UnitTests.csproj");
            var dirInfo = new DirectoryInfo(@"C:\Users\ngoc\Documents\[Projects] C#\IziProjectsManager");

            if (IziEnvironmentsHelper.IsMyPcVnn())
            {
                fi = new FileInfo(@"C:\Users\ivan\Documents\.csharp\IziHardLibManager\tests\Csproj.UnitTests\Csproj.UnitTests.csproj");
                dirInfo = new DirectoryInfo(@"C:\Users\ivan\Documents\.csharp\IziHardLibManager");
            }
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