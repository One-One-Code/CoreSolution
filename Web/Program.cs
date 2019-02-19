using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using log4net;
using log4net.Repository;
using log4net.Config;
using OneOne.Core.Logger;
using Microsoft.Extensions.Configuration;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.LogRegister(new FileInfo("logger.config"));
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true).Build();
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseConfiguration(config)
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
