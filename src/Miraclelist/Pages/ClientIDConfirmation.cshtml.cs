using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Miraclelist_WebAPI.Pages
{
 public class ClientIDConfirmationModel : PageModel
 {

  [TempData]
  public string EMail { get; set; }
  [TempData]
  public string Name { get; set; }

  public void OnGet()
  {

  }
 }
}