using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.IO;
using System.Configuration;
using System.Web.Hosting;

public class MailHelper
{
    private const int Timeout = 180000;
    private readonly string _sender;
    private readonly string _name;
    private readonly string _to;
    private readonly string _cc;
    private readonly string _bcc;
    private readonly bool _ssl;

    public string Sender { get; set; }
    public string Name { get; set; }
    public string Recipient { get; set; }
    public string RecipientCC { get; set; }
    public string RecipientBCC { get; set; }
    public string ReplyTo { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public Attachment Attachment { get; set; }
    public bool isPlainText { get; set; }

    public MailHelper()
    {
        //MailServer Settings
        _sender = ConfigurationManager.AppSettings["email.sender"];
        _name = ConfigurationManager.AppSettings["email.name"];
        _to = ConfigurationManager.AppSettings["email.to"];
        _cc = ConfigurationManager.AppSettings["email.cc"];
        _bcc = ConfigurationManager.AppSettings["email.bcc"];
        _ssl = false;
    }

    public bool Send()
    {
        try
        {
            // We do not catch the error here... let it pass direct to the caller
            Attachment att = null;
            Sender = Sender ?? _sender;
            Recipient = Recipient ?? _to;

            bool isHtml = true;
            if (isPlainText == true) isHtml = false;

            var message = new MailMessage(Sender, Recipient, Subject, Body) { IsBodyHtml = isHtml };
            if (Name != null)
            {
                message.From = new MailAddress(Sender, Name);
            }
            else
            {
                message.From = new MailAddress(_sender, _name);
            }
            if (RecipientCC != null)
            {
                string[] array = RecipientCC.Split(';');
                foreach (string value in array)
                {
                    message.CC.Add(value.Trim());
                }
            }
            else if (RecipientCC == null && _cc != "")
            {
                string[] array = _cc.Split(';');
                foreach (string value in array)
                {
                    message.CC.Add(value.Trim());
                }
            }
            if (RecipientBCC != null)
            {
                string[] array = RecipientBCC.Split(';');
                foreach (string value in array)
                {
                    message.Bcc.Add(value.Trim());
                }
            }
            else if (RecipientBCC == null && _bcc != "")
            {
                string[] array = _bcc.Split(';');
                foreach (string value in array)
                {
                    message.Bcc.Add(value.Trim());
                }
            }
            if (ReplyTo != null)
            {
                message.ReplyToList.Add(ReplyTo);
            }
            var smtp = new SmtpClient();

            if (Attachment != null)
            {
                message.Attachments.Add(Attachment);
            }

            smtp.EnableSsl = _ssl;
            smtp.Send(message);

            if (att != null)
                att.Dispose();
            message.Dispose();
            smtp.Dispose();

            return true;
        }

        catch (Exception ex)
        {
            string logFile = HostingEnvironment.MapPath("/ErrorMailHelper.txt");
            StreamWriter sw = new StreamWriter(logFile, true);
            sw.WriteLine("{0};{1};{2};{3};{4};{5}", Sender, Recipient, Subject, Body, DateTime.UtcNow, ex.Message);
            sw.Close();
            return false;
        }
    }
}