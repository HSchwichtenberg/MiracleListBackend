using System;
using System.ComponentModel.DataAnnotations;

namespace BO
{
 /// <summary>
 /// Entity class representing a subtask
 /// Used on the server up to the WebAPI
 /// Corresponding proxy class in TypeScript is used on client
 /// </summary>
 public class SubTask
 {
  public int SubTaskID { get; set; } // PK
  [MaxLength(250)]
  public string Title { get; set; }
  public bool Done { get; set; }
  public DateTime Created { get; set; } = DateTime.Now;
  // -------------- Navigation Properties
  public Task Task { get; set; } 
  public int TaskID { get; set; } 
 }
}