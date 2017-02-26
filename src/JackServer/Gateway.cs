using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace JackServer
{
    internal class Gateway : IDisposable
    {
        private readonly IWebHost _host;

        public Gateway(IConfigurationRoot config)
        {
            var gatewayUrls = config.GetValue("gateway:urls", "http://*:8080");
            _host = new WebHostBuilder()
                .UseUrls(gatewayUrls)
                .UseKestrel()
                .Build();
        }

        public void Start()
        {
            _host.Start();
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}