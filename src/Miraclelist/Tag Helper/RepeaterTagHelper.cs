using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ITVisions
{

 /// <summary>
 /// x-mal Wiederholung des Kind-Inhaltes
 /// </summary>
 public class RepeaterTagHelper : TagHelper
 {
  public int Count { get; set; }

  public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
  {
   output.TagName = null;
   for (int i = 0; i <= Count; i++)
   {
    var e = await output.GetChildContentAsync(useCachedResult: false);
    output.Content.AppendHtml(e.GetContent());
   }
  }
 }
}
