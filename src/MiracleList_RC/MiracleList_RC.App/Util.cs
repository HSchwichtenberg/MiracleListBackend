using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace MiracleList_RC.App
{
 class Util
 {

  public static async void Log(string s)
  {
   await JSRuntime.Current.InvokeAsync<string>("log", "ML: " + s);
   //UriComponents.
   //this.StateHasChanged();
  }
 }
}
