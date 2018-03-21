using BO;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using ITVisions;
using System.Collections.Generic;
using System.Data.Common;

namespace DAL
{
 /// <summary>
 /// Context class for Entity Framework Core
 /// Forms the DAL that is used the BL manager classes
 /// </summary>
 public class Context : DbContext
 {
  // Register the entity classes in the context
  public DbSet<Client> ClientSet { get; set; }
  public DbSet<User> UserSet { get; set; }
  public DbSet<Task> TaskSet { get; set; }
  public DbSet<Category> CategorySet { get; set; }
  public DbSet<Log> LogSet { get; set; }


  #region Pseudo-entities for grouping results
  public DbSet<UserStatistics> UserStatistics { get; set; } // for grouping result
  #endregion


  // This connection string is just for testing. Is filled at runtime from configuration file
  public static string ConnectionString { get; set; } = "Data Source=.;Initial Catalog = MiracleList_TEST; Integrated Security = True; Connect Timeout = 15; Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;Application Name=EntityFramework";
  public static DbConnection Connection { get; set; } = null;

  // =  "Data Source=.,1434;Initial Catalog = MiracleList_INFOTAG; User Id=sa; password=xxx; Connect Timeout = 15; Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;Application Name=EntityFramework";
  public Context()
  {
   instanceCount++;
  }

  public Context(DbContextOptions<Context> options)         : base(options)
  {
   instanceCount++;
  }

  private static int instanceCount = 0;
  private static List<string> additionalColumnSet = null;
  public static List<string> AdditionalColumnSet
  {
   get { return additionalColumnSet; }
   set
   {
    if (instanceCount > 0) throw new ApplicationException("Cannot set AdditionalColumnSet as context has been used before!");
    additionalColumnSet = value;
   }
  }

  protected override void OnConfiguring(DbContextOptionsBuilder builder)
  {
   if (!builder.IsConfigured)
   {
    if (Connection != null)
    {
     builder.UseSqlite(Context.Connection);
 
    }
    else
    {
     if (!String.IsNullOrEmpty(Context.ConnectionString))
      builder.UseSqlServer(Context.ConnectionString);
     else
      builder.UseInMemoryDatabase("Miracle ListInMemoryDB");
    }
   }
  }


  public static bool IsRuntime { get; set; } = false;

  protected override void OnModelCreating(ModelBuilder builder)
  {

   #region Trick for pseudo entities for grouping and Views

   var p = System.Diagnostics.Process.GetCurrentProcess();
   //Console.WriteLine(p.ProcessName + "/" + System.Diagnostics.Debugger.IsAttached);


   if (!IsRuntime)
   {
    builder.Ignore<UserStatistics>();
   }


   #endregion

   // In this case, EFCore can derive the database schema from the entity classes by convention and annotation.
   // The following Fluent API configurations only change the default behavior!
   #region Shadow property
   if (AdditionalColumnSet != null)
   {
    foreach (string shadowProp in AdditionalColumnSet)
    {
     var splitted = shadowProp.Split(';');
     string entityclass = splitted[0];
     string columnname = splitted[1];
     string columntype = splitted[2];

     Type columntypeObj = Type.GetType(columntype);

     builder.Entity(entityclass).Property(columntypeObj, columnname);
    }
   }
   #endregion

   #region Mass configuration via model class
   foreach (IMutableEntityType entity in builder.Model.GetEntityTypes())
   {
    // all table names = class names (as with EF 6.x), 
    // except the classes that have [Table] annotation
    var annotation = entity.ClrType.GetCustomAttribute<TableAttribute>();
    if (annotation == null)
    {
     entity.Relational().TableName = entity.DisplayName();
    }
   }
   #endregion

   #region Computed Column
   builder.Entity<Task>().Property(x => x.DueInDays)
         .HasComputedColumnSql("DATEDIFF(day, GETDATE(), [Due])");

   builder.Entity<Task>().Property(x => x.Title).HasDefaultValue(BO.Task.DefaultTitle);
   #endregion

   #region Custom Indices
   builder.Entity<Category>().HasIndex(x => x.Name);
   builder.Entity<Task>().HasIndex(x => x.Title);
   builder.Entity<Task>().HasIndex(x => x.Done);
   builder.Entity<Task>().HasIndex(x => x.Due);
   builder.Entity<Task>().HasIndex(x => new { x.Title, x.Due });
   #endregion
  }
 }
}
