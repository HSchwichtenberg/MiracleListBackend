using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BO
{

 /// <summary>
 /// Entity class representing a task
 /// Used on the server up to the WebAPI
 /// Corresponding proxy class in TypeScript is used on client
 /// </summary>
 public class Task
 {
  public const string DefaultTitle = "???";
  public int TaskID { get; set; } // PK per Konvention
  [MaxLength(250)] // alias: StringLength
  public string Title { get; set; }
  public DateTime Created { get; set; } = DateTime.Now;
  public DateTime? Due { get; set; }
  public Importance? Importance { get; set; }
  public string Note { get; set; }

  public bool Done { get; set; }
  public decimal? Effort { get; set; }
  public int Order { get; set; }
  public int DueInDays  { get; set; } // Computed Column

  // -------------- Navigation Properties
  public List<SubTask> SubTaskSet { get; set; } // 1:N
  [Newtonsoft.Json.JsonIgnore] // Do not serialize 
  public Category Category { get; set; }
  public int CategoryID { get; set; } // optional: FK Property
 }
}