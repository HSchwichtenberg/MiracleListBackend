using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ITVisions
{
 [HtmlTargetElement(Attributes = "fett")]
 public class FettTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
 {
  public override void Process(TagHelperContext context, TagHelperOutput output)
  {
   output.Attributes.RemoveAll("bold");
   output.PreContent.SetHtmlContent("<strong>");
   output.PostContent.SetHtmlContent("</strong>");

  }
 }


 [HtmlTargetElement(Attributes = "kursiv")]
 public class Italic : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
 {
  public override void Process(TagHelperContext context, TagHelperOutput output)
  {
   output.Attributes.RemoveAll("bold");
   output.PreContent.SetHtmlContent("<i>");
   output.PostContent.SetHtmlContent("</i>");

  }
 }
}
