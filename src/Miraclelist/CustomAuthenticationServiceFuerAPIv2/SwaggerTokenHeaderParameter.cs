using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace MiracleList
{
 public class SwaggerTokenHeaderParameter : IOperationFilter
 {
  public void Apply(Operation operation, OperationFilterContext context)
  {
   bool brauchtToken = false;
   var controllerAttributes = context.ApiDescription.ControllerAttributes().ToList();
   var actionAttributes = context.ApiDescription.ActionAttributes().ToList();

   // prüfen, ob der Controller ein [Autorize] besitzt
   if (controllerAttributes.Any(x=>x.GetType() == typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute))) {
    brauchtToken = true;
   }

   // prüfen, ob die Operation ein [Autorize] besitzt
   if (actionAttributes.Any(x => x.GetType() == typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute)))
   {
    brauchtToken = true;
   }

   if (!brauchtToken) return; // nichts tun, wenn kein [Autorize]

   if (operation.Parameters == null)
    operation.Parameters = new List<IParameter>();

   operation.Parameters.Add(new HeaderParameter()
   {
    Name = "ML-AuthToken",
    In = "header",
    Type = "string",
    Required = false
   });

  }
 }

 class HeaderParameter : NonBodyParameter
 {
 }
}
