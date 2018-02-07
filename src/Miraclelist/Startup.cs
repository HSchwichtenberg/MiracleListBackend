using BL;
using ITVisions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Miraclelist
{

 public class Startup
 {
  public IConfigurationRoot Configuration { get; }
 
  public Startup(IHostingEnvironment env)
  {
   CUI.Headline("Startup");

   //System.Environment.SetEnvironmentVariable("ConnectionStrings:MiracleListDB",))

   // Get all configuration sources
   var builder = new ConfigurationBuilder()
       .SetBasePath(env.ContentRootPath)
       .AddInMemoryCollection()
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
       .AddEnvironmentVariables(); // NUGET: Microsoft.Extensions.Configuration.EnvironmentVariables

 

   if (env.IsEnvironment("Development"))
   {
    // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
    builder.AddApplicationInsightsSettings(developerMode: true);
    // Connect to EFCore Profiler
    HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.Initialize();
   }
   else
   {

   }

   builder.AddEnvironmentVariables();
   Configuration = builder.Build();

   var CS = Configuration["ConnectionStrings:MiracleListDB"];
   Console.WriteLine("ConnectionString=" + CS);

   // inject connection string into DAL
   DAL.Context.ConnectionString = CS;


   try
   {
    #region testuser
    if (env.IsEnvironment("Development"))
    {
     var um = new UserManager("HS", "HS");
     um.InitDefaultTasks();

     var um2 = new UserManager("unittest", "unittest");
     um2.InitDefaultTasks();
    }
    #endregion
   }
   catch (Exception)
   {

   }

  }



  /// <summary>
  /// Called by ASP.NET Core during startup
  /// </summary>
  public void ConfigureServices(IServiceCollection services)
  {
   CUI.Headline("ConfigureServices");

   #region Enable Auth service for MLToken in the HTTP header
   services.AddAuthentication().AddMLToken();
   #endregion

   #region Enable App Insights
   services.AddApplicationInsightsTelemetry(Configuration);
   #endregion

   #region JSON configuration: no circular references and ISO date format
   services.AddMvc().AddJsonOptions(options =>
   {
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;
    options.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat;
   });
   #endregion

   #region Enable MVC
   services.AddMvc(options =>
   {
    // Exception Filter
    options.Filters.Add(typeof(GlobalExceptionFilter));
    //options.Filters.Add(typeof(GlobalExceptionAsyncFilter)); 
    options.Filters.Add(typeof(LoggingActionFilter));
   });
   #endregion

   #region Enable CORS 
   services.AddCors();
   #endregion

   // Make configuration available everywhere
   services.AddSingleton(Configuration);

   #region Swagger
   services.AddSwaggerGen(c =>
   {
    c.DescribeAllEnumsAsStrings(); // Important for Enums!

    c.SwaggerDoc("v1", new Info
    {
     Version = "v1",
     Title = "MiracleList API",
     Description = "Backend for MiracleList.de with token in URL",
     TermsOfService = "None",
     Contact = new Contact { Name = "Holger Schwichtenberg", Email = "", Url = "http://it-visions.de/kontakt" }
    });

    c.SwaggerDoc("v2", new Info
    {
     Version = "v2",
     Title = "MiracleList API",
     Description = "Backend für MiracleList.de with token in HTTP header",
     TermsOfService = "None",
     Contact = new Contact { Name = "Holger Schwichtenberg", Email = "", Url = "http://it-visions.de/kontakt" }
    });

    // Adds tokens as header parameters
    c.OperationFilter<SwaggerTokenHeaderParameter>();

    // include XML comments in Swagger doc
    var basePath = PlatformServices.Default.Application.ApplicationBasePath;
    var xmlPath = Path.Combine(basePath, "Miraclelist_WebAPI.xml");
    c.IncludeXmlComments(xmlPath);
   });
   #endregion
  }

  /// <summary>
  /// Called by ASP.NET Core during startup
  /// </summary>
  public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
  {

   #region Error handling

   app.UseExceptionHandler(errorApp =>
   {
    errorApp.Run(async context =>
    {
     context.Response.StatusCode = 500;
     context.Response.ContentType = "text/plain";

     var error = context.Features.Get<IExceptionHandlerFeature>();
     if (error != null)
     {
      var ex = error.Error;
      await context.Response.WriteAsync("ASP.NET Core Exception Middleware:" + ex.ToString());
     }
    });
   });

   // ---------------------------- letzte Fehlerbehandlung: Fehlerseite für HTTP-Statuscode
   app.UseStatusCodePages();

   #endregion

   #region ASP.NET Core services
   app.UseDefaultFiles();
   app.UseStaticFiles();
   app.UseDirectoryBrowser();
   loggerFactory.AddConsole(Configuration.GetSection("Logging"));
   loggerFactory.AddDebug();
   #endregion

   #region CORS
   // NUGET: install-Package Microsoft.AspNet.Cors
   // Namespace: using Microsoft.AspNet.Cors;
   app.UseCors(builder =>
    builder.AllowAnyOrigin()
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials()
    );
   #endregion

   #region Swagger
   // NUGET: Install-Package Swashbuckle.AspNetCore
   // Namespace: using Swashbuckle.AspNetCore.Swagger;
   app.UseSwagger(c =>
   {
   });

   // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
   app.UseSwaggerUI(c =>
   {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiracleList v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "MiracleList v2");
   });
   #endregion

   #region  MVC with Routing
   app.UseMvc(routes =>
  {
   routes.MapRoute(
                name: "default",
                template: "{controller}/{action}/{id?}",
                defaults: new { controller = "Home", action = "Index" });

   // iX tutorial 2017
   routes.MapRoute(
              name: "iX",
              template: "iX",
              defaults: new { controller = "Client", action = "Index" });


   // Schulungsteilnehmer ab Jan 2017
   routes.MapRoute(
             name: "Schulung",
             template: "Schulung",
             defaults: new { controller = "Client", action = "Index" });
  });
   #endregion

  }
 }

 public class GlobalExceptionFilter : IExceptionFilter
 {
  public void OnException(ExceptionContext context)
  {
   if (context.Exception is UnauthorizedAccessException)
   {
    context.HttpContext.Response.StatusCode = 403;
   }
   else
   {
    context.HttpContext.Response.StatusCode = 500;
   }
   context.HttpContext.Response.ContentType = "text/plain";

   var s = "MiracleListBackend v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
   s += " on ASP.NET Core v" + typeof(WebHost).Assembly.GetName().Version.ToString() + " GlobalExceptionFilter: ";

   context.HttpContext.Response.WriteAsync(s + context.Exception.ToString());
  }
 }

 public class GlobalExceptionAsyncFilter : IAsyncExceptionFilter
 {
  public Task OnExceptionAsync(ExceptionContext context)
  {
   context.HttpContext.Response.StatusCode = 500;
   context.HttpContext.Response.ContentType = "text/plain";
   return context.HttpContext.Response.WriteAsync("MVC GlobalExceptionAsyncFilter:" + context.Exception.ToString());
  }
 }
}