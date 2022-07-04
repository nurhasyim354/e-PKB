using ePKBModel.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Data.Entity;


[RoutePrefix("api/account")]
public class AccountController : ApiController
{
    AppModel context = new AppModel();
    ApplicationUserManager UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
    //SEND EMAIL VERIFICATION
    public static void EmailVerifyLinkGenerator(string email)
    {
        ApplicationUserManager UserManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
        var userId = UserManager.FindByName(email).Id;
        var token = UserManager.GenerateEmailConfirmationToken(userId);
        var url = string.Format(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/update/email?id={0}&code={1}", HttpUtility.UrlEncode(email), System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token)));
        Mailer.EmailVerify(email, url);
    }

    [Route("register")]
    public async Task<IHttpActionResult> PostRegister(AuthParam model)
    {
        var role = context.AspNetRoles.Where(a => a.Name.ToLower() == "User").FirstOrDefault();
        if (role == null)
            return BadRequest("Project Setup incomplete. User role not defined.");

        string newid = Helpers.NewId();
        ApplicationUser userManager = new ApplicationUser()
        {
            Id = newid,
            UserName = model.UserName,
            Email = model.UserProfile.Email,
            EmailConfirmed = true,
        };
        var result = await UserManager.CreateAsync(userManager, model.Password);

        if (!result.Succeeded)
            return BadRequest(GetErrorResult(result, "Gagal mendaftarkan pengguna:"));


        AspNetUser aspNetUser = context.AspNetUsers.Where(a => a.Id == newid).FirstOrDefault();
        UserProfile userprofile = model.UserProfile;
        userprofile.Id = Helpers.NewId();
        userprofile.CreateDate = DateTime.UtcNow;
        userprofile.IdAspNetUser = newid;
        userprofile.UserAgent = HttpContext.Current.Request.UserAgent;
        userprofile.IPAddress = HttpContext.Current.Request.Headers["X-Forwarded-For"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

        aspNetUser.AspNetRoles.Add(role);
        context.UserProfiles.Add(userprofile);

        //EmailVerifyLinkGenerator(userprofile.Email);
        //if (userprofile.IsSubscribed)
        //    registerMailchimp(user);

        await context.SaveChangesAsync();
        return Ok(userprofile.Id);
    }
    [Route("guest")]
    public IHttpActionResult PostGuest(AuthParam model)
    {
        var user = context.UserProfiles.Where(p => p.Email.ToLower() == model.UserName.ToLower()
        && p.FirstName.ToLower() == model.UserProfile.FirstName.ToLower()
        && p.LastName.ToLower() == model.UserProfile.LastName.ToLower()).FirstOrDefault();

        if (user == null)
        {
            user = model.UserProfile;
            user.Id = Helpers.NewId();
            user.IdAspNetUser = null;
            user.UserAgent = HttpContext.Current.Request.UserAgent;
            user.IPAddress = HttpContext.Current.Request.Headers["X-Forwarded-For"] ?? HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            user.CreateDate = DateTime.UtcNow;
            context.UserProfiles.Add(user);
        }
        else
        {
            user.FirstName = model.UserProfile.FirstName;
            user.LastName = model.UserProfile.LastName;
            //user.IsSubscribed = model.AspNetUserProfile.IsSubscribed;
            user.LastUpdateDate = DateTime.UtcNow;
        }
        context.SaveChanges();

        var result = new
        {
            Id = user.Id,
            Roles = new string[] { "guest" },
            Firstname = user.FirstName,
            Lastname = user.LastName,
            Email = user.Email,
            Contact = user.Contact,
        };
        return Ok(result);
    }

    [Route("passwordResetLink")]
    public async Task<IHttpActionResult> PostPasswordResetLink(AuthParam param)
    {
        var user = context.AspNetUsers.FirstOrDefault(p => p.UserName.ToLower() == param.UserName.ToLower());
        if (user == null)
            return BadRequest("Email belum terdaftar!");

        var userId = UserManager.FindByName(param.UserName).Id;
        var token = await UserManager.GeneratePasswordResetTokenAsync(userId);
        var url = string.Format(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/update/password?id={0}&code={1}", HttpUtility.UrlEncode(user.UserName), System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token)));

        Mailer.ResetPassword(user.UserProfiles.FirstOrDefault(), url);
        return Ok();
    }

    [Route("passwordreset")]
    public async Task<IHttpActionResult> PostPasswordReset(AuthParam model)
    {
        var email = HttpUtility.UrlDecode(model.UserName);
        var token = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(model.Token));
        var userId = UserManager.FindByName(email).Id;
        IdentityResult result = await UserManager.ResetPasswordAsync(userId, token, model.Password);

        if (!result.Succeeded)
        {
            return BadRequest(GetErrorResult(result));
        }
        return Ok();
    }

    [Authorize]
    [Route("profile/{id}")]
    public async Task<IHttpActionResult> GetProfile(string id)
    {
        var res = from a in await context.UserProfiles.Where(p => p.Id == id).ToListAsync()
                  select new
                  {
                      a.Id,
                      a.FirstName,
                      a.LastName,
                      a.Email,
                      a.Contact,
                  };

        return Ok(res.FirstOrDefault());
    }
    
    [Authorize]
    [Route("profile")]
    public async Task<IHttpActionResult> PostProfile(AuthParam param)
    {
        var exist = await context.UserProfiles.FirstOrDefaultAsync(p => p.Id == param.UserProfile.Id);
        if (exist == null)
            return BadRequest("Invalid data");

        exist.FirstName = param.UserProfile.FirstName;
        exist.LastName = param.UserProfile.LastName;
        exist.Email = param.UserProfile.Email;

        if (!string.IsNullOrEmpty(param.CurrentPassword))
        {
            var result = UserManager.ChangePasswordAsync(exist.AspNetUser.Id, param.CurrentPassword, param.Password).Result;

            string errmsg = "";
            foreach (var error in result.Errors)
                errmsg = errmsg + "\n" + error;

            if (result.Errors.Count() > 0)
                return BadRequest(errmsg);
        }

        await context.SaveChangesAsync();
        return Ok();
    }
   
    private string GetErrorResult(IdentityResult result, string message = "")
    {
        string error = "";
        foreach (var err in result.Errors)
            error = error + "\n" + err;

        if (error.ToLower().Contains("invalid token"))
            return "Your reset password link has expired, you may only use the link up to 30 minutes since you reset your password. Please reset your password again";

        return message + error;
    }

    [Route("emailVerifyLink")]
    public IHttpActionResult PostEmailVerifyLink(UserProfile user)
    {
        EmailVerifyLinkGenerator(user.Email);
        return Ok();
    }

    [Route("emailVerify")]
    public async Task<IHttpActionResult> PostEmailVerify(AuthParam param)
    {
        var email = HttpUtility.UrlDecode(param.UserProfile.Email);
        var token = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(param.Token));
        var userId = UserManager.FindByName(email).Id;

        var result = await UserManager.ConfirmEmailAsync(userId, token);
        var isSuccess = true;

        if (!result.Succeeded)
            isSuccess = false;
        else
        {
            UserProfile user = context.UserProfiles.Where(a => a.Email.ToLower() == email).FirstOrDefault();
            user.LastUpdateDate = DateTime.UtcNow;
            user.VerificationDate = DateTime.UtcNow;
            context.SaveChanges();
        }

        return Ok(isSuccess);
    }

    public class AuthParam
    {
        public UserProfile UserProfile { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CurrentPassword { get; set; }
        public string EmailOld { get; set; }
        public string Token { get; set; }
    }

}