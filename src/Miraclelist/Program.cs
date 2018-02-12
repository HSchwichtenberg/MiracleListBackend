﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore;
using ITVisions;

namespace Miraclelist
{

 /// <summary>
 /// Start with: dotnet run --hosturl http://192.168.1.66:5001
 /// </summary>
 public class Program
 {

  /// <summary>
  /// Start code for ASP.NET Core 2.0, see https://docs.microsoft.com/en-us/aspnet/core/migration/1x-to-2x/
  /// </summary>
  /// <param name="args"></param>
  public static void Main(string[] args)
  {

   CUI.MainHeadline("MiracleList Backend v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
   CUI.Print("ASP.NET Core v" + typeof(WebHost).Assembly.GetName().Version.ToString());
   CUI.Print("Runtime: .NET Core v " + ITVisions.CLRInfo.GetCoreClrVersion());

   var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
     .AddCommandLine(args)
     .Build();

   var hostUrl = configuration["hosturl"];
   if (string.IsNullOrEmpty(hostUrl))
    hostUrl = "http://localhost:6000";

   WebHost.CreateDefaultBuilder(args)
    .UseUrls(hostUrl)
    .UseSetting("detailedErrors", "true")
    .UseStartup<Startup>()
    .CaptureStartupErrors(true)
    .Build().Run();
  }

  // old start code for ASP.NET Core 1.x
  //public static void Main(string[] args)
  //{
  // var host = new WebHostBuilder()
  //              .UseKestrel()
  //              .UseUrls(hostUrl)  
  //              .UseContentRoot(Directory.GetCurrentDirectory())
  //              .UseIISIntegration()
  //              .UseStartup<Startup>()
  //              .Build();

  // host.Run();
  //}
 }
}
