using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Models.Services.Interop;
using Disco.Services.Users;
using Microsoft.AspNet.SignalR;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;

namespace Disco.Web.App_Start
{
    public static class AuthenticationConfig
    {
        private const string testOicSessionCookieName = "DiscoIctAuthTestSession";
        private static Guid? testOidcSession;
        private static bool? testOidcSuccess;
        private static DateTime? testOidcExpiration;
        private static string testOidcUserPrincipalName = null;
        private static readonly SsoConfiguration testSsoConfiguration = new SsoConfiguration();
        private static readonly OpenIdConnectAuthenticationOptions testOidcOptions = new OpenIdConnectAuthenticationOptions()
        {
            ClientId = " ",
            Authority = "https://login.microsoftonline.com/",
            TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
            {
                NameClaimType = "preferred_username",
            },
            Notifications = new OpenIdConnectAuthenticationNotifications()
            {
                MessageReceived = context =>
                {
                    if (testOidcExpiration.GetValueOrDefault(DateTime.MinValue) < DateTime.UtcNow)
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Config/SystemConfig/SSO?error=" + Uri.EscapeDataString("SSO Testing has expired. Please re-enter the configuration to test."));
                    }

                    if (!string.Equals(testOidcSession.Value.ToString("N"), context.Request.Cookies[testOicSessionCookieName], StringComparison.Ordinal))
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Config/SystemConfig/SSO?error=" + Uri.EscapeDataString($"The SSO configuration was changed during testing. Please re-enter the configuration to test."));
                        return Task.CompletedTask;
                    }

                    return Task.CompletedTask;
                },
                SecurityTokenValidated = context =>
                {
                    var identity = context.AuthenticationTicket.Identity;

                    var nameClaim = identity.FindFirst(context.AuthenticationTicket.Identity.NameClaimType);
                    if (nameClaim == null)
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Config/SystemConfig/SSO?error=" + Uri.EscapeDataString($"The identity did not contain the required name claim: '{context.AuthenticationTicket.Identity.NameClaimType}'"));
                        return Task.CompletedTask;
                    }

                    if (!string.Equals(nameClaim.Value, testOidcUserPrincipalName, StringComparison.OrdinalIgnoreCase))
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/Config/SystemConfig/SSO?error=" + Uri.EscapeDataString($"The login name did not match the expected value. Expected: '{testOidcUserPrincipalName}', Actual: '{nameClaim.Value}'."));
                        return Task.CompletedTask;
                    }

                    identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", @"DiscoIctSso"));

                    lock (testOidcOptions)
                    {
                        if (!string.Equals(testOidcSession.Value.ToString("N"), context.Request.Cookies[testOicSessionCookieName], StringComparison.Ordinal))
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/Config/SystemConfig/SSO?error=" + Uri.EscapeDataString($"The SSO configuration was changed during testing. Please re-enter the configuration to test."));
                            return Task.CompletedTask;
                        }

                        testOidcSuccess = true;
                        context.HandleResponse();
                        context.Response.Redirect($"/Config/SystemConfig/SSO?Session={testOidcSession.Value:N}");
                    }

                    return Task.CompletedTask;
                },
                AuthenticationFailed = context =>
                {
                    context.HandleResponse();
                    context.Response.Redirect("/Config/SystemConfig/SSO?error=" + Uri.EscapeDataString(context.Exception.Message));
                    return Task.CompletedTask;
                }
            },
            CallbackPath = new PathString("/API/Authentication/OIDC/SignIn"),
        };

        public static void SetTestEntraIdOpenIdConnectOptions(string tenantId, string clientId, User user)
        {
            lock (testOidcOptions)
            {
                testOidcSession = Guid.NewGuid();
                testOidcSuccess = null;

                testSsoConfiguration.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
                testSsoConfiguration.ClientId = clientId;
                testSsoConfiguration.UpnClaimName = "preferred_username";

                testOidcOptions.Authority = testSsoConfiguration.Authority;
                testOidcOptions.ClientId = testSsoConfiguration.ClientId;
                testOidcOptions.TokenValidationParameters.ValidAudience = testSsoConfiguration.ClientId;
                testOidcOptions.MetadataAddress = testSsoConfiguration.Authority + "/.well-known/openid-configuration";
                testOidcOptions.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(testOidcOptions.MetadataAddress, new OpenIdConnectConfigurationRetriever(),
                            new HttpDocumentRetriever(testOidcOptions.Backchannel) { RequireHttps = testOidcOptions.RequireHttpsMetadata });
                testOidcExpiration = DateTime.UtcNow.AddMinutes(15);
                testOidcUserPrincipalName = user.UserPrincipalName;
                var sessionCookie = new HttpCookie(testOicSessionCookieName, testOidcSession.Value.ToString("N"))
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTime.MinValue,
                };
                HttpContext.Current.Response.Cookies.Set(sessionCookie);
            }
        }

        public static void EnableOpenIdConnect(string sessionId, DiscoDataContext database)
        {
            lock (testOidcOptions)
            {
                if (database.DiscoConfiguration.SsoAdministrativelyDisabled)
                    throw new InvalidOperationException("SSO is administratively disabled (see web.config) and cannot be enabled until an administrator removes the block.");
                if (!testOidcSession.HasValue || !string.Equals(testOidcSession.Value.ToString("N"), sessionId, StringComparison.Ordinal))
                    throw new InvalidOperationException("The SSO test configuration changed, please try again.");
                if (!testOidcSuccess.GetValueOrDefault())
                    throw new InvalidOperationException("The OpenID Connect test authentication process did not complete.");

                database.DiscoConfiguration.SsoConfiguration = testSsoConfiguration;
                database.DiscoConfiguration.SsoEnabled = true;
                database.SaveChanges();

                testOidcSession = null;
                testOidcSuccess = null;
                testOidcExpiration = null;
                testOidcUserPrincipalName = null;
            }
        }

        public static void DisableOpenIdConnect(DiscoDataContext database)
        {
            lock (testOidcOptions)
            {
                database.DiscoConfiguration.SsoEnabled = false;
                database.DiscoConfiguration.SsoConfiguration = null;
                database.SaveChanges();

                testOidcSession = null;
                testOidcSuccess = null;
                testOidcExpiration = null;
                testOidcUserPrincipalName = null;
            }
        }

        public static bool TryGetSuccessfulSessionConfiguration(string sessionId, out SsoConfiguration configuration)
        {
            if (sessionId == null)
            {
                configuration = null;
                return false;
            }

            lock (testOidcOptions)
            {
                if (testOidcSession.HasValue && string.Equals(testOidcSession.Value.ToString("N"), sessionId, StringComparison.Ordinal) && testOidcSuccess.GetValueOrDefault())
                {
                    configuration = testSsoConfiguration.Clone();
                    return true;
                }

                configuration = null;
                return false;
            }
        }

        public static void ConfigureDiscoIctAuthentication(this IAppBuilder app)
        {
            var ssoEnabled = false;
            var ssoAdministrativelyDisabled = false;
            var ssoConfig = default(SsoConfiguration);
            using (var db = new DiscoDataContext())
            {
                ssoAdministrativelyDisabled = db.DiscoConfiguration.SsoAdministrativelyDisabled;
                ssoEnabled = db.DiscoConfiguration.SsoEnabled;
                if (ssoEnabled)
                    ssoConfig = db.DiscoConfiguration.SsoConfiguration;
            }

            if (!ssoAdministrativelyDisabled && ssoEnabled)
            {
                app.MapWhen(context =>
                    !context.Request.Path.StartsWithSegments(new PathString("/ClientBin")) &&
                    !context.Request.Path.StartsWithSegments(new PathString("/ClientSource")) &&
                    !context.Request.Path.StartsWithSegments(new PathString("/Services/Client"))
                    , a =>
                    {
                        a.Use((context, next) =>
                        {
                            if (context.Authentication.User != null &&
                                context.Authentication.User is WindowsPrincipal)
                                context.Authentication.User = null;
                            return next();
                        });

                        a.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

                        a.UseCookieAuthentication(new CookieAuthenticationOptions()
                        {
                            CookieName = "DiscoIctAuth",
                            CookieHttpOnly = true,
                            CookieSecure = CookieSecureOption.Always,
                            ExpireTimeSpan = TimeSpan.FromHours(12),
                            SlidingExpiration = false,
                        });
                        a.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions()
                        {
                            ClientId = ssoConfig.ClientId,
                            Authority = ssoConfig.Authority,
                            TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                            {
                                NameClaimType = ssoConfig.UpnClaimName,
                            },
                            Notifications = new OpenIdConnectAuthenticationNotifications()
                            {
                                SecurityTokenValidated = context =>
                                {
                                    var identity = context.AuthenticationTicket.Identity;

                                    User user;
                                    using (var db = new DiscoDataContext())
                                    {
                                        if (!UserService.TryGetUserByUserPrincipalName(identity.Name, db, false, out user))
                                            throw new InvalidOperationException($"Unable to locate the user principal name '{identity.Name}' in the domain forest");
                                    }

                                    var existingClaim = identity.FindFirst(context.AuthenticationTicket.Identity.NameClaimType);
                                    if (existingClaim != null)
                                        identity.RemoveClaim(existingClaim);

                                    identity.AddClaim(new Claim(context.AuthenticationTicket.Identity.NameClaimType, user.UserId));
                                    identity.AddClaim(new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", @"DiscoIctSso"));

                                    context.Request.User = new ClaimsPrincipal(identity);
                                    return Task.CompletedTask;
                                },
                            },
                            CallbackPath = new PathString("/API/Authentication/OIDC/SignIn"),
                        });

                        a.MapSignalR();
                    });
                app.UseStageMarker(PipelineStage.Authenticate);
            }
            else if (!ssoAdministrativelyDisabled)
            {
                // configure "testing" mode to allow for SSO configuration
                // while testing, show PII to get more information about exceptions
                IdentityModelEventSource.ShowPII = true;

                app.MapWhen(context =>
                    context.Request.Path.StartsWithSegments(new PathString("/API/Authentication/OIDC/Test"))
                    || context.Request.Path.StartsWithSegments(new PathString("/API/Authentication/OIDC/SignIn")), a =>
                {
                    a.Use((context, next) =>
                    {
                        if (context.Authentication.User != null &&
                            context.Authentication.User is WindowsPrincipal)
                            context.Authentication.User = null;
                        return next();
                    });

                    a.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

                    a.UseCookieAuthentication(new CookieAuthenticationOptions()
                    {
                        CookieName = "DiscoIctAuthTest",
                        CookieHttpOnly = true,
                        CookieSecure = CookieSecureOption.Always,
                        ExpireTimeSpan = TimeSpan.FromMinutes(15),
                        SlidingExpiration = false,
                    });

                    a.UseOpenIdConnectAuthentication(testOidcOptions);

                    a.Map("/API/Authentication/OIDC/Test", ab =>
                    {
                        ab.Use((context, next) =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            return next();
                        });
                    });
                });

                app.UseStageMarker(PipelineStage.Authenticate);

                app.MapSignalR();
            }
            else
            {
                app.MapSignalR();
            }
        }

        private static void MapSignalR(this IAppBuilder app)
        {
            var hubConfig = new HubConfiguration()
            {
                EnableJavaScriptProxies = false,
            };
            app.MapSignalR("/API/Signalling", hubConfig)
                .UseStageMarker(PipelineStage.MapHandler);
        }

    }
}