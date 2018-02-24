﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BL;
using BO;
using ITVisions.AspNetCore;
using ITVisions.Extensions;
using ITVisions.NetworkUtil;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Miraclelist_WebAPI.Pages
{
 public class ClientIDModel : PageModel
 {

  //[TempData]
  [BindProperty]
  public string Name { get; set; }
  [TempData]// kann nicht gleichzeitig [BindProperty] sein
  public string NameErfasst { get; set; }
  [BindProperty]
  public string Firma { get; set; }
  //[TempData]
  [BindProperty]
  public string EMail { get; set; }
  [TempData]// kann nicht gleichzeitig [BindProperty] sein
  public string EMailErfasst { get; set; }
  [BindProperty]
  public bool Einverstanden { get; set; }
  [TempData]
  public string Status { get; set; }
  [BindProperty]
  public int Aufrufe { get; set; }
  [BindProperty]
  public string ClientArt { get; set; }
  // KEIN [BindProperty] für statische/read-only Daten, die der Browser nicht zurücksendet
  public List<SelectListItem> ClientArten { get; set; } = (new List<String>() { "Web-Client", "Desktop-Client", "Mobile Client", "Server-Anwendung" }).ToSelectListItem();

  public ClientIDModel()
  {
  // alternativ: ClientArten = (new List<String>() { "Web-Client", "Desktop-Client", "Mobile Client", "Server-Anwendung" }).ToSelectListItem();
  }

  public void OnGet()
  {

   // ViewBag gibt es hier nicht!!!  ViewBag.ClientArten = ClientArten;

   // War der Benutzer schon mal hier? (will er mehrere Clients registrieren? Dann zeige seine vorherigen Daten an)
  var client = HttpContext.Session.GetObject<Client>("Client");
   if (client != null)
   {
    this.EMail = client.EMail;
    this.Name = client.Name;
    this.Firma = client.Company;
   }
   else
   {
    this.EMail = "test@it-visions.de";
    this.Name = "Test";
    this.Firma = "Test";
   }

   int aufrufe = 0;
   aufrufe = HttpContext.Session.GetInt32("aufrufe") ?? 0;
   HttpContext.Session.SetInt32("aufrufe", ++aufrufe);
  
   this.Aufrufe = aufrufe;

  }

   public IActionResult OnPostBeantragen()
  {

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
    "Sie benötigen eine personalisierte Client-ID, wenn Sie selbst einen Beispiel-Client für das MiracleList-Backend schreiben wollen. Die Client-ID ist bei als Parameter der Login-Operation zu übergeben.\n\nDr. Holger Schwichtenberg, www.IT-Visions.de";

   var e1 = new ITVisions.NetworkUtil.MailUtil().SendMailTollerant("do-not-reply@mail.miraclelist.net", EMail, "Client-ID für MiracleList-Backend", text
     );

   new LogManager().Log(Event.ClientCreated, Severity.Information, EMail, "CreateClientID", "", null, this.Request.HttpContext.Connection.RemoteIpAddress.ToString(), text + "\n\n" + s);

   var e2 = new ITVisions.NetworkUtil.MailUtil().SendMailTollerant("system@mail.miraclelist.net", "hs_status@IT-Visions.de", "MiracleList Client_ID", text + "\n\n-----\n" + s);
   Status = e1.ToString();

   this.NameErfasst = this.Name;
   this.EMailErfasst = this.EMail;

   return RedirectToPage("./ClientIDConfirmation");

  }
 }
}