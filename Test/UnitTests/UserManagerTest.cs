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
 public class UserManagerTest
 {

  public UserManagerTest()
  {
   var cs = Util.GetConnectionString();
   DAL.Context.ConnectionString = cs;
  }


  [Fact]
  public void NewUserTest()
  {
   var name = Guid.NewGuid().ToString(); // GUID als Token und dann auch User Name
   var um = new UserManager(name, true);

   um.InitDefaultTasks();
   var cm = new CategoryManager(um.CurrentUser.UserID);
   var cset = cm.GetCategorySet();
   Assert.True(cset.Count == 4);
   Assert.All<Category>(cset, x => Assert.Equal(x.UserID, um.CurrentUser.UserID));
  }

  [Fact]
  public void LoginTest()
  {
   var kennwort = "unittest";
   var name = "unittest"; // GUID als User Name
   var um = new UserManager(name, kennwort);

   um.InitDefaultTasks();

   for (int i = 0; i < 5; i++)
   {
    var um2 = new UserManager(name, kennwort);
    Assert.Equal(um2.CurrentUser.UserName, name);
    var cm = new CategoryManager(um2.CurrentUser.UserID);
    var cset = cm.GetCategorySet();
    Assert.True(cset.Count == 1);
    Assert.All<Category>(cset, x => Assert.Equal(x.UserID, um.CurrentUser.UserID));
   }

  }

  [Theory]
  [InlineData("test3")]
  [InlineData("test2")]
  [InlineData("test1")]
  public void ExtistingUserTest(string name)
  {
   var um = new UserManager(name, true);
   var cm = new CategoryManager(um.CurrentUser.UserID);
   cm.RemoveAll();

   um.InitDefaultTasks();

   var cset = cm.GetCategorySet();
   Assert.True(cset.Count >= 3);
   Assert.All<Category>(cset, x => Assert.Equal(x.UserID, um.CurrentUser.UserID));
  }


 }
}
