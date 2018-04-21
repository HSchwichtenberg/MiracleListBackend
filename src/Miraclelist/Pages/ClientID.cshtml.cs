using System;
using System.Collections.Generic;
using BL;
using BO;
using ITVisions.AspNetCore; // Erweiterungsmethoden einbinden
using ITVisions.NetworkUtil;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Miraclelist_WebAPI.Pages
{

 public class ClientIDModelResult
 {
  public string Name { get; set; }
  public string EMail { get; set; }
 }

 public class ClientIDModel : PageModel
 {

  #region Einfache Properties für Ein-Wege-Bindung
  // KEIN [BindProperty] für statische/read-only Daten, die der Browser nicht zurücksendet
  public bool DownloadAnbieten
  {
   get
   {
    var webRoot = env.WebRootPath;
    var path = System.IO.Path.Combine(webRoot, "Download/Flyer.pdf");
    return System.IO.File.Exists(path);
   }
  }
  public int Aufrufe { get; set; }
  public List<SelectListItem> ClientArten { get; set; } = (new List<String>() { "Web-Client", "Desktop-Client", "Mobile Client", "Server-Anwendung" }).ToSelectListItem();
  #endregion

  #region Properties für Zwei-Wege-Bindung
  [BindProperty]
  public string Name { get; set; }
  [BindProperty]
  public string Firma { get; set; }
  [BindProperty]
  public string EMail { get; set; }
  [BindProperty]
  public bool Einverstanden { get; set; }
  [BindProperty]
  public string ClientArt { get; set; }
  #endregion

  #region Properties für Datenübergabe an Folgeseite
  //[TempData]// kann nicht gleichzeitig [BindProperty] sein
  // System.InvalidOperationException: The 'Miraclelist_WebAPI.Pages.ClientIDModel.ClientIDModelResult' property with TempDataAttribute is invalid. A property using TempDataAttribute must be of primitive or string type.
  //public ClientIDModelResult ClientIDModelResult { get; set; }

  [TempData]
  public string ClientIDModel_EMail { get; set; }
  [TempData]
  public string ClientIDModel_Name { get; set; }
  [TempData]
  public string ClientIDModel_Result { get; set; } // Name and EMail will be serialized here
  #endregion

  //public ClientIDModel()
  //{
  //// alternativ: ClientArten = (new List<String>() { "Web-Client", "Desktop-Client", "Mobile Client", "Server-Anwendung" }).ToSelectListItem();
  //}
  //public async void OnGetAsync()
  //{
  //}

  private IHostingEnvironment env; // injected via DI
  public ClientIDModel(IHostingEnvironment env)
  {
   this.env = env;
  }


  public void OnGet()
  {
   // ViewBag not available in Razor Pages! ViewBag.ClientArten = ClientArten;

   // Counter via Session
   int aufrufe = 0;
   aufrufe = HttpContext.Session.GetInt32("aufrufe") ?? 0;
   HttpContext.Session.SetInt32("aufrufe", ++aufrufe);
   this.Aufrufe = aufrufe;

   // if the user was here before, show his data (he might register multiple clients)
   var client = HttpContext.Session.GetObject<Client>("Client");
   if (client != null)
   {
    this.EMail = client.EMail;
    this.Name = client.Name;
    this.Firma = client.Company;
   }
   else
   {
    if (env.IsDevelopment())
    {
     this.EMail = "test@abc.de";
     this.Name = "Test";
     this.Firma = "Test";
    }
   }
  }

  /// <summary>
  /// Handler für Download-Schaltfläche
  /// </summary>
  public IActionResult OnPostDownload()
  {
   var webRoot = env.WebRootPath;
   var path = System.IO.Path.Combine(webRoot, "Download/Flyer.pdf");
   return PhysicalFile(path, "application/pdf");
  }

  /// <summary>
  /// Handler für Beantragen-Schaltfläche
  /// </summary>
  public IActionResult OnPostBeantragen()
  {
   #region Validierung
   if (string.IsNullOrEmpty(Name)) this.ModelState.AddModelError(nameof(Name), "Name darf nicht leer sein!");
   if (string.IsNullOrEmpty(Firma)) this.ModelState.AddModelError(nameof(Firma), "Firma darf nicht leer sein!");
   if (string.IsNullOrEmpty(EMail)) this.ModelState.AddModelError(nameof(EMail), "EMail darf nicht leer sein!");
   if (string.IsNullOrEmpty(ClientArt)) this.ModelState.AddModelError(nameof(EMail), "ClientArt darf nicht leer sein!");
   if (this.Einverstanden != true) this.ModelState.AddModelError(nameof(Einverstanden), "Sie müssen einverstanden sein!");

   if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(EMail)) this.ModelState.AddModelError(nameof(EMail), "EMail ungültig!");
   if (MailUtil.IsWegwerfadresse(EMail).Result) this.ModelState.AddModelError(nameof(EMail), "E-Mail-Domain nicht erlaubt!");

   if (!this.ModelState.IsValid)
   {
    return Page();
   }
   #endregion

   #region Logik
   // Client via Geschäftslogik registrieren und E-Mail senden

   var c = new Client();
   c.Name = Name;
   c.Company = Firma;
   c.EMail = EMail;
   c.Created = DateTime.Now;
   c.ClientID = Guid.NewGuid();
   c.Type = HttpContext.Request.Form["C_Quelle"]; ;

   HttpContext.Session.SetObject("Client", c);

   string s = this.Request.HttpContext.Connection.RemoteIpAddress + "\n";
   foreach (var v in this.Request.Headers)
   {
    s += v.Key + ":" + v.Value + "\n";
   }

   c.Memo = s;
   var cm = new ClientManager();

   cm.New(c);

   var text =
    $"Sie erhalten nachstehend Ihre personalisierte Client-ID. Bitte beachten Sie, dass eine Client-ID jederzeit widerrufen werden kann, wenn Sie diese missbrauchen! Bitte beachten Sie die Regeln: https://miraclelistbackend.azurewebsites.net/client\n\n" +
    $"Name: {c.Name}\n" +
    $"Firma: {c.Company}\n" +
    $"E-Mail: {c.EMail}\n" +
    (!String.IsNullOrEmpty(c.Type) ? $"Typ: {c.Type}\n" : "") +
    $"Client-ID: {c.ClientID}\n\n" +
    "Sie benötigen eine personalisierte Client-ID, wenn Sie selbst einen Beispiel-Client für das MiracleList-Backend schreiben wollen. Die Client-ID ist als Parameter bei der Login-Operation zu übergeben.\n\nDr. Holger Schwichtenberg, www.IT-Visions.de";

   var e1 = new ITVisions.NetworkUtil.MailUtil().SendMailTollerant("do-not-reply@mail.miraclelist.net", EMail, "Client-ID für MiracleList-Backend", text
     );

   new LogManager().Log(Event.ClientCreated, Severity.Information, EMail, "CreateClientID", "", null, this.Request.HttpContext.Connection.RemoteIpAddress.ToString(), text + "\n\n" + s);

   #endregion

   // Übergabewerte setzen
   // this.ClientIDModel_EMail = this.EMail;
   //this.ClientIDModel_Name = this.Name;

   // oder serialisieren:
   var result = new ClientIDModelResult() { Name = this.Name, EMail = this.EMail };
   this.ClientIDModel_Result = JsonConvert.SerializeObject(result);

   // Folgeseite aufrufen
   return RedirectToPage("./" + nameof(ClientIDConfirmationModel).Replace("Model",""));

  }


  public struct ClientIDModelResult
  {
   public string Name { get; set; }
   public string EMail { get; set; }
  }

 }
}