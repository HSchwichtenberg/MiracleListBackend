﻿using BL;
using ITVisions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

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
       CUI.PrintSuccess("Connection to InMemoryDB!");
       break;
      default:
       DAL.Context.ConnectionString = ConnectionString;
       // Enable EF Profiler
       HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
       CUI.PrintSuccess("Connection to: " + ConnectionString);
       break;
     }
     DAL.Context.IsRuntime = true;

     var um2 = new UserManager("unittest", "unittest");
     um2.InitDefaultTasks();
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
  /// Developer PC: will switch Connection String using appsettings.json, can optional use launchsettings.json
  /// Server Build: will set $ENV:ConnectionStrings:MiracleListDB, which has higher Priority
  /// </summary>
  /// <returns></returns>
  public static string GetConnectionString()
  {
   var e1 = System.Environment.GetEnvironmentVariable("ConnectionStrings:MiracleListDB", EnvironmentVariableTarget.Process);
   var e2 = System.Environment.GetEnvironmentVariable("ConnectionStrings:MiracleListDB", EnvironmentVariableTarget.User);
   var e3 = System.Environment.GetEnvironmentVariable("ConnectionStrings:MiracleListDB", EnvironmentVariableTarget.Machine);

   Console.WriteLine("ENV Process: " + e1);
   Console.WriteLine("ENV User: " + e2);
   Console.WriteLine("ENV Machine: " + e3);


   if (String.IsNullOrEmpty(e1))
   {
    // Launch Settings werden nicht automatisch in Unit Test-Projekten berücksichtigt :-(
    // https://stackoverflow.com/questions/43927955/should-getenvironmentvariable-work-in-xunit-test
    using (var file = File.OpenText("Properties\\launchSettings.json"))
    {
     var reader = new JsonTextReader(file);
     var jObject = JObject.Load(reader);
     var csLaunchSettings = jObject["profiles"]?["UnitTests"]?["environmentVariables"]["ConnectionStrings:MiracleListDB"]?.Value<string>();
     if (!String.IsNullOrEmpty(csLaunchSettings)) System.Environment.SetEnvironmentVariable("ConnectionStrings:MiracleListDB", csLaunchSettings);
    }
   }


   // Build configuration sources (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?tabs=basicconfiguration)
   var dic = new Dictionary<string, string> { { "ConnectionStrings:MiracleListDB", "" } };
   var builder = new ConfigurationBuilder() // NUGET: Microsoft.Extensions.Configuration
   .AddInMemoryCollection(dic)
   .AddJsonFile("appsettings.json") // NUGET: Microsoft.Extensions.Configuration.Json
   .AddEnvironmentVariables(); // NUGET: Microsoft.Extensions.Configuration.EnvironmentVariables e.g. "ConnectionStrings:MiracleListDB"
   IConfigurationRoot configuration = builder.Build();
   var cs = configuration["ConnectionStrings:MiracleListDB"];

 


   return cs;
  }
 }
}
