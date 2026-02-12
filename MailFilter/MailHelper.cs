using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ActiveUp.Net.Mail;
using ActiveUp.Net.Security;
using log4net;
using Tct.MailFilterService.Configuration;

namespace Tct.MailFilterService
{
    public class MailHelper : IDisposable
    {
        private const string rfc2822MessageId = "Message-ID";
        private const string rfc2822InReplyTo = "In-Reply-To";
        private const string rfc2822References = "References";
        private readonly Regex grabRefRegex = new Regex("<[^>]*>", RegexOptions.Singleline);
        private readonly Regex messageIdCaptureRegex = new Regex("\\[[*]([^*]+)[*]\\]");
        private readonly Random random = new Random();
        private readonly DateStore dateStore;
        private MailFilterConfig config;
        private ILog log;
        private int[] messageIds { set; get; }
        private Mailbox inbox;
        public IEnumerable<Message> messageCollection;
        Imap4Client imapClient = null;

        public MailHelper(ILog _log , MailFilterConfig _config, DateStore _deteStore)
        {
            log = _log;
            config = _config;
            dateStore = _deteStore;
            imapClient = new Imap4Client();
            SslHandShake sslh = new SslHandShake(config.Server, System.Security.Authentication.SslProtocols.Tls, (sender, certificate, chain, errors) => 
            {
                return errors == System.Net.Security.SslPolicyErrors.None || ((System.Security.Cryptography.X509Certificates.X509Certificate2)certificate).Thumbprint == "EBE6DC113D995EEDD93F3B0BE72C6695FAE25881";
            });
            if (config.SslEnabled)
                imapClient.ConnectSsl(config.Server, config.Port, sslh);
            else
                imapClient.Connect(config.Server, config.Port);
            imapClient.Login(config.Login, config.Password);
            inbox = imapClient.SelectMailbox("INBOX");
        }
        public void Dispose()
        {
            Disconnect();
            imapClient = null;
        }
        public void Read(string alias, out DateTime dateBefore)
        {
            dateBefore = dateStore.GetValue(alias);
            string f = string.Format("HEADER TO \"{0}\" NOT BEFORE {1}", alias, dateBefore.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture));
            var mco = inbox.SearchParse(f);
            if (mco.Count == 0)
            {
                log.Info("There were no unseen emails in " + config.Login);
                messageCollection = null;
                return;
            }
            messageIds = inbox.Search(f);
            messageCollection = mco.Cast<Message>();
            if (dateStore.Exists(alias))
                messageCollection = messageCollection.Where(e => e.Date > dateStore.GetValue(alias));
        }
      
        public string CreateId(Message mailItem)
        {
            var mailId = mailItem.GetHeader(rfc2822MessageId);
            var references = mailItem.GetHeader( rfc2822References);
            var inreplyto = mailItem.GetHeader(rfc2822InReplyTo);
            ulong id = 0;

            var matchBody =
                messageIdCaptureRegex.Match(mailItem.BodyText.Text);
            var subject = mailItem.Subject;
            if (matchBody.Success)
            {
                return matchBody.Groups[1].Value;
            }
            var match = subject != null ? messageIdCaptureRegex.Match(subject) : null;
            if (match != null && match.Success)
            {
                return match.Groups[1].Value;
            }
            
            if (!string.IsNullOrEmpty(references))
            {
                var matcher = grabRefRegex.Match(references);
                id = getInt64HashCode(matcher.Value);
            }
            else if (!string.IsNullOrEmpty(inreplyto))
                id = getInt64HashCode(inreplyto);
            else if (!string.IsNullOrEmpty(mailId))
                id = getInt64HashCode(mailId);
            else
            {
                byte[] buf = new byte[8];
                random.NextBytes(buf);
                id = BitConverter.ToUInt64(buf, 0);
            }
            return encode(id);
        }
        private ulong getInt64HashCode(string strText)
        {
            ulong hashCode = 0;
            if (!String.IsNullOrEmpty(strText))
            {
                byte[] byteContents = Encoding.Unicode.GetBytes(strText);
                SHA256 hash =
                    new SHA256CryptoServiceProvider();
                byte[] hashText = hash.ComputeHash(byteContents);
                var hashCodeStart = BitConverter.ToUInt64(hashText, 0);
                var hashCodeMedium = BitConverter.ToUInt64(hashText, 8);
                var hashCodeEnd = BitConverter.ToUInt64(hashText, 24);
                hashCode = hashCodeStart ^ hashCodeMedium ^ hashCodeEnd;
            }
            return (hashCode);
        }
        private const string encodeTable = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string encode(ulong id)
        {
            var rest = id;
            var encoded = new StringBuilder();
            var tableSize = (uint)encodeTable.Length;
            while (rest > 0)
            {
                var index = (int)(rest % tableSize);
                encoded.Append(encodeTable[index]);
                rest /= tableSize;
            }
            return encoded.ToString();
        }
        public void Delete(Message me)
        {
            if (messageIds.Length <= 0) return;
            imapClient.Command("capability");
            foreach (int t in messageIds)
            {
                var msg = inbox.Fetch.MessageObject(t);
                if (msg == null) continue;
                if (msg.MessageId == me.MessageId)
                {
                    // imapClient.Command("copy " + t + " [Gmail]/Trash" );
                    inbox.UidDeleteMessage(t,true);
                    inbox.DeleteMessage(t,true);
                    break;
                }
            }
        }
        public void Disconnect()
        {
            if (imapClient != null)
                imapClient.Disconnect();            
        }

	}
    public static class Extension
    {
        public static string GetHeader(this Message m, string key)
        {
            var e = m.HeaderFields[key];
            return e;
        }
    }
}