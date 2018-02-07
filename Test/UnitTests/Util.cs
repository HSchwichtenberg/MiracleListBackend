using Microsoft.Extensions.Configuration;
using System.Collections.Generic;


// XUNIT: https://xunit.github.io/docs/getting-started-dotnet-core.html

namespace UnitTests
{
 public class Util
 {

  /// <summary>
  /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?tabs=basicconfiguration
  /// 
  /// TODO: Env wird ignoriert :-(
  /// </summary>
  /// <returns></returns>
  public static string GetConnectionString()
  {

   // Wenn es einen Eintrag in mehr als einer Datei gibt, gewinnt der zuletzt hinzugefügte Eintrag

   var dic = new Dictionary<string, string> { { "ConnectionStrings:MiracleListDB", "" } };

   var builder = new ConfigurationBuilder() // NUGET: Microsoft.Extensions.Configuration
   .AddInMemoryCollection(dic)
   .AddJsonFile("appsettings.json") // NUGET: Microsoft.Extensions.Configuration.Json
   .AddEnvironmentVariables(); // NUGET: Microsoft.Extensions.Configuration.EnvironmentVariables

   IConfigurationRoot configuration = builder.Build();


   var e = System.Environment.GetEnvironmentVariable("ConnectionStrings:MiracleListDB");


   var cs = configuration["ConnectionStrings:MiracleListDB"];
   cs = "";
   return cs;
  }

 }
}
