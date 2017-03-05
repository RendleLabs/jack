using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace JackServer
{
    internal class Gateway : IDisposable
    {
        private readonly IWebHost _host;
        private readonly HttpClient _client;

        public Gateway(IConfigurationRoot config)
        {
            var gatewayUrls = config.GetValue("gateway:urls", "http://*:8080");
            _host = new WebHostBuilder()
                .UseUrls(gatewayUrls)
                .UseKestrel()
                .Configure(Configure)
                .Build();
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:5000");
        }

        private void Configure(IApplicationBuilder app)
        {
            app.Use(_ => async ctx =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, ctx.Request.Path);
                var path = ctx.Request.Path;
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                await CopyResponse(response, ctx.Response);
            });
        }

        public void Start()
        {
            _host.Start();
        }

        public void Dispose()
        {
            _host.Dispose();
        }

        private static async Task CopyResponse(HttpResponseMessage from, HttpResponse to)
        {
            to.StatusCode = (int)from.StatusCode;
            foreach (var kvp in from.Headers)
            {
                if (!kvp.Key.Equals("transfer-encoding", StringComparison.OrdinalIgnoreCase))
                {
                    System.Console.WriteLine($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
                    to.Headers.Add(kvp.Key, new StringValues(kvp.Value.ToArray()));
                }
            }
            foreach (var kvp in from.Content.Headers)
            {
                if (!kvp.Key.Equals("transfer-encoding", StringComparison.OrdinalIgnoreCase))
                {
                    System.Console.WriteLine($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
                    to.Headers.Add(kvp.Key, new StringValues(kvp.Value.ToArray()));
                }
            }
            await from.Content.CopyToAsync(to.Body);
        }
    }
}