using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using ElectronNET.API;

namespace Discord_Custom_Status
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .UseElectron(args);
                });
    }
}
