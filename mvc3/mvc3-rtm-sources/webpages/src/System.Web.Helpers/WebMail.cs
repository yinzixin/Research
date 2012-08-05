using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web.Helpers.Resources;
using System.Web.WebPages;
using System.Web.WebPages.Scope;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Helpers {
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WebMail", Justification = "The name of this class is consistent with the naming convention followed in other helpers")]
    public static class WebMail {
        internal static readonly object _smtpServerKey = new object();
        internal static readonly object _smtpPortKey = new object();
        internal static readonly object _smtpUseDefaultCredentialsKey = new object();
        internal static readonly object _enableSslKey = new object();
        internal static readonly object _passwordKey = new object();
        internal static readonly object _userNameKey = new object();
        internal static readonly object _fromKey = new object();
        internal static readonly Lazy<IDictionary<object, object>> _smtpDefaults = new Lazy<IDictionary<object, object>>(ReadSmtpDefaults);

        ///////////////////////////////////////////////////////////////////////////
        // Public APIs
        public static void Send(string to,
                                string subject,
                                string body,
                                string from = null,
                                string cc = null,
                                IEnumerable<string> filesToAttach = null,
                                bool isBodyHtml = true,
                                IEnumerable<string> additionalHeaders = null) {

            if (filesToAttach != null) {
                foreach (string fileName in filesToAttach) {
                    if (String.IsNullOrEmpty(fileName)) {
                        throw new ArgumentException(HelpersResources.WebMail_ItemInCollectionIsNull, "filesToAttach");
                    }
                }
            }
            if (additionalHeaders != null) {
                foreach (string header in additionalHeaders) {
                    if (String.IsNullOrEmpty(header)) {
                        throw new ArgumentException(HelpersResources.WebMail_ItemInCollectionIsNull, "additionalHeaders");
                    }
                }
            }

            if (String.IsNullOrEmpty(SmtpServer)) {
                throw new InvalidOperationException(HelpersResources.WebMail_SmtpServerNotSpecified);
            }

            using (MailMessage message = new MailMessage()) {
                SetPropertiesOnMessage(message, to, subject, body, from, cc, filesToAttach,
                                       isBodyHtml, additionalHeaders);
                using (SmtpClient client = new SmtpClient()) {
                    SetPropertiesOnClient(client);
                    client.Send(message);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////
        // Public Properties
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "SmtpServer is more descriptive as compared to the actual argument \"value\"")]
        public static string SmtpServer {
            get {
                return ReadValue<string>(_smtpServerKey);
            }
            set {
                if (String.IsNullOrEmpty(value)) {
                    throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "SmtpServer");
                }
                ScopeStorage.CurrentScope[_smtpServerKey] = value;
            }
        }

        public static int SmtpPort {
            get {
                return ReadValue<int>(_smtpPortKey);
            }
            set {
                ScopeStorage.CurrentScope[_smtpPortKey] = value;
            }
        }

        public static string From {
            get {
                return ReadValue<string>(_fromKey);
            }
            set {
                ScopeStorage.CurrentScope[_fromKey] = value;
            }
        }

        public static bool SmtpUseDefaultCredentials {
            get {
                return ReadValue<bool>(_smtpUseDefaultCredentialsKey);
            }
            set {
                ScopeStorage.CurrentScope[_smtpUseDefaultCredentialsKey] = value;
            }
        }

        public static bool EnableSsl {
            get {
                return ReadValue<bool>(_enableSslKey);
            }
            set {
                ScopeStorage.CurrentScope[_enableSslKey] = value;
            }
        }

        public static string UserName {
            get {
                return ReadValue<string>(_userNameKey);
            }
            set {
                ScopeStorage.CurrentScope[_userNameKey] = value;
            }
        }

        public static string Password {
            get {
                return ReadValue<string>(_passwordKey);
            }
            set {
                ScopeStorage.CurrentScope[_passwordKey] = value;
            }
        }

        private static TValue ReadValue<TValue>(object key) {
            return (TValue)(ScopeStorage.CurrentScope[key] ?? _smtpDefaults.Value[key]);
        }

        private static IDictionary<object, object> ReadSmtpDefaults() {
            Dictionary<object, object> smtpDefaults = new Dictionary<object, object>();
            try {
                // Create a new SmtpClient object: this will read config & tell us what the default value is
                using (SmtpClient client = new SmtpClient()) {
                    smtpDefaults[_smtpServerKey] = client.Host;
                    smtpDefaults[_smtpPortKey] = client.Port;
                    smtpDefaults[_enableSslKey] = client.EnableSsl;
                    smtpDefaults[_smtpUseDefaultCredentialsKey] = client.UseDefaultCredentials;
                    var credentials = client.Credentials as NetworkCredential;
                    if (credentials != null) {
                        smtpDefaults[_userNameKey] = credentials.UserName;
                        smtpDefaults[_passwordKey] = credentials.Password;
                    }
                    else {
                        smtpDefaults[_userNameKey] = null;
                        smtpDefaults[_passwordKey] = null;
                    }
                    using (MailMessage message = new MailMessage()) {
                        smtpDefaults[_fromKey] = (message.From != null) ? message.From.Address : null;
                    }
                }
            }
            catch (InvalidOperationException) {
                // Due to Bug Dev10 PS 337470 ("SmtpClient reports InvalidOperationException when disposed"), we need to ignore the spurious InvalidOperationException
            }
            return smtpDefaults;
        }

        ///////////////////////////////////////////////////////////////////////////
        // Private impementation
        internal static void SetPropertiesOnClient(SmtpClient client) {
            // If no value has been assigned to these properties, at the very worst we will simply 
            // write back the values we just read from the SmtpClient
            if (SmtpServer != null) {
                client.Host = SmtpServer;
            }
            client.Port = SmtpPort;
            client.UseDefaultCredentials = SmtpUseDefaultCredentials;
            client.EnableSsl = EnableSsl;
            if (!String.IsNullOrEmpty(UserName)) {
                client.Credentials = new NetworkCredential(UserName, Password);
            }
        }

        internal static void SetPropertiesOnMessage(MailMessage message, string to, string subject,
                                                    string body, string from, string cc,
                                                    IEnumerable<string> filesToAttach, bool isBodyHtml,
                                                    IEnumerable<string> additionalHeaders) {
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isBodyHtml;

            if (to != null) {
                message.To.Add(to);
            }

            if (!string.IsNullOrEmpty(cc)) {
                message.CC.Add(cc);
            }

            if (additionalHeaders != null) {
                foreach (string header in additionalHeaders) {
                    int pos = header.IndexOf(':');
                    if (pos > 0) {
                        string name = header.Substring(0, pos).TrimEnd();
                        message.Headers.Add(name, header.Substring(pos + 1).TrimStart());
                    }
                }
            }

            if (from != null) {
                message.From = new MailAddress(from);
            }
            else if (!String.IsNullOrEmpty(From)) {
                message.From = new MailAddress(From);
            }
            else if (message.From == null || String.IsNullOrEmpty(message.From.Address)) {
                var httpContext = HttpContext.Current;
                if (httpContext != null) {
                    message.From = new MailAddress("DoNotReply@" + httpContext.Request.Url.Host);
                }
                else {
                    throw new InvalidOperationException(HelpersResources.WebMail_UnableToDetermineFrom);
                }
            }

            if (filesToAttach != null) {
                foreach (string file in filesToAttach) {
                    if (!Path.IsPathRooted(file) && HttpRuntime.AppDomainAppPath != null) {
                        message.Attachments.Add(new Attachment(Path.Combine(HttpRuntime.AppDomainAppPath, file)));
                    }
                    else {
                        message.Attachments.Add(new Attachment(file));
                    }
                }
            }
        }

    }
}