using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using BO;
using DAL;
using ITVisions.EFC;
using ITVisions.EFCore;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BL
{
 public class UserManager : EntityManagerBase<Context, User>
 {
  public User CurrentUser = null;
  private CategoryManager cm;
  private TaskManager tm;


  // nur für Test
  public UserManager(Context ctx) 
  {
   this.ctx = ctx;
  }

  public UserManager()
  {

  }

  /// <summary>
  /// Erzeugen einer Instanz der Klasse mit Benutzertoken. Speichert letzte Verwendung in User.LastActivity.
  /// </summary>
  public UserManager(string token, bool CreateIfNotExists = false, bool PasswordReset = false)
  {
   this.CurrentUser = GetUserByToken(token, CreateIfNotExists, PasswordReset);


   if (this.CurrentUser != null)
   {
    cm = new CategoryManager(this.CurrentUser.UserID);
    tm = new TaskManager(this.CurrentUser.UserID);
   }
  }

  /// <summary>
  /// Erzeugen einer Instanz der Klasse mit Benutzername und Kennwort. Wenn die Daten gültig sind, wird der Benutzer angemeldet und erhält ein neues Token (eine GUID!)
  /// Besonderheit NUR FÜR DEMOANWENDUNG: Wenn das der Benutzer nicht existiert, wird ein neuer Benutzer anlegt, mit dem Benutzername und dem genannten Kennwort
  /// </summary>
  public UserManager(string username, string password, string token = "")
  {
   this.CurrentUser = GetOrCreateUser(username, password, token);
   if (this.CurrentUser != null)
   {
    cm = new CategoryManager(this.CurrentUser.UserID);
    tm = new TaskManager(this.CurrentUser.UserID);
   }
  }

  /// <summary>
  /// Abmelden des Benutzers, der das genannte Token besitzt. Das Token wird in DB gelöscht!
  /// </summary>
  /// <param name="token"></param>
  /// <returns></returns>
  public static bool Logoff(string token)
  {
   try
   {
    var ctx = new Context();
    var u = ctx.UserSet.SingleOrDefault(x => x.Token.ToLower() == token.ToLower());
    if (u == null) return false;
    u.Token = "";
    ctx.SaveChanges();
    return true;
   }
   catch
   {
    return false;
   }
  }


  private User GetUserByToken(string token, bool CreateIfNotExists = true, bool PasswordReset = false )
  {
   //Guid guid;
   //if (!Guid.TryParse(token, out guid)) return null;
   var ctx = new Context();
   var u = ctx.UserSet.SingleOrDefault(x => x.Token.ToLower() == token.ToLower());
   if (u != null)
   {
    u.LastActivity = DateTime.Now;
    ctx.SaveChanges();
    return u;
   }
   if (!CreateIfNotExists) return null;
   // nur zur Demo: Wenn es Token nicht gibt, wird AdHoc ein neuer User dafür erzeugt
   return GetOrCreateUser(token, token, token, PasswordReset);
  }

  private User GetOrCreateUser(string name, string password, string token = "", bool PasswordReset = false)
  {
   this.StartTracking();

   var u = ctx.UserSet.SingleOrDefault(x => x.UserName.ToLower() == name.ToLower());

   if (u != null)
   {
    // stimmt das Kennwort?
    var hashObj = ITVisions.Security.Hashing.HashPassword(password, u.Salt);

    if (u.PasswordHash != hashObj.HashedText)
    {
     if (!PasswordReset) return null;
    }

    u.PasswordHash = hashObj.HashedText;
    u.Salt = hashObj.Salt;
    if (String.IsNullOrEmpty(u.Token)) u.Token = Guid.NewGuid().ToString("D");
    else if (!String.IsNullOrEmpty(token)) { u.Token = token; }

    u.Memo += "Login " + DateTime.Now + "/" + password + "/" + u.Token + "\n";
    ctx.SaveChanges();
    this.SetTracking();
    return u;
   }

   if (u == null)
   {
    u = new User();
    u.UserName = name;
    var hashObj = ITVisions.Security.Hashing.HashPassword(password);
    u.PasswordHash = hashObj.HashedText;
    u.Salt = hashObj.Salt;
    u.Created = DateTime.Now;
    if (token == "") token = Guid.NewGuid().ToString("D");
    u.Token = token;
   }
   // 38 zeichen mit { und -
   u.Memo = "Created " + DateTime.Now + "/" + password + "\n";
   this.New(u);

   return u;
  }

  /// <summary>
  /// DEMOMODUS: Erzeugt für den aktuellen Benutzer einige Standardaufgaben. Wird immer bei jeder Operation aufergufen, um sicherzustellen, dass ein Benutzer immer einige Aufgaben beseitzt
  /// </summary>
  public void InitDefaultTasks()
  {
   if (ctx.CategorySet.Where(x => x.UserID == CurrentUser.UserID).Count() > 0) return;

   var st01 = new SubTask();
   st01.Title = "Aufgaben in Kategorie Beruf ansehen";
   var st02 = new SubTask();
   st02.Title = "Aufgaben in Kategorie Haushalt ansehen";
   var st03 = new SubTask();
   st03.Title = "Aufgaben in Kategorie Freizeit ansehen";

   var c0 = cm.CreateCategory("Über die App");
   var t01 = tm.CreateTask(c0.CategoryID, "Beispielaufgaben erforschen", "Jeder neue Benutzer erhält automatisch einige Beispielaufgaben in vier Kategorien. ACHTUNG: Wenn Sie die letzte Aufgabe löschen, werden die Beispielaufgaben automatisch beabsichtigt alle wieder angelegt :-)", DateTime.Now.AddHours(3), Importance.A, 1, new List<SubTask>() { st01, st02, st03 });

   var st02a = new SubTask() { Title = "Mithelfen, das Beispiel besser zu machen: https://github.com/HSchwichtenberg/MiracleListClient" };

   var t02 = tm.CreateTask(c0.CategoryID, "Verstehen, dass MiracleList eine Beispiel-Anwendung ist und kein fertiges Produkt.", "Es geht in diesem Beispiel darum, möglichste viele Techniken zu zeigen und nicht darum, bis wie bei einem echten Produkt exakt und rein zu programmieren.", DateTime.Now.AddHours(3), Importance.A, 1, new List<SubTask>() { st02a });

   var t04 = tm.CreateTask(c0.CategoryID, "Web- und Cross-Platform-Techniken lernen", "Wenn Sie die hier eingesetzen Techniken (.NET Core, C#, ASP.NET Core WebAPI, Entity Framework Core, SQL Azure, Azure Web App, Swagger, HTML, CSS, TypeScript, Angular, Bootstrap, MomentJS, angular2-moment, angular2-contextmenu, angular2-modal, Electron, Cordova etc.) lernen wollen, besuchen Sie www.IT-Visions.de/ST", DateTime.Now.AddDays(30), Importance.B, 40, null);

   var st03a = new SubTask() { Title = "Client-ID beantragen: https://miraclelistbackend.azurewebsites.net/client" };
   var st03b = new SubTask() { Title = "Client programmieren" };

   var t03 = tm.CreateTask(c0.CategoryID, "Sie können selbst einen eigenen MiracleList-Client schreiben.", "Das Backend steht Ihnen dafür zur Verfügung: https://miraclelistbackend.azurewebsites.net", DateTime.Now.AddDays(60), Importance.C, 100, new List<SubTask>() { st03a, st03b });

   var st1 = new SubTask();
   st1.Title = "Teil 1";
   var st2 = new SubTask();
   st2.Title = "Teil 2";
   var st3 = new SubTask();
   st3.Title = "Teil 3";

   var c1 = cm.CreateCategory("Beruf");
   var t10 = tm.CreateTask(c1.CategoryID, "Angular-Tutorial für die iX schreiben", "Teil 1: Einrichten eines Angular-Projekts, Datenabruf von REST-Diensten, Rendern von Daten per Template\nTeil 2: Routing, Formulare für das Einfügen, Ändern und Löschen von Daten, Senden von Daten an REST-Dienste\nTeil 3: Menü, Kontextmenü, Dialogfenster, Animationen, Benutzeranmeldung und Auslieferung des Projekts", DateTime.Now.AddDays(30), Importance.A, 40, new List<SubTask>() { st1, st2, st3 });

   st1 = new SubTask();
   st1.Title = "Planen";
   st2 = new SubTask();
   st2.Title = "Ausführen";

   var t11 = tm.CreateTask(c1.CategoryID, "Projektplan erstellen", "Beispielaufgabe", DateTime.Now.AddDays(-2), Importance.A, 2, new List<SubTask>() { st1, st2 });

   st1 = new SubTask();
   st1.Title = "Planen";
   st2 = new SubTask();
   st2.Title = "Ausführen";

   var t12 = tm.CreateTask(c1.CategoryID, "Teambesprechung abhalten", "Beispielaufgabe", DateTime.Now.AddDays(7), Importance.B, 3, new List<SubTask>() { st1, st2 });

   st1 = new SubTask();
   st1.Title = "Planen";
   st2 = new SubTask();
   st2.Title = "Ausführen";
   var t13 = tm.CreateTask(c1.CategoryID, "Schulungen buchen", "siehe www.IT-Visions.de", DateTime.Now.AddDays(-10), Importance.B, 1, new List<SubTask>() { st1, st2 });

   var c2 = cm.CreateCategory("Haushalt");
   st1 = new SubTask();
   st1.Title = "Planen";
   st2 = new SubTask();
   st2.Title = "Ausführen";

   var t21 = tm.CreateTask(c2.CategoryID, "Saugen", "Beispielaufgabe", DateTime.Now.AddDays(2), Importance.B, 1.25m, new List<SubTask>() { st1, st2 });

   st1 = new SubTask();
   st1.Title = "Planen";
   st2 = new SubTask();
   st2.Title = "Ausführen";

   var t22 = tm.CreateTask(c2.CategoryID, "Müll herausbringen", "Beispielaufgabe",
 DateTime.Now.AddDays(1), Importance.A, 0.5m, new List<SubTask>() { st1, st2 });

   var c3 = cm.CreateCategory("Freizeit");
   st1 = new SubTask();
   st1.Title = "Planen";
   st2 = new SubTask();
   st2.Title = "Ausführen";
   var t31 = tm.CreateTask(c3.CategoryID, "Trainieren für MTB-Marathon", "Beispielaufgabe", DateTime.Now.AddDays(1), Importance.A, 120, new List<SubTask>() { st1, st2 });

   st1 = new SubTask();
   st1.Title = "Planen";
   st2 = new SubTask();
   st2.Title = "Ausführen";
   var t32 = tm.CreateTask(c3.CategoryID, "Kino", "Beispielaufgabe", DateTime.Now.AddDays(14), Importance.B, 3.5m, new List<SubTask>() { st1, st2 });

  }

  public TokenValidationResult IsValid()
  {
   if (this.CurrentUser == null) return TokenValidationResult.TokenUngültig;
   return TokenValidationResult.Ok; // alle OK. Aussperren von User noch nicht realisiert!
  }

  public enum TokenValidationResult
  {
   Ok, TokenUngültig, BenutzerIstDeaktiviert
  }


  public static List<User> GetLatestUserSet()
  {
   using (var ctx = new Context())
   {
    var r = ctx.UserSet.FromSql("Select * from [User]").OrderByDescending(x => x.Created).Take(10).ToList();

    return r;
   }

  }

  public static List<UserStatistics> GetUserStatistics()
  {
   using (var ctx = new Context())
   {
    ctx.Log((x) =>
   {
    System.Diagnostics.Debug.WriteLine(x);
    using (StreamWriter sw = File.AppendText(@"c:\temp\EFCLog.txt"))
    {
     sw.WriteLine(x);
    }

   }
    );


    //var groups = (from u in ctx.UserSet
    //              join x in ((from p in ctx.TaskSet
    //                          group p by p.Category.UserID into g
    //                          select new { userID = g.Key, Count = g.Count() }).OrderBy(x => x.Count).Take(10))
    //               on u.UserID equals x.userID
    //              select new { u.UserName, x.Count });

    //var r = new List<UserStatistics>();
    //foreach (var g in groups)
    //{
    // r.Add(new UserStatistics() { UserName = g.UserName, NumberOfTasks = g.Count });
    //}

    var SQL = @"SELECT[User].UserName, COUNT(Task.TaskID) AS NumberOfTasks FROM Category INNER JOIN
                          Task ON Category.CategoryID = Task.CategoryID INNER JOIN
                          [User] ON Category.UserID = [User].UserID
                          GROUP BY[User].UserName";

    var r = ctx.UserStatistics.FromSql(SQL).OrderByDescending(x => x.NumberOfTasks).Take(10).ToList();

    return r;
   }
  }

 }


}
