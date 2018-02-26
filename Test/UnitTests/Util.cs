using ITVisions.EFCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;


// XUNIT: https://xunit.github.io/docs/getting-started-dotnet-core.html

namespace UnitTests
{
 public class Util
 {

  public static void Init()
  {
   var cs = Util.GetConnectionString();
   DAL.Context.ConnectionString = cs;

   if (cs == "SQLite")
   {
    DAL.Context.Connection = Util.conn;
    var ctx = new DAL.Context();
    ctx.Database.EnsureCreated();
   }

   //var serviceProvider = new ServiceCollection()
   //      .AddDbContext<DAL.Context>(opt=>opt.UseSqlite(Util.conn));
  }

  public static SqliteConnection _conn;
  public static SqliteConnection conn {  get
   {
    if (_conn != null) return _conn;
    _conn = new SqliteConnection("DataSource=:memory:");
    _conn.Open();
    return _conn; 
   }
  }

  //public static DbContextOptionsBuilder<DAL.Context> builder
  //{
  // get
  // {
  //  return new DbContextOptionsBuilder<DAL.Context>().UseSqlite(_conn);
  // }
  //}



  /// <summary>
  /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?tabs=basicconfiguration
  /// 
  /// TODO: Env wird ignoriert :-(
  /// </summary>
  /// <returns></returns>
  public static string GetConnectionString()
  {

   //HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();

   // Wenn es einen Eintrag in mehr als einer Datei gibt, gewinnt der zuletzt hinzugefügte Eintrag

   var dic = new Dictionary<string, string> { { "ConnectionStrings:MiracleListDB", "" } };

   var builder = new ConfigurationBuilder() // NUGET: Microsoft.Extensions.Configuration
   .AddInMemoryCollection(dic)
   .AddJsonFile("appsettings.json") // NUGET: Microsoft.Extensions.Configuration.Json
   .AddEnvironmentVariables(); // NUGET: Microsoft.Extensions.Configuration.EnvironmentVariables


   IConfigurationRoot configuration = builder.Build();

   var e = System.Environment.GetEnvironmentVariable("ConnectionStrings:MiracleListDB");
   var cs = configuration["ConnectionStrings:MiracleListDB"];
   DAL.Context.IsRuntime = true;
   DAL.Context.ConnectionString = cs;
   //var ctx = new DAL.Context();
   //ctx.Log();

   Console.WriteLine(cs);

   return cs;
  }

 }
}
