using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BottlePos
{
    class clsEmail
    {
        public clsEmail()
        {

        }

        public bool sendEmail(string to, string cc, string bcc, string subject, string body)
        {
            try
            {
                char[] sep = { ',' };
                string[] w = to.Split(sep);

                var message = new MailMessage();
                for (int i = 0; i < w.Length; i++)
                {
                    if (!(w[i].ToLower().StartsWith("undertesting") && w[i].ToLower().EndsWith("@gmail.com")) && !(w[i].ToLower().EndsWith("@mk.com")) && !(w[i].ToLower().StartsWith("1") && w[i].ToLower().EndsWith("@bottlecapps.com")))
                        message.To.Add(w[i]);
                }
                if (cc != null && cc != "" && !(cc.ToLower().StartsWith("undertesting") && cc.ToLower().EndsWith("@gmail.com")) && !(cc.ToLower().EndsWith("@mk.com")) && !(cc.ToLower().StartsWith("1") && cc.ToLower().EndsWith("@bottlecapps.com")))
                    message.CC.Add(cc);
                if (bcc != null && bcc != "")
                {
                    string[] Tmpbcc = bcc.Split(sep);
                    for (int j = 0; j < Tmpbcc.Length; j++)
                    {
                        if (!(Tmpbcc[j].ToLower().StartsWith("undertesting") && Tmpbcc[j].ToLower().EndsWith("@gmail.com")) && !(Tmpbcc[j].ToLower().EndsWith("@mk.com")) && !(Tmpbcc[j].ToLower().StartsWith("1") && Tmpbcc[j].ToLower().EndsWith("@bottlecapps.com")))
                            message.Bcc.Add(Tmpbcc[j]);
                    }
                }
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                if (message.To.Count > 0)
                {

                    var smtp = new SmtpClient();
                    smtp.Send(message);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message);
                return false;
            }
        }

        public bool sendEmailUser(string to, string cc, string bcc, string subject, string body, string FromStoreAddress)
        {
            try
            {
                char[] sep = { ',' };
                string[] w = to.Split(sep);

                var message = new MailMessage();
                for (int i = 0; i < w.Length; i++)
                {
                    if (!(w[i].ToLower().StartsWith("undertesting") && w[i].ToLower().EndsWith("@gmail.com")) && !(w[i].ToLower().EndsWith("@mk.com")) && !(w[i].ToLower().StartsWith("1") && w[i].ToLower().EndsWith("@bottlecapps.com")))
                        message.To.Add(w[i]);
                }
                if (cc != null && cc != "" && !(cc.ToLower().StartsWith("undertesting") && cc.ToLower().EndsWith("@gmail.com")) && !(cc.ToLower().EndsWith("@mk.com")) && !(cc.ToLower().StartsWith("1") && cc.ToLower().EndsWith("@bottlecapps.com")))
                    message.CC.Add(cc);
                if (bcc != null && bcc != "")
                {
                    string[] Tmpbcc = bcc.Split(sep);
                    for (int j = 0; j < Tmpbcc.Length; j++)
                    {
                        if (!(Tmpbcc[j].ToLower().StartsWith("undertesting") && Tmpbcc[j].ToLower().EndsWith("@gmail.com")) && !(Tmpbcc[j].ToLower().EndsWith("@mk.com")) && !(Tmpbcc[j].ToLower().StartsWith("1") && Tmpbcc[j].ToLower().EndsWith("@bottlecapps.com")))
                            message.Bcc.Add(Tmpbcc[j]);
                    }
                }
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                var fromaddressStoreuser = "";
                fromaddressStoreuser = System.Configuration.ConfigurationManager.AppSettings.Get("MailUserName");
                var fromAddress = new MailAddress(fromaddressStoreuser, System.Configuration.ConfigurationManager.AppSettings.Get("BCappsUserName").ToString());
                message.From = fromAddress;

                //  SmtpClient smtp = new SmtpClient();
                if (message.To.Count > 0)
                {
                    using (var smtp = new SmtpClient())
                    {
                        smtp.Send(message);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("" + ex.Message);
                return false;
            }
        }
    }

}
