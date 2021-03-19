using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile("ocelot.json", false, true)
                        .AddJsonFile("ocelot.json")
                        .AddEnvironmentVariables();
                })
                .ConfigureServices(s => {
                    
                    s.AddAuthentication("Bearer")
                        .AddJwtBearer("Bearer", options =>
                        {
                            options.Authority = "http://localhost:5005";
                            options.RequireHttpsMetadata = false;
                    
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateAudience = false
                            };
                        });
                    
                    s.AddOcelot();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    //add your logging
                })
                .Configure(app =>
                {
                    app.UseOcelot().Wait();
                })
                .Build()
                .Run();
        }
    }
}