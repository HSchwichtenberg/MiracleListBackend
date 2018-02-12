using BO;
using DAL;
using ITVisions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EFTools
{
 public class Program
 {


  private static string GetConnectionString()
  {

   // Wenn es einen Eintrag in mehr als einer Datei gibt, gewinnt der zuletzt hinzugefügte Eintrag

   var dic = new Dictionary<string, string> { { "ConnectionStrings:MiracleListDB", "Data Source=D120;Initial Catalog=MiracleList_Test;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False" } };
   
   var builder = new ConfigurationBuilder() // NUGET: Microsoft.Extensions.Configuration
   .AddInMemoryCollection(dic)
   .AddJsonFile("appsettings.json") // NUGET: Microsoft.Extensions.Configuration.Json
   .AddEnvironmentVariables(); // NUGET: Microsoft.Extensions.Configuration.EnvironmentVariables

   IConfigurationRoot configuration = builder.Build();

   var cs = configuration["ConnectionStrings:MiracleListDB"];
   return cs;
  }

  public static void Main(string[] args)
  {

   //var m = new ITVisions.Mail.MailUtil();
   //m.SendEmail("test", "hs@IT-Visions.de", "HS", "hs@IT-Visions.de", "TEST BETREFF", "INHALT");


   //var m = new ITVisions.Mail.MailGun();

   //new ITVisions.NetworkUtil.MailUtil().SendMailTollerant("hs@IT-Visions.de", "hs@IT-Visions.de", "TEST BETREFF", "INHALT");
   PrintInfo("MiracleList Backend EFC Tools " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

   Context.ConnectionString = GetConnectionString();

   PrintInfo("Connection String:" + Context.ConnectionString);

   if (String.IsNullOrEmpty(Context.ConnectionString))
   {
    PrintError("No Connection String!");
    System.Environment.Exit(1);
   }

   if (args.Count() == 0)
   {
    CUI.PrintError("Missing Parameter: migrate | createtestuser | both");
    System.Environment.Exit(1);
   }

   switch (args[0].ToLower())
   {
    case "migrate": Migrate(); break;
    case "createtestuser": CreateTestUser(); break;
    case "both": Migrate();  CreateTestUser(); break;
    default: Migrate(); CreateTestUser(); break;
   }


   //var ctx = new Context();
   //var sts = ctx.Set<SubTask>().Where(st=>st.Done==true).ToList();
   //Console.WriteLine(sts.Count);


   PrintInfo("DONE!");
   System.Environment.Exit(0);
   //Console.ReadLine();

  }

  private static void Migrate()
  {
   PrintInfo("Migrate Database...");

   try
   {
    var ctx = new Context();

    var mset = ctx.Database.GetMigrations();
    foreach (var m in mset)
    {
     PrintInfo(m);
    }

    ctx.Database.Migrate();

   }
   catch (Exception ex)
   {
    PrintError("Migration Error", ex);
    System.Environment.Exit(2);
   }
  }

  private static void CreateTestUser()
  {
   try
   {
    var zeit = DateTime.Now.ToString();

    var um = new BL.UserManager("test", "test");
    um.InitDefaultTasks();
    var cm = new BL.CategoryManager(um.CurrentUser.UserID);
    var cs = cm.GetCategorySet();

    PrintInfo(cs.Count + " Tasks for User ID=" + um.CurrentUser.UserID + " (" + um.CurrentUser.UserName +")");
    if (cs.Count != 4)
    {
     PrintError("Data Test Error: Count=" + cs.Count);
     System.Environment.Exit(3);
    }
   }
   catch (Exception ex)
   {
    PrintError("Data Test Error", ex);
    System.Environment.Exit(4);
    throw;
   }
  }
  public static void PrintInfo(string s)
  {
   // VSO Logging Commands https://github.com/Microsoft/vsts-tasks/blob/master/docs/authoring/commands.md
   //s = s += "##vso[task.logdetail]" + s;
   CUI.Print(s);
  }

  public static void PrintError(string s, Exception ex = null)
  {
   // VSO Logging Commands https://github.com/Microsoft/vsts-tasks/blob/master/docs/authoring/commands.md
   s = s += "##vso[task.logissue type=error;]" + s + (ex != null ? ": " + ex.Message : "");
   CUI.PrintError(s);
  }

 }
}
