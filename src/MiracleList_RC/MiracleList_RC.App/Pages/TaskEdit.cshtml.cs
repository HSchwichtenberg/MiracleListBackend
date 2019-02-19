using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MiracleList_RC.App.Pages
{
 public class TaskEditModel : ComponentBase
 {

  [Parameter]
  protected int TaskID { get; set; }

  [Parameter]
  protected Action<int> TaskIDChanged { get; set; }

  [Parameter]
  protected Action<int> TaskHasChanged { get; set; }

  public BO.Task task;
  public int userID = 1; // Demo ohne Login!

  protected override void OnInit()
  {
   Util.Log(nameof(OnInit) + ": " + TaskID);
   //GetTask(TaskID);
  }

  // wenn Parameter gesetzt wird
  protected override void OnParametersSet()
  {
   GetTask(TaskID);
  }

  protected void Save()
  {
   Util.Log(nameof(Save) + ": " + TaskID);
   Util.Log(this.task);
   new BL.TaskManager(userID).ChangeTask(this.task);
   TaskHasChanged?.Invoke(TaskID);
   //this.StateHasChanged();
  }

  protected void Cancel()
  {
   Util.Log(nameof(Cancel) + ": " + TaskID);
   GetTask(TaskID);
   TaskHasChanged?.Invoke(TaskID);
  }
  private void GetTask(int id)
  {
   Util.Log(nameof(GetTask) + ": " + id);
   this.task = new BL.TaskManager(userID).GetTask(id);
   Util.Log(this.task);
  }

 }
}