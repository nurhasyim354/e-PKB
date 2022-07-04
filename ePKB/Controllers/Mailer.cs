using ePKBModel.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

public class Mailer
{
    //ERROR
    public static void Error(string msg, string subject)
    {
        var MailHelper = new MailHelper
        {
            Recipient = ConfigurationManager.AppSettings["email.error"],
            Subject = subject,
            Body = msg,
        };
        MailHelper.Send();
    }

    //RESET PASSWORD
    public static void ResetPassword(UserProfile user, string url)
    {
        var body = Helpers.EmailTemplate(null, "passwordReset")
            .Replace("{url}", url);
        var MailHelper = new MailHelper
        {
            Recipient = user.Email,
            Subject = "Petunjuk untuk Reset Password",
            Body = body,
        };
        MailHelper.Send();
    }

    //VERIFY EMAIL
    public static void EmailVerify(string email, string url)
    {
        var body = Helpers.EmailTemplate(null, "emailVerify")
            .Replace("{email}", email)
            .Replace("{url}", url);
        var MailHelper = new MailHelper
        {
            Recipient = email,
            Subject = "Verify your email address",
            Body = body,
        };
        MailHelper.Send();
    }

    //UPDATE EMAIL
    public static void EmailUpdate(UserProfile user, string oldEmail, string url)
    {
        var body = Helpers.EmailTemplate(null, "emailUpdate")
            .Replace("{email}", user.Email)
            .Replace("{url}", url);
        var MailHelper = new MailHelper
        {
            Recipient = oldEmail,
            Subject = "Update your email address",
            Body = body,
        };
        MailHelper.Send();
    }

    public static void registerEpkb(TaxPayment tp)
    {
        var body = Helpers.EmailTemplate(null, "register_epkb")
            .Replace("{noreg}", tp.RegNumber)
            .Replace("{nopol}", tp.Vehicle.PoliceNumber)
            .Replace("{name}", tp.UserProfile.FirstName.ToUpper())
            .Replace("{date}", tp.CreateDate.ToString("dd MMMM yyyy"))
            .Replace("{url_profile}", string.Format(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/profile/"))
            .Replace("{vehicle_type}", tp.Vehicle.Model);

        var MailHelper = new MailHelper
        {
            Recipient = tp.UserProfile.Email,
            Subject = "Registrasi pembayaran pajak kendaraan online berhasil",
            Body = body,
        };
        MailHelper.Send();
    }

    public static void notifEpkb(TaxPayment tp, string msg)
    {
        var body = Helpers.EmailTemplate(null, "notif_epkb")
            .Replace("{noreg}", tp.RegNumber)
            .Replace("{nopol}", tp.Vehicle.PoliceNumber)
            .Replace("{name}", tp.UserProfile.FirstName.ToUpper())
            .Replace("{date}", tp.CreateDate.ToString("dd MMMM yyyy"))
             .Replace("{msg}", msg)
            .Replace("{url_profile}", string.Format(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/profile/"))
            .Replace("{vehicle_type}", tp.Vehicle.Model);

        var MailHelper = new MailHelper
        {
            Recipient = tp.UserProfile.Email,
            Subject = "Informasi terkait pembayaran pajak kendaraan online",
            Body = body,
        };
        MailHelper.Send();
    }

    public static void datavalidatedEpkb(TaxPayment tp)
    {
        var body = Helpers.EmailTemplate(null, "datavalidated")
            .Replace("{noreg}", tp.RegNumber)
            .Replace("{nopol}", tp.Vehicle.PoliceNumber)
             .Replace("{pkb}", tp.PKB.ToString("N"))
            .Replace("{pkb_add}", tp.PKB_add.ToString("N"))
            .Replace("{swdkllj}", tp.SWDKLLJ.ToString("N"))
            .Replace("{swdkllj_add}", tp.SWDKLLJ_add.ToString("N"))
            .Replace("{admstnk}", tp.ADMSTNK.ToString("N"))
            .Replace("{admstnk_add}", tp.ADMSTNK_add.ToString("N"))
            .Replace("{admtnkb}", tp.ADMTNKB.ToString("N"))
            .Replace("{admtnkb_add}", tp.ADMTNKB_add.ToString("N"))
            .Replace("{total}", tp.GetTotal().ToString("N"))
            .Replace("{name}", tp.UserProfile.FirstName.ToUpper())
            .Replace("{date}", tp.CreateDate.ToString("dd MMMM yyyy"))
            .Replace("{url_profile}", string.Format(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/profile/"))
            .Replace("{vehicle_type}", tp.Vehicle.Model);

        var MailHelper = new MailHelper
        {
            Recipient = tp.UserProfile.Email,
            Subject = "Validasi data pembayaran pajak kendaraan online berhasil",
            Body = body,
        };
        MailHelper.Send();
    }

    public static void paymentreceivedEpkb(TaxPayment tp)
    {
        var root = "/uploads";
        var reg_dir = Path.Combine(root, tp.RegNumber);
        var phy_path = HttpContext.Current.Server.MapPath(reg_dir);
        var files = Directory.GetFiles(phy_path);
        var qrfile = files.Where(p => Path.GetFileName(p).Contains("qrcode")).FirstOrDefault();

        var body = Helpers.EmailTemplate(null, "paymentreceived")
            .Replace("{noreg}", tp.RegNumber)
            .Replace("{nopol}", tp.Vehicle.PoliceNumber)
            .Replace("{name}", tp.UserProfile.FirstName.ToUpper())
            .Replace("{date}", tp.CreateDate.ToString("dd MMMM yyyy"))
            .Replace("{url_qrcode}", string.Format("{0}/{1}", HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority), Path.Combine(reg_dir, Path.GetFileName(qrfile))))
            .Replace("{url_profile}", string.Format(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/profile/"))
            .Replace("{vehicle_type}", tp.Vehicle.Model);

        var MailHelper = new MailHelper
        {
            Recipient = tp.UserProfile.Email,
            Subject = "Pembayaran pajak kendaraan online telah selesai",
            Body = body,
        };
        MailHelper.Send();
    }
}