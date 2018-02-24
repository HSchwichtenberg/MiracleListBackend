using BL;
using BO;
using ITVisions;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Miraclelist.Controllers
{
 /// <summary>
 /// Zweite API-Version
 /// </summary>
 [ApiExplorerSettings(GroupName = "v2")]
 [Route("v2")]

 public class MiracleListApiV2Controller : Controller
 {
  private TelemetryClient telemetry = new TelemetryClient();
  TaskManager tm; 
  CategoryManager cm;

  private IConfigurationRoot Configuration;

  public MiracleListApiV2Controller(IConfigurationRoot configuration)
  {
   this.Configuration = configuration;
  }

  /// <summary>
  /// Hilfsroutine für alle Actions mit auth
  /// </summary>
  private void Init()
  {
   var userID = Int32.Parse(HttpContext.User.Identity.Name);
   cm = new CategoryManager(userID);
   tm = new TaskManager(userID);
  }


  /// <summary>
  /// Informationen über den Server
  /// </summary>
  /// <returns></returns>
  
  [Route("About")]
  [HttpGet]
  public IEnumerable<string> About()
  {
   var s = new AppManager().GetAppInfo();
   s = s.Append("API-Version: v2");
   s = s.Append("Release-Date: " + this.Configuration["AppInfo:ReleaseDate"]);
   return s;
  }

  /// <summary>
  /// Liefert die Version des Servers als Zeichenkette
  /// </summary>
  /// <returns></returns>
  [Route("Version")]
  [HttpGet]
  
  public string Version()
  {
   return
   Assembly.GetEntryAssembly()
 .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
 .InformationalVersion.ToString();
  }

  /// <summary>
  /// Nur für einen Test
  /// </summary>
  /// <returns></returns>
  [Route("About2")]
  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet]
  public JsonResult GetAbout2()
  {
   var v = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
   var e = new string[] { "MiracleListBackend", "(C) Dr. Holger Schwichtenberg, www.IT-Visions.de", "Version: " + v };
   var r = new JsonResult(e);
   this.Response.Headers.Add("X-Version", v);
   r.StatusCode = 202;
   return r;
  }

  /// <summary>
  /// Anmeldung mit einer Client-ID, einem Benutzernamen und einem Kennwort. Diese Operation /Login sendet eine GUID als Sitzungstoken zurück, welches in allen folgenden Operationen mitzugeben ist.
  /// </summary>
  /// <param name="loginInfo"></param>
  /// <returns></returns>
  [HttpPost("Login")] // neu
  public async System.Threading.Tasks.Task<LoginInfo> Login([FromBody] LoginInfo loginInfo)
  {

   return await new MiracleListApiController(this.Configuration).Login(loginInfo);
   
  }

  /// <summary>
  /// Abmelden des Benutzers
  /// </summary>
  
  /// <returns></returns>
  //[ApiExplorerSettings(GroupName = "Security")]
  [HttpGet("Logoff")] // neu
  public bool Logoff(string token)
  {
   return UserManager.Logoff(token);
  }

  /// <summary>
  /// Liste der Kategorien 
  /// </summary>
  
  /// <returns></returns>
  [Authorize(AuthenticationSchemes = "MLToken")]
  [HttpGet("CategorySet")]
  public IEnumerable<Category> GetCategorySet()
  {
   Init();
   return cm.GetCategorySet();
  }



  /// <summary>
  /// Liste der Aufgaben in einer Kategorie
  /// </summary>
  
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpGet("TaskSet/{id}")]
  [Authorize(AuthenticationSchemes = "MLToken")] // Auth Policy
  public IEnumerable<Task> GetTaskSet(int id)
  {
   if (id <= 0) throw new Exception("Ungültig ID!");
   Init();
   return tm.GetTaskSet(id);
  }

  /// <summary>
  /// Details zu einer Aufgabe mit Teilaufgaben
  /// </summary>
  
  /// <param name="id"></param>
  /// <returns></returns>
  [HttpGet("Task/{id}")]
  [Authorize(AuthenticationSchemes = "MLToken")]
  public Task Task(int id)
  {
   if (id <= 0) throw new Exception("Ungültig ID!");
   Init();
   return tm.GetTask(id);
  }

  /// <summary>
  /// Volltextsuche in Aufgaben und Teilaufgaben
  /// </summary>
  
  /// <param name="text"></param>
  /// <returns></returns>
  [HttpGet("Search/{text}")]
  [Authorize(AuthenticationSchemes = "MLToken")]
  public IEnumerable<Category> Search(string text)
  {
   Init();
   return tm.Search(text);
  }

  /// <summary>
  /// Liste der fälligen Aufgaben
  /// </summary>
  
  /// <returns></returns>
  [HttpGet("DueTaskSet")]
  [Authorize(AuthenticationSchemes = "MLToken")]
  public IEnumerable<Category> GetDueTaskSet()
  {
   Init();
   return tm.GetDueTaskSet();
  }

  /// <summary>
  /// Anlegen einer Kategorie
  /// </summary>
  
  /// <param name="name"></param>
  /// <returns></returns>
  [HttpPost("CreateCategory/{name}")]
  [Authorize(AuthenticationSchemes = "MLToken")]
  public Category CreateCategory(string name)
  {
   Init();
   return cm.CreateCategory(name);
  }

  /// <summary>
  /// Erstellen einer Aufgabe, die im Body im JSON-Format zu übermitteln ist (inkl. Teilaufgaben)
  /// </summary>
  
  /// <param name="t"></param>
  /// <returns></returns>
  [HttpPost("CreateTask")] // neu
  [Authorize(AuthenticationSchemes = "MLToken")]
  public Task CreateTask([FromBody]Task t)
  {
   Init();
   return tm.CreateTask(t);
  }

  /// <summary>
  /// Ändern einer Aufgabe, die im Body im JSON-Format zu übermitteln ist (inkl. Teilaufgaben)
  /// </summary>
  
  /// <param name="t"></param>
  /// <returns></returns>
  [HttpPut("ChangeTask")] // geändert
  [Authorize(AuthenticationSchemes = "MLToken")]
  public Task ChangeTask([FromBody]Task t)
  {
   Init();
   return tm.ChangeTask(t);
  }

  /// <summary>
  /// Setzt eine Aufgabe auf "erledigt"
  /// </summary>
  
  /// <param name="id"></param>
  /// <param name="done"></param>
  /// <returns></returns>
  [HttpPut("ChangeTaskDone")]
  [Authorize(AuthenticationSchemes = "MLToken")]
  public Task ChangeTaskDone(int id, bool done)
  {
   throw new UnauthorizedAccessException("du kommst hier nicht rein!");
  }

  /// <summary>
  /// Ändern einer Unteraufgabe
  /// </summary>
  
  /// <param name="st"></param>
  /// <returns></returns>
  [HttpPut("ChangeSubTask")]
  [Authorize(AuthenticationSchemes = "MLToken")]
  public SubTask ChangeSubTask([FromBody]SubTask st)
  {
   throw new UnauthorizedAccessException("du kommst hier nicht rein!");
  }

  /// <summary>
  /// Löschen einer Aufgabe mit allen Teilaufgaben
  /// </summary>
  
  /// <param name="id"></param>
  [HttpDelete("DeleteTask/{id}")]
  [Authorize(AuthenticationSchemes = "MLToken")]
  public void DeleteTask(int id)
  {
  Init();
   tm.Remove(id);
  }

  /// <summary>
  /// Löschen einer Kategorie mit allen Aufgaben und Teilaufgaben
  /// </summary>
  
  /// <param name="id"></param>
  [HttpDelete("[action]/{id}")]
  [Authorize(AuthenticationSchemes = "MLToken")]
  public void DeleteCategory(int id)
  {
  Init();
   cm.Remove(id);
  }
 }
}
