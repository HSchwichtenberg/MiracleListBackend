using System;
using BL;
using Microsoft.AspNetCore.Mvc;

namespace ASPNET.Controllers
{
 public class POCOController
 {
  //   // Aufruf = Standardroute: http://localhost:12345/poco/index
  public string Index()
  {
   //return System.DateTime.Now + ": Hello World!";
   var daten = new AppManager().GetAppInfo();
   return string.Join("\n", daten);
  }


  //   // Aufruf = Standardroute: http://localhost:12345/poco/hallo?name=Holger
  //public ActionResult Hallo(string name)
  //{
  // return new ContentResult()
  // {
  //  ContentType = "text/html",
  //  Content = "<html><body><h2>Hallo " + name
  //  + "</h2></body></html>"
  // };
  //}

  //// http://localhost:12345/hallowelt2 und http://localhost:12345/hallo/welt/2
  //[Route("Datum")]
  //[Route("/Datum/Get")]
  //public ActionResult HTMLIndex2()
  //{
  // return new ContentResult()
  // {
  //  ContentType = "text/html",
  //  Content = "<html><body>" 
  //  + "<h2>Hallo Welt 3!</h2>"
  //  + DateTime.Now
  //  + "</body></html>"
  // };
  //}
 }
}
