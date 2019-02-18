using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using MiracleList_RC.App.Services;

namespace MiracleList_RC.App
{
 public class Startup
 {
  public void ConfigureServices(IServiceCollection services)
  {
   // Example of a data service
   services.AddSingleton<WeatherForecastService>();
   services.AddScoped<BL.CategoryManager>();
  }

  public void Configure(IComponentsApplicationBuilder app)
  {
   app.AddComponent<App>("app");
  }
 }
}
