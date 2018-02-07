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
  /// Instanzierung unter Angabe der User-ID, auf die sich alle Operationen in dieser Instanz beziehen
  /// </summary>
  public CategoryManager(int userID)
  {
   this.userID = userID;
  }

  /// <summary>
  /// Anlegen einer Kategorie
  /// </summary>
  /// <param name="name">Name der neuen Kategorie</param>
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
  /// Holt die Liste der Kategorien eines Benutzers inkl. der Aufgaben
  /// </summary>
  /// <returns></returns>
  public List<Category> GetCategorySet()
  {
   return ctx.CategorySet.Include(x=>x.TaskSet).Where(x => x.UserID == userID).ToList();
  }

  /// <summary>
  /// Löscht alle Kategorien eines Benutzers
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
  /// Prüft, ob die catID existiert und dem aktuellen Benutzer gehört
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
