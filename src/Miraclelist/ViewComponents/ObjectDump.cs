
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;



namespace ITVisions.Components
{
 [ViewComponent(Name = "ObjectDump")]
 public class ObjectDumpComponentController : ViewComponent
 {

  public ObjectDumpComponentController()
  {

  }

  public IViewComponentResult Invoke(object obj, bool? details = false)
  {

   var modell = obj.ToNameValueDictionary().Where(x => !x.Key.Contains("BackingField")).ToList();
    return View(modell);
  }
 }
}