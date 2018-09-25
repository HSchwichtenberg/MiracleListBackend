﻿using BL;
using BO;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests
{
 public class LogManagerTest
 {



   public LogManagerTest()
   {
    Util.Init();
   }

   [Fact]

   public void Log()
   {
   var id = new LogManager().Log(Event.LoginOK, Severity.Information, "Test", "test", "none");
   Assert.True(id > 0);
  }
  [Fact]
  public void LogWithUser()
  {
   var um = new UserManager("logtestuser", true);
   var id = new LogManager().Log(Event.LoginOK, Severity.Information, "Test", "test", "none", um.CurrentUser.UserID);
   Assert.True(id > 0);
  }

 

 }
}

