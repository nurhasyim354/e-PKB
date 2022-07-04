using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;

[assembly: OwinStartup(typeof(oAuthStartup))]

public class oAuthStartup
{
    public void Configuration(IAppBuilder app)
    {
        //Identity Config
        app.CreatePerOwinContext(ApplicationDbContext.Create);
        app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
        OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
        {
            AllowInsecureHttp = true,
            TokenEndpointPath = new PathString("/token"),
            AccessTokenExpireTimeSpan = TimeSpan.FromDays(30),
            Provider = new OAuthProvider(),
        };

        // Token Generation
        app.UseOAuthBearerTokens(OAuthServerOptions);
        //app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
    }
}

public class ApplicationUserManager : UserManager<ApplicationUser>
{
    public ApplicationUserManager(IUserStore<ApplicationUser> store)
        : base(store)
    {
    }

    public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
    {
        var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
        // Configure validation logic for usernames
        manager.UserValidator = new UserValidator<ApplicationUser>(manager)
        {
            AllowOnlyAlphanumericUserNames = false,
            RequireUniqueEmail = true,
        };
        // Configure validation logic for passwords
        manager.PasswordValidator = new PasswordValidator
        {
            RequiredLength = 6,
            //RequireNonLetterOrDigit = false,
            //RequireDigit = true,
            //RequireLowercase = true,
            //RequireUppercase = true,
        };

        // Setting for User Lockout
        manager.UserLockoutEnabledByDefault = true;
        manager.MaxFailedAccessAttemptsBeforeLockout = 10;
        manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(10);

        var dataProtectionProvider = options.DataProtectionProvider;
        if (dataProtectionProvider != null)
        {
            manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("Stefanto Tandyasraya"))
            {
                //Set token Lifespan
                TokenLifespan = TimeSpan.FromMinutes(30)
            };
        }
        return manager;
    }
}

public class ApplicationUser : IdentityUser
{
    public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
    {
        // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
        // Add custom user claims here
        return userIdentity;
    }
}

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext() : base("AppModel", throwIfV1Schema: false) { }
    public static ApplicationDbContext Create()
    {
        return new ApplicationDbContext();
    }
}