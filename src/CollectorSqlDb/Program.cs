using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using SqlCollectorDb.Data;
using Microsoft.EntityFrameworkCore;

namespace SqlCollectorDb
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureHostConfiguration(configHost =>
				{
					configHost.SetBasePath(Directory.GetCurrentDirectory());
					configHost.AddJsonFile("hostsettings.json", optional: true);
					configHost.AddEnvironmentVariables(prefix: "ASPNETCORE_");
					configHost.AddCommandLine(args);
				})
				.ConfigureAppConfiguration((hostContext, configApp) =>
				{
					configApp.AddJsonFile("appsettings.json", optional: false);
					configApp.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: false);
				})
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<WorkerService>();
					services.AddDbContext<SqlCollectorDbContext>(options =>
						options.UseSqlServer(hostContext.Configuration.GetConnectionString("SqlCollectorDb")));
				}).UseConsoleLifetime();
	}
}
