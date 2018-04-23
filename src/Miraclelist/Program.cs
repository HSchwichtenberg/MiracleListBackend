using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore;
using ITVisions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System;

namespace Miraclelist
{

 /// <summary>
 /// Start with: dotnet run --hosturl http://192.168.1.66:5000
 /// </summary>
 public class Program
 {



  public static string GetNetCoreVersion()
  {
   var assembly = typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly;
   Console.WriteLine(assembly.FullName);
   Console.WriteLine(assembly.CodeBase);
   var assemblyPath = assembly.CodeBase.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
   int netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
   if (netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
    return assemblyPath[netCoreAppIndex + 1];

   //// Versuch Ubuntu https://github.com/dotnet/BenchmarkDotNet/issues/448
   //netCoreAppIndex = Array.IndexOf(assemblyPath, "System.Private.CoreLib");
   //if (netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
   // return assemblyPath[netCoreAppIndex + 1];

   return null;
  }
  /// <summary>
  /// Start code for ASP.NET Core 2.0, see https://docs.microsoft.com/en-us/aspnet/core/migration/1x-to-2x/
  /// </summary>
  /// <param name="args"></param>
  public static void Main(string[] args)
  {

   CUI.MainHeadline("MiracleList Backend v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

   CUI.Headline("Main");
   CUI.Print("Runtime: .NET Core v" + GetNetCoreVersion());
   CUI.Print("Webframework: ASP.NET Core v" + typeof(WebHost).Assembly.GetName().Version.ToString());
   // TODO: GetCoreClrVersion() geht nicht auf LINUX! :-(
  

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
