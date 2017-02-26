using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace JackServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddCommandLine(args)
                .AddEnvironmentVariables("JACK_")
                .Build();

            var gateway = new Gateway(config);

            var adminUrls = config.GetValue("admin:urls", "http://*:6464");

            var managementHost = new WebHostBuilder()
                .UseUrls(adminUrls)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            using (gateway)
            {
                gateway.Start();
                managementHost.Run();
            }
        }
    }
}
