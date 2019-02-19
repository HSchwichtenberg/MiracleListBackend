using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using ITVisions;
using System.Threading.Tasks;

namespace MiracleList_RC.App
{
 class Util
 {
  public static async void Log(object o)
  {
   string s;
   if (o is string) s = o.ToString();
   else s = o.ToNameValueString();

   await JSRuntime.Current.InvokeAsync<string>("log", "ML: " + s);
   //UriComponents.
   //this.StateHasChanged();
  }

  public static async Task<bool> Confirm(string text1, string text2 = "")
  {
   Log("Confirm");
   return await JSRuntime.Current.InvokeAsync<bool>("confirm", text1 + "\n" + text2);
  }
 }
}