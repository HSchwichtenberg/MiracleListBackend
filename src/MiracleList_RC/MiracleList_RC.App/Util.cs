using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;
using ITVisions;

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
 }
}


