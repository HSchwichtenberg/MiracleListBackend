using System;
using System.Collections.Generic;
using System.Linq;
using BO;
using DAL;
using ITVisions.EFC;

using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ITVisions.EFCore;

namespace BL
{
 /// <summary>
 /// Business Logic manager for Tasks entities
 /// </summary>
 public class TaskManager : EntityManagerBase<Context, Task>
 {
  // To manage the subtasks
  private SubTaskManager stm = new SubTaskManager();
  // Current user
  private int userID;

  /// <summary>
  /// Instantiation specifying the user ID to which all operations in this instance refer
  /// </summary>
  /// <param name="userID"></param>
  public TaskManager(int userID)
  {
   this.userID = userID;
  }


  /// <summary>
  /// Get a task list of one category for the current user
  /// </summary>
  public List<Task> GetTaskSet(int categoryID)
  {
   return ctx.TaskSet.Include(x => x.SubTaskSet).Where(x => x.Category.UserID == this.userID && x.CategoryID == categoryID).ToList();
  }


  /// <summary>
  /// Get a task including its subtasks
  /// </summary>
  public Task GetTask(int taskID)
  {
   var t = ctx.TaskSet.Include(x => x.SubTaskSet).Where(x => x.Category.UserID == this.userID && x.TaskID == taskID).SingleOrDefault();
   return t;
  }

  /// <summary>
  /// Create a new task from Task object
  /// </summary>
  public Task CreateTask(Task t)
  {
   ValidateTask(t);
   return this.New(t);
  }

  /// <summary>
  /// Create a new task from details
  /// </summary>
  public Task CreateTask(int categoryID, string title, string note, DateTime due, Importance importance, decimal? effort, List<SubTask> subtasks = null)
  {
   this.StartTracking();
   var t = new Task();
   t.CategoryID = categoryID;
   t.Created = DateTime.Now;
   SetTaskDetails(t, title, note, due, importance, false, effort, subtasks);
   this.New(t);
   this.SetTracking();
   return t;
  }

  private static void SetTaskDetails(Task t, string title, string note, DateTime? due, Importance? importance, bool done, decimal? effort, List<SubTask> subtasks)
  {
   t.Title = title;
   t.Note = note;
   t.Due = due;
   t.Importance = importance;
   t.SubTaskSet = subtasks;
   t.Effort = effort;
   t.Done = done;
  }

  /// <summary>
  /// Change a task
  /// </summary>
  public Task ChangeTask(int taskID, string title, string note, DateTime due, Importance? importance, bool done, decimal? effort, List<SubTask> subtasks)
  {
   ctx = new Context();
   //ctx.Log();
   // Delete subtasks and then create new ones instead of change detection!
   stm.DeleteSubTasks(taskID);

   var t = ctx.TaskSet.SingleOrDefault(x => x.TaskID == taskID);
   SetTaskDetails(t, title, note, due, importance, done, effort, null);
   ctx.SaveChanges();

   t.SubTaskSet = subtasks;
   ctx.SaveChanges();
   return t;
  }

  public void Log(string s)
  {
   Debug.WriteLine(s);
  }

  /// <summary>
  /// Change a task including subtasks
  /// </summary>
  public Task ChangeTask(Task tnew)
  {
   if (tnew == null) return null;

   // Validate of the sent data!
   if (tnew.Category != null) tnew.Category = null; // user cannot change the category this way!
   ValidateTask(tnew);

   var ctx1 = new Context();
   ctx1.Log(Log);
   stm.DeleteSubTasks(tnew.TaskID);

   if (tnew.SubTaskSet != null) tnew.SubTaskSet.ForEach(x => x.SubTaskID = 0); // delete ID, so that EFCore regards this as a new object

   tnew.CategoryID = this.GetByID(tnew.TaskID).CategoryID; // Use existing category

   ctx1.TaskSet.Update(tnew);

   var anz = ctx1.SaveChanges();
   return tnew;
  }

  /// <summary>
  /// Checks if the TaskID exists and belongs to the current user
  /// </summary>
  private void ValidateTask(int taskID)
  {
   if (taskID <= 0) return; // neuer Task
   var taskAusDB = ctx.TaskSet.Include(t => t.Category).SingleOrDefault(x => x.TaskID == taskID);
   if (taskAusDB == null) throw new UnauthorizedAccessException("Task does not exist!");
   if (taskAusDB.Category.UserID != this.userID) throw new UnauthorizedAccessException("Task does not belong to this user!");
  }

  /// <summary>
  /// Checks if transferred task object is valid
  /// </summary>
  private void ValidateTask(Task tnew = null)
  {
   ValidateTask(tnew.TaskID);
   if (tnew.CategoryID > 0)
   {
    var catAusDB = new CategoryManager(this.userID).GetByID(tnew.CategoryID);
    if (catAusDB.UserID != this.userID) throw new UnauthorizedAccessException("Task does not belong to this user!");
   }
  }

  /// <summary>
  /// Full-text search in tasks and subtasks, return tasks grouped by category
  /// </summary>
  public List<Category> Search(string text)
  {
   var r = new List<Category>();
   text = text.ToLower();
   var taskSet = ctx.TaskSet.Include(x => x.SubTaskSet).Include(x => x.Category).
    Where(x => x.Category.UserID == this.userID && // nur von diesem User !!!
    (x.Title.ToLower().Contains(text) || x.Note.ToLower().Contains(text) || x.SubTaskSet.Any(y => y.Title.Contains(text)))).ToList();

   foreach (var t in taskSet)
   {
    if (!r.Any(x => x.CategoryID == t.CategoryID)) r.Add(t.Category);
   }
   return r;
  }

  /// <summary>
  /// Returns all tasks due, including tomorrow, grouped by category, sorted by date
  /// </summary>
  public List<Category> GetDueTaskSet()
  {
   var tomorrow = DateTime.Now.Date.AddDays(1);
   var r = new List<Category>();
   var taskSet = ctx.TaskSet.Include(x => x.SubTaskSet).Include(x => x.Category).
    Where(x => x.Category.UserID == this.userID && // nur von diesem User !!!
    (x.Done == false && x.Due != null && x.Due.Value.Date <= tomorrow)).OrderByDescending(x => x.Due).ToList();

   foreach (var t in taskSet)
   {
    if (!r.Any(x => x.CategoryID == t.CategoryID)) r.Add(t.Category);
   }
   return r;
  }


  public List<Task> GetImportantTaskSet()
  {
   var r = ctx.TaskSet.FromSql("Select * from [task] where importance = 1").OrderByDescending(x => x.Created).Take(10).ToList();

   return r;

  }

  /// <summary>
  /// Remove Task with its subtasks
  /// </summary>
  public void RemoveTask(int id)
  {
   ValidateTask(id);
   this.Remove(id);
  }
 }
}
