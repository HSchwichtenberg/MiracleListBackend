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
   var ctx = new DAL.Context();
   var userCount = ctx.UserSet.Count();
   var taskCount = ctx.TaskSet.Count();
   var clientCount = ctx.ClientSet.Count();
   var logCount =  -1; // ctx.LogSet.Count();

   // TODO: https://blogs.msdn.microsoft.com/martijnh/2010/07/15/sql-serverhow-to-quickly-retrieve-accurate-row-count-for-table/
   // SELECT CONVERT(bigint, rows)
//   FROM sysindexes
//WHERE id = OBJECT_ID('Transactions')
//AND indid< 2

   SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ctx.Database.GetDbConnection().ConnectionString);
   var conn = ctx.Database.GetDbConnection();
   conn.Open();
   var DbVersion = ctx.Database.GetDbConnection()?.ServerVersion;
   conn.Close();
   t.Stop();

   return new string[] { "MiracleListBackend", "(C) Dr. Holger Schwichtenberg, www.IT-Visions.de", "Server: " + System.Environment.MachineName, "Server-Version: " + Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion, "Datenbank: " + builder?.DataSource, "Datenbank-Version:" + DbVersion, clientCount + " Clients", userCount + " Benutzer", taskCount + " Aufgaben", logCount + " Protokolleinträge", DateTime.Now.ToString(), t.ElapsedMilliseconds + "ms"
  };
  }
 }
}
