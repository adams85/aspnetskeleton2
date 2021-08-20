using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Security;
using MimeKit;
using WebApp.Core.Infrastructure;

namespace WebApp.Service.Mailing
{
    internal sealed class PickupDirMailClient : MailTransport
    {
        private readonly IGuidProvider _guidProvider;
        private readonly string _path;

        public PickupDirMailClient(IGuidProvider guidProvider) : this(guidProvider, Environment.CurrentDirectory) { }

        public PickupDirMailClient(IGuidProvider guidProvider, string path) : base(new NullProtocolLogger())
        {
            _guidProvider = guidProvider ?? throw new ArgumentNullException(nameof(guidProvider));
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public override object SyncRoot => this;

        protected override string Protocol => "smtp";

        public override HashSet<string>? AuthenticationMechanisms => default;

        public override int Timeout
        {
            get => default;
            set { }
        }

        public override bool IsConnected => true;

        public override bool IsSecure => false;

        public override bool IsAuthenticated => true;

        private static void AddUnique(IList<MailboxAddress> recipients, HashSet<string> unique, IEnumerable<MailboxAddress> mailboxes)
        {
            foreach (var mailbox in mailboxes)
                if (unique.Add(mailbox.Address))
                    recipients.Add(mailbox);
        }

        private static MailboxAddress GetMessageSender(MimeMessage message)
        {
            if (message.ResentSender != null)
                return message.ResentSender;

            if (message.ResentFrom.Count > 0)
                return message.ResentFrom.Mailboxes.FirstOrDefault();

            if (message.Sender != null)
                return message.Sender;

            return message.From.Mailboxes.FirstOrDefault();
        }

        private static IList<MailboxAddress> GetMessageRecipients(MimeMessage message)
        {
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var recipients = new List<MailboxAddress>();

            if (message.ResentSender != null || message.ResentFrom.Count > 0)
            {
                AddUnique(recipients, unique, message.ResentTo.Mailboxes);
                AddUnique(recipients, unique, message.ResentCc.Mailboxes);
                AddUnique(recipients, unique, message.ResentBcc.Mailboxes);
            }
            else
            {
                AddUnique(recipients, unique, message.To.Mailboxes);
                AddUnique(recipients, unique, message.Cc.Mailboxes);
                AddUnique(recipients, unique, message.Bcc.Mailboxes);
            }

            return recipients;
        }

        private async Task WriteAsync(FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress? progress = null)
        {
            var format = options.Clone();
            format.HiddenHeaders.Add(HeaderId.ContentLength);
            format.HiddenHeaders.Add(HeaderId.ResentBcc);
            format.HiddenHeaders.Add(HeaderId.Bcc);

            // prepare the message
            message.Prepare(EncodingConstraint.SevenBit);

            var retryCount = 4;
            for (; ; )
            {
                var filePath = Path.Combine(_path, _guidProvider.NewGuid().ToString() + ".eml");

                FileStream fs;
                try { fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 1024, useAsync: true); }
                catch when (--retryCount >= 0) { continue; }

                await using (fs.ConfigureAwait(false))
                {
                    await message.WriteToAsync(format, fs, cancellationToken).ConfigureAwait(false);

                    if (progress != null)
                    {
                        var numWritten = fs.Length;
                        progress.Report(numWritten, numWritten);
                    }

                    return;
                }
            }
        }

        public override void Send(FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress? progress = null)
        {
            SendAsync(options, message, cancellationToken, progress).GetAwaiter().GetResult();
        }

        public override Task SendAsync(FormatOptions options, MimeMessage message, CancellationToken cancellationToken = default, ITransferProgress? progress = null)
        {
            var recipients = GetMessageRecipients(message);
            var sender = GetMessageSender(message);

            if (sender == null)
                throw new InvalidOperationException("No sender has been specified.");

            if (recipients.Count == 0)
                throw new InvalidOperationException("No recipients have been specified.");

            return WriteAsync(options, message, cancellationToken, progress);
        }

        public override void Send(FormatOptions options, MimeMessage message, MailboxAddress sender, IEnumerable<MailboxAddress> recipients, CancellationToken cancellationToken = default, ITransferProgress? progress = null)
        {
            SendAsync(options, message, sender, recipients, cancellationToken, progress).GetAwaiter().GetResult();
        }

        public override Task SendAsync(FormatOptions options, MimeMessage message, MailboxAddress sender, IEnumerable<MailboxAddress> recipients, CancellationToken cancellationToken = default, ITransferProgress? progress = null)
        {
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var rcpts = new List<MailboxAddress>();

            AddUnique(rcpts, unique, recipients);

            if (rcpts.Count == 0)
                throw new ArgumentException("No recipients have been specified.", nameof(recipients));

            // cloning message
            using (var ms = new MemoryStream())
            {
                message.WriteTo(ms, cancellationToken);
                ms.Position = 0;
                message = MimeMessage.Load(ms, cancellationToken);
            }

            message.From.Clear();
            message.ResentFrom.Clear();
            message.Sender = null;
            message.ResentSender = null;

            message.From.Add(sender);

            message.To.Clear();
            message.ResentTo.Clear();
            message.Cc.Clear();
            message.ResentCc.Clear();
            message.Bcc.Clear();
            message.ResentBcc.Clear();

            message.To.AddRange(rcpts);

            return WriteAsync(options, message, cancellationToken, progress);
        }

        public override void Connect(string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default) { }
        public override Task ConnectAsync(string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public override void Connect(Socket socket, string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default) { }
        public override Task ConnectAsync(Socket socket, string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public override void Connect(Stream stream, string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default) { }
        public override Task ConnectAsync(Stream stream, string host, int port = 0, SecureSocketOptions options = SecureSocketOptions.Auto, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public override void Authenticate(Encoding encoding, ICredentials credentials, CancellationToken cancellationToken = default) { }
        public override Task AuthenticateAsync(Encoding encoding, ICredentials credentials, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public override void Authenticate(SaslMechanism mechanism, CancellationToken cancellationToken = default) { }
        public override Task AuthenticateAsync(SaslMechanism mechanism, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public override void Disconnect(bool quit, CancellationToken cancellationToken = default) { }
        public override Task DisconnectAsync(bool quit, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public override void NoOp(CancellationToken cancellationToken = default) { }
        public override Task NoOpAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
