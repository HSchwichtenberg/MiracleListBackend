using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Miraclelist_WebAPI.Pages
{
 public class Info
 {
  public string Name { get; set; }
  public string EMail { get; set; }
 }
 public class ClientIDConfirmationModel : PageModel
 {

  
  [TempData]
  public string EMailErfasst { get; set; }
  [TempData]
  public string NameErfasst { get; set; }
  [BindProperty]
  public Info Info { get; set; }
  public void OnGet()
  {
   //(string Name, string EMail) info = (this.Name, this.EMail);
   this.Info = new Info { Name = this.NameErfasst, EMail = this.EMailErfasst };
  }
 }
}