using BO;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;

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



  // This connection string is just for testing. Is filled at runtime from configuration file
  public static string ConnectionString { get; set; } = "Data Source=D120;Initial Catalog = MiracleList_TEST; Integrated Security = True; Connect Timeout = 15; Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;Application Name=EntityFramework";

  // =  "Data Source=.,1434;Initial Catalog = MiracleList_INFOTAG; User Id=sa; password=demo+123; Connect Timeout = 15; Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;Application Name=EntityFramework";


  protected override void OnConfiguring(DbContextOptionsBuilder builder)
  {
   if (!String.IsNullOrEmpty(Context.ConnectionString))
    builder.UseSqlServer(Context.ConnectionString);
   else
    builder.UseInMemoryDatabase("MiracleListInMemoryDB");
  }

  protected override void OnModelCreating(ModelBuilder builder)
  {
   // In this case, EFCore can derive the database schema from the entity classes by convention and annotation.
   // The following Fluent API configurations only change the default behavior!

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
