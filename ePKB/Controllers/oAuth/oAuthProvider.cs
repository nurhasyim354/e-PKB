using ePKBModel.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

public class OAuthProvider : OAuthAuthorizationServerProvider
{
    #region Important
    public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
    {
        // Resource owner password credentials does not provide a client ID.
        if (context.ClientId == null)
        {
            context.Validated();
        }

        return Task.FromResult<object>(null);
    }

    public override Task TokenEndpoint(OAuthTokenEndpointContext context)
    {
        foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
        {
            context.AdditionalResponseParameters.Add(property.Key, property.Value);
        }

        return Task.FromResult<object>(null);
    }
    #endregion

    public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext param)
    {

        try
        {
            var userManager = param.OwinContext.GetUserManager<ApplicationUserManager>();

            ApplicationUser user = await userManager.FindAsync(param.UserName, param.Password);
            var userIsExist = userManager.FindByName(param.UserName);

            if (userIsExist == null)
            {
                param.SetError("Email is not registered", "Please register first");
                return;
            }

            //Check if user is lockedout
            var lockoutIsEnabled = userManager.UserLockoutEnabledByDefault;

            var userId = userManager.FindByName(param.UserName).Id;
            if (lockoutIsEnabled && userManager.IsLockedOut(userId))
            {
                param.SetError("Account has been locked", "Your account has been locked because there has been more than 10 invalid login attemp. Please try login again within the 10 minutes.");
                return;
            }

            //Check email verification status
            if (!userManager.IsEmailConfirmed(userId))
            {
                param.SetError("Email has yet to be verfied", string.Format("Please click the link that we've just sent you to verify your email.", param.UserName));

                //SEND VERIFICATION EMAIL
                AccountController.EmailVerifyLinkGenerator(param.UserName);
                return;
            }

            if (user == null)
            {
                param.SetError("Incorrect Password", "Forgot your password? You can reset it anytime");

                //Add to failed count if user make failed login attempt
                var failedCount = userManager.GetAccessFailedCount(userId);
                var maxFailedCount = userManager.MaxFailedAccessAttemptsBeforeLockout - 1;
                if (lockoutIsEnabled && failedCount >= maxFailedCount)
                {
                    userManager.SetLockoutEnabled(userId, true);
                }
                userManager.AccessFailed(userId);
                return;
            }

            //Reset lockout count when login success
            userManager.SetLockoutEnabled(user.Id, false);
            userManager.ResetAccessFailedCount(user.Id);

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager, OAuthDefaults.AuthenticationType);
            AppModel context = new AppModel();
            var aspuser = from a in context.UserProfiles.Where(p => p.IdAspNetUser == user.Id).ToList()
                          select new
                          {
                              Id = a.Id,
                              Roles = from b in a.AspNetUser.AspNetRoles
                                      select b.Name,
                              FirstName = a.FirstName,
                              LastName = a.LastName,
                              Email = a.Email,
                              TaxPaymentsCount = a.TaxPayments.Count(),
                          };

            var properties = new AuthenticationProperties(new Dictionary<string, string>
                {
                    {
                        "user", JsonConvert.SerializeObject(aspuser.FirstOrDefault())
                    },
                });

            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
            param.Validated(ticket);
        }
        catch(Exception ex)
        {
            param.SetError("Unexpected", ex.Message);
            return;
        }
       
    }
}