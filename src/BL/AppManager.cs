using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BL
{
 public class AppManager
 {
  public IEnumerable<string> GetAppInfo()
  {
   var t = new Stopwatch();
   t.Start();


   // TODO: https://blogs.msdn.microsoft.com/martijnh/2010/07/15/sql-serverhow-to-quickly-retrieve-accurate-row-count-for-table/
   // SELECT CONVERT(bigint, rows)
   //   FROM sysindexes
   //WHERE id = OBJECT_ID('Transactions')
   //AND indid< 2

   string DbVersion = "?";
   string DbName = "?";
   string DbStatus = "?";
   var userCount = -1;
   var taskCount = -1;
   var clientCount = -1;
   var logCount = -1;

   try
   {
    var ctx = new DAL.Context();
    ctx.Database.SetCommandTimeout(new TimeSpan(0, 0, 10));
    userCount = ctx.UserSet.Count();
    taskCount = ctx.TaskSet.Count();
    clientCount = ctx.ClientSet.Count();
    logCount = 0;
    //logCount = ctx.LogSet.Count();

    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ctx.Database.GetDbConnection().ConnectionString);
    DbName = builder?.DataSource;
    var conn = ctx.Database.GetDbConnection();
    conn.Open();
    DbVersion = ctx.Database.GetDbConnection()?.ServerVersion;
    conn.Close();
    DbStatus = "OK";
   }
   catch (Exception ex)
   {
    DbStatus = "Error: " + ex.Message;
   }

   t.Stop();

   return new string[] { DateTime.Now.ToString(), "MiracleListBackend", "(C) Dr. Holger Schwichtenberg, www.IT-Visions.de", "Web Server: " + System.Environment.MachineName, "Server Version: " + Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion, ".NET Core Version: " + ITVisions.CLRInfo.GetCoreClrVersion(), "Database Status: " + DbStatus, "Database Name: " + DbName, "Database Version: " + DbVersion, clientCount + " Clients", userCount + " Users", taskCount + " Tasks", logCount + " Log Entries", t.ElapsedMilliseconds + "ms"
  };
  }
 }
}
