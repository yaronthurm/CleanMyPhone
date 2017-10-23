using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace CleanMyPhone
{
    public class EmailSender
    {
        private string _server;
        private int _port;
        private bool _enableSsl;
        private NetworkCredential _credentials;
        private string _from;
        private string[] _to;

        public static EmailSender GetEmailSender(string configFilePath)
        {
            var ret = new EmailSender();
            var configValues = File.ReadAllLines(configFilePath)
                .Select(x => x.Split(new char[] { '=' }, 2))
                .Select(x => new { key = x[0].Trim(), value = x[1].Trim() })
                .ToDictionary(x => x.key, x => x.value);
            ret._from = configValues["from"];
            ret._to = configValues["to"].Split(',').Select(x => x.Trim()).ToArray();
            ret._server = configValues["server"];
            ret._port = int.Parse(configValues["port"]);
            ret._credentials = new NetworkCredential(configValues["username"], configValues["password"]);
            ret._enableSsl = bool.Parse(configValues["enable-ssl"]);

            return ret;
        }

        public void SendEmail(string displayName, string subject, string body)
        {
            var smtpClient = new SmtpClient(_server);
            smtpClient.Port = _port;
            smtpClient.Credentials = _credentials;
            smtpClient.EnableSsl = _enableSsl;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(_from, displayName);
            foreach (var to in _to)
                mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;

            smtpClient.Send(mail);
        }
    }
}
