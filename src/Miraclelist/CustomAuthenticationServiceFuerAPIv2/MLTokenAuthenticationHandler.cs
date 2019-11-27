using BL;
using BO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiracleList.CustomAuthenticationService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{

 /// <summary>
 /// Erweiterung für die einfache Einbindung in Startup
 /// </summary>
 public static class AddMLTokenExtensions
 {
  public static AuthenticationBuilder AddMLToken(this AuthenticationBuilder builder)
  {
   return builder.AddScheme<MLTokenAuthenticationOptions, MLTokenAuthenticationHandler>("MLToken", "MLToken", null);
  }
 }
}

namespace MiracleList.CustomAuthenticationService
{

 public class MLTokenAuthenticationOptions : AuthenticationSchemeOptions
 {

 }


 /// <summary>
 /// Behandelt Header ML-Token
 /// </summary>
 public class MLTokenAuthenticationHandler : AuthenticationHandler<MLTokenAuthenticationOptions>
 {
  public MLTokenAuthenticationHandler(
   IOptionsMonitor<MLTokenAuthenticationOptions> options, 
   ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
  { }


#pragma warning disable CS1998 // Fail geht nicht async :-(
  protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
  {
   var token = this.Context.Request.Headers["ML-AuthToken"];
   if (String.IsNullOrEmpty(token))
   {
    new LogManager().Log(Event.TokenCheckError, Severity.Warning, "No Token", "", token);
    return AuthenticateResult.Fail("No Token!");
   }

   #region ---------- Token auswerten
   var um = new UserManager(token);
   var checkResult = um.IsValid();
   if (checkResult != UserManager.TokenValidationResult.Ok || um.CurrentUser == null)
   {
    new LogManager().Log(Event.TokenCheckError, Severity.Warning, checkResult.ToString(), this.Context.Request.Path, token, um.CurrentUser?.UserID);
    return AuthenticateResult.Fail(checkResult.ToString());
   }
   string userID = um.CurrentUser.UserID.ToString();
   um.InitDefaultTasks();
   #endregion

   #region ---------- Claims erstellen
   var identity = new ClaimsIdentity("MLToken");
   identity.AddClaim(new Claim(System.Security.Claims.ClaimTypes.Name, userID)); // Speichert die UserID im Standard-Claim "Name"
   // zusätzliche Claims:
   identity.AddClaim(new Claim("Token", token));
   identity.AddClaim(new Claim("AuthentifiziertAm", DateTime.Now.ToString()));
   identity.AddClaim(new Claim("AuthentifiziertVon", nameof(MLTokenAuthenticationHandler)));
   var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, "MLToken");
   #endregion

   new LogManager().Log(Event.TokenCheckOK, Severity.Information, null, this.Context.Request.Path, token, um?.CurrentUser?.UserID);
   return AuthenticateResult.Success(ticket);
  }
 }
}
