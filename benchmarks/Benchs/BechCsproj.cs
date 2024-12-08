using System.Net.Http.Json;
using BenchmarkDotNet.Attributes;
using IziHardGames.IziProjectsManager.Common.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Benchs
{
    [MemoryDiagnoser]
    public class BechCsproj
    {
        private HttpClient apiClient;
        private CsprojDto[]? dtos;

        [GlobalSetup]
        public void Setup()
        {
            var factory = new WebApplicationFactory<IziLibraryApiGate.Program>();
            //.WithWebHostBuilder(configuration =>
            //{

            //});

            this.apiClient = factory.CreateClient();

            var response =  this.apiClient.GetAsync("api/Tables/Csprojs").GetAwaiter().GetResult();
            this.dtos = response.Content.ReadFromJsonAsync<CsprojDto[]>().GetAwaiter().GetResult();
        }

        [Benchmark]
        public void Single()
        {
            var ss = 1.ToString();
            foreach (var dto in dtos)
            {
                string s = dto.ToString();
                Console.WriteLine(s);
            }
        }
    }
}
