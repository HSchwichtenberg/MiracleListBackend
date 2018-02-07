using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using BL;
using BO;
using DAL;


// XUNIT: https://xunit.github.io/docs/getting-started-dotnet-core.html

namespace UnitTests
{

 public class TaskManagerTest
 {

  public TaskManagerTest()
  {
   var cs = Util.GetConnectionString();
   DAL.Context.ConnectionString = cs;
  }

  [Theory]
  [InlineData("test3")]
  [InlineData("test2")]
  [InlineData("test1")]
  public void GetTaskTest(string name)
  {
   var um = new UserManager(name, true);
      um.InitDefaultTasks();
   var tm = new TaskManager(um.CurrentUser.UserID);
   var cm = new CategoryManager(um.CurrentUser.UserID);
   var cset1 = cm.GetCategorySet();

   var c = cset1[1];
   var tset = tm.GetTaskSet(c.CategoryID);
   foreach (var t in tset)
   {
    var t2 = tm.GetTask(t.TaskID);
    Assert.Equal(t.TaskID, t2.TaskID);
    Assert.True(t2.SubTaskSet.Count > 1,"Fehlende Subtastsks bei Task: " + t2.TaskID.ToString());
   }
  }

  [Theory]
  [InlineData("test6")]
  [InlineData("test5")]
  [InlineData("test4")]
  public void ChangeTest(string name)
  {
   var um = new UserManager(name, true);
   var tm = new TaskManager(um.CurrentUser.UserID);
   um.InitDefaultTasks();
   var cm = new CategoryManager(um.CurrentUser.UserID);
   var cset1 = cm.GetCategorySet();

   var testwert = DateTime.Now;

   // Werte ändern
   foreach (var c in cset1)
   {
    Assert.Equal(um.CurrentUser.UserID, c.UserID);
    var tset = tm.GetTaskSet(c.CategoryID);


    foreach (var t in tset)
    {
     var st1 = new SubTask() { Title = testwert.ToString() };
     var st2 = new SubTask() { Title = testwert.ToString() };

     Assert.Equal(c.CategoryID, t.CategoryID);
     tm.ChangeTask(t.TaskID, testwert.ToString(), testwert.ToString(), testwert, t.Importance, true, t.Effort,
      new List<SubTask> { st1, st2 });
    }
   }

   // nun prüfen!
   var cset2 = cm.GetCategorySet();

   foreach (var c in cset2)
   {
    Assert.Equal(c.UserID, um.CurrentUser.UserID);
    var tset = tm.GetTaskSet(c.CategoryID);

    foreach (var t in tset)
    {
     Assert.Equal(c.CategoryID, t.CategoryID);
     Assert.Equal(testwert.ToString(), t.Title);
     Assert.Equal(testwert.ToString(), t.Note);
     Assert.True(t.Done);
     Assert.Equal(2, t.SubTaskSet.Count);
     Assert.All<SubTask>(t.SubTaskSet, st => Assert.Equal(testwert.ToString(), st.Title));
     Assert.All<SubTask>(t.SubTaskSet, st => Assert.Equal(t.TaskID, st.TaskID));
    }
   }
  }

  [Theory]
  [InlineData("ctest3")]
  [InlineData("ctest2")]
  [InlineData("ctest1")]
  public void ChangeTest2(string name)
  {
   var um = new UserManager(name, true);
   var tm = new TaskManager(um.CurrentUser.UserID);
   um.InitDefaultTasks();
   var cm = new CategoryManager(um.CurrentUser.UserID);
   var cset1 = cm.GetCategorySet();

   var testwert = DateTime.Now;

   // Werte ändern
   foreach (var c in cset1)
   {
    Assert.Equal(um.CurrentUser.UserID, c.UserID);
    var tset = tm.GetTaskSet(c.CategoryID);


    foreach (var t in tset)
    {
     t.Title = testwert.ToString(); 
     t.Note = testwert.ToString();
     t.Due = testwert;
     t.Done = true;
     t.SubTaskSet.ForEach(x => x.Title = testwert.ToString());
     tm.ChangeTask(t);
    }
   }

   // nun prüfen!
   var cset2 = cm.GetCategorySet();

   foreach (var c in cset2)
   {
    Assert.Equal(c.UserID, um.CurrentUser.UserID);
    var tset = tm.GetTaskSet(c.CategoryID);

    foreach (var t in tset)
    {
     Assert.Equal(c.CategoryID, t.CategoryID);
     Assert.Equal(testwert.ToString(), t.Title);
     Assert.Equal(testwert.ToString(), t.Note);
     Assert.True(t.Done);

     Assert.All<SubTask>(t.SubTaskSet, st => Assert.Equal(testwert.ToString(), st.Title));
     Assert.All<SubTask>(t.SubTaskSet, st => Assert.Equal(t.TaskID, st.TaskID));
    }
   }
  }
 }
}
