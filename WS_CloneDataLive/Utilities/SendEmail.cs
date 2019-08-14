using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Mail;
using System.Net;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace WS_CloneDataLive
{
    public class SendEmail
    {
        public static Exception Send_Email(List<string> ToEmail, List<string> CC, string title, string content, bool isHtml)
        {

            for (int j = 0; j < 5; j++)
            {
                try
                {
                    MailMessage mail = new MailMessage();
                    string FromEmail = ConfigurationManager.AppSettings["Email"];
                    string Pass = ConfigurationManager.AppSettings["Password"];
                    string FromName = ConfigurationManager.AppSettings["FromName"];

                    SmtpClient smtpServer = new SmtpClient(ConfigurationManager.AppSettings["SMTP_Server"]);
                    mail.From = new MailAddress(FromEmail, FromName);

                    if (ToEmail == null || ToEmail.Count == 0)
                    {
                        return new Exception("To Email is Null.");
                    }
                    for (int i = 0; i < ToEmail.Count; i++)
                    {
                        if (string.IsNullOrEmpty(ToEmail[i]))
                        {
                            continue;
                        }
                        mail.To.Add(ToEmail[i]); // Email cần đến
                    }

                    if (CC != null)
                    {
                        for (int i = 0; i < CC.Count; i++)
                        {
                            if (string.IsNullOrEmpty(CC[i]))
                            {
                                continue;
                            }
                            mail.CC.Add(CC[i]); // Email cần CC
                        }
                    }


                    mail.Subject = (title == "") ? "No subject" : title;
                    mail.Body = content;
                    mail.IsBodyHtml = isHtml;
                    mail.Priority = MailPriority.High;
                    smtpServer.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
                    smtpServer.Credentials = new NetworkCredential(FromEmail, Pass);
                    smtpServer.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);

                    Disable_CertificateValidation();

                    smtpServer.Send(mail);

                    File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\" + DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString() + ": Sending email \"" + title + "\" TO " + String.Join(", ", ToEmail.ToArray()) + " and " + ((CC == null) ? "no CC" : "CC to " + String.Join(", ", CC.ToArray())) + " Succufully!", true);

                    break;
                }
                catch (Exception er)
                {
                    File_Read_Write.Write_File(AppDomain.CurrentDomain.BaseDirectory + @"Log\" + DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString() + ": Try " + (j + 1) + ": Error - Sending email \"" + title + "\" TO " + String.Join(", ", ToEmail.ToArray()) + " and " + ((CC == null) ? "no CC" : "CC to " + String.Join(", ", CC.ToArray())) + " error!" + er.Message, true);
                }
            }
            return null;
        }



        static void Disable_CertificateValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                )
                {
                    return true;
                };
        }
    }
}
