using ITVisions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace UnitTests
{

 public class Util
 {

  /// <summary>
  /// Will be called in the constructor of each test class
  /// </summary>
  public static void Init()
  {
   lock (ConnectionString)
   {
    if (ConnectionString == "notset")
    {
     ConnectionString = Util.GetConnectionString();
     switch (ConnectionString)
     {
      case "SQLite":
       DAL.Context.Connection = Util.SQLiteInMemoryConnection;
       CUI.PrintSuccess("Connection to SQLite InMemory");
       break;
       // as "" will not be working with Environment Variables, we must offer other options here as well
      case "":
      case "-":
      case "InMem":
      case "InMemory":
      case "InMemoryDB":
       DAL.Context.ConnectionString = "";
       CUI.PrintSuccess("Connection to InMemoryDB");
       break;
      default:
       DAL.Context.ConnectionString = ConnectionString;
       // Enable EF Profiler
       HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
       CUI.PrintSuccess("Connection to " + ConnectionString);
       break;
     }
     DAL.Context.IsRuntime = true;
    }
   }

    //var serviceProvider = new ServiceCollection()
    //      .AddDbContext<DAL.Context>(opt=>opt.UseSqlite(Util.conn));
   }

  public static string ConnectionString = "notset";

   public static SqliteConnection _SQLiteInMemoryConnection;
  public static SqliteConnection SQLiteInMemoryConnection {  get
   {
    if (_SQLiteInMemoryConnection == null)
    {
     _SQLiteInMemoryConnection = new SqliteConnection("DataSource=:memory:");
     _SQLiteInMemoryConnection.Open();
     var ctx = new DAL.Context();
     ctx.Database.EnsureCreated();
    }
    return _SQLiteInMemoryConnection; 
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
  /// Get Connection String from Memory, AppSettings.json or Environment
  /// </summary>
  /// <returns></returns>
  public static string GetConnectionString()
  {
   // Build configuration sources (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?tabs=basicconfiguration)
   var dic = new Dictionary<string, string> { { "ConnectionStrings:MiracleListDB", "" } };
   var builder = new ConfigurationBuilder() // NUGET: Microsoft.Extensions.Configuration
   .AddInMemoryCollection(dic)
   .AddJsonFile("appsettings.json") // NUGET: Microsoft.Extensions.Configuration.Json
   .AddEnvironmentVariables(); // NUGET: Microsoft.Extensions.Configuration.EnvironmentVariables e.g. "ConnectionStrings:MiracleListDB"
   IConfigurationRoot configuration = builder.Build();
   var cs = configuration["ConnectionStrings:MiracleListDB"];
   Console.WriteLine("ENV: " + System.Environment.GetEnvironmentVariable("ConnectionStrings:MiracleListDB"));
   return cs;
  }
 }
}
