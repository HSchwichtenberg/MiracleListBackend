using System;
using System.Collections.Generic;
using BO;
using DAL;
using ITVisions.EFC;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace BL
{
 public class CategoryManager : EntityManagerBase<Context, Category>
 {
  private int userID;

  /// <summary>
  ///   Instantiation, specifying the user ID to which all operations in this instance refer
  /// </summary>
  public CategoryManager(int userID)
  {
   this.userID = userID;
  }

  /// <summary>
  /// Create a category
  /// </summary>
  /// <param name="name">Name of the new category</param>
  /// <returns></returns>
  public Category CreateCategory(string name)
  {
   var c = new Category();
   c.Name = name;
   c.Created = DateTime.Now;
   c.UserID = userID;
   this.New(c);
   return c;
  }

  /// <summary>
  /// Get the list of categories of a user including the tasks
  /// </summary>
  /// <returns></returns>
  public List<Category> GetCategorySet()
  {
   return ctx.CategorySet.Include(x=>x.TaskSet).Where(x => x.UserID == userID).ToList();
  }

  /// <summary>
  /// Deletes all categories of a user
  /// </summary>
  public void RemoveAll()
  {
   foreach (var c in this.GetCategorySet())
   {
    this.Remove(c.CategoryID);
   }
  }
  
  public void RemoveCategory(int id)
  {
   ValidateCategory(id);
   this.Remove(id);
  }

  /// <summary>
  /// Checks if the catID exists and belongs to the current user
  /// </summary>
  /// <param name="taskID"></param>
  private void ValidateCategory(int catID)
  {
   var cat = this.GetByID(catID);
   if (cat == null) throw new UnauthorizedAccessException("Category nicht vorhanden!");
   if (cat.UserID != this.userID) throw new UnauthorizedAccessException("Category gehört nicht zu diesem User!");
  }

 }
}
