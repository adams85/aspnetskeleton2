using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using WebApp.Service.Infrastructure.Templating;

namespace WebApp.Service.Mailing.Users
{
    internal sealed class PasswordResetMailMessageProducer : MailMessageProducer<PasswordResetMailModel>
    {
        private readonly string _noReplyMailFrom;

        public PasswordResetMailMessageProducer(ITemplateRenderer templateRenderer, IOptions<MailingOptions> mailingOptions)
            : base(templateRenderer)
        {
            if (mailingOptions?.Value == null)
                throw new ArgumentNullException(nameof(mailingOptions));

            _noReplyMailFrom =
                mailingOptions.Value.NoReplyMailFrom ??
                throw new ArgumentException($"{nameof(MailingOptions.NoReplyMailFrom)} must be specified.", nameof(mailingOptions));
        }

        protected override string GetSender(PasswordResetMailModel model) => _noReplyMailFrom;

        protected override IEnumerable<string> GetTo(PasswordResetMailModel model) => new[] { model.Email };

        protected override string GenerateSubject(PasswordResetMailModel model) => "Password Reset";
    }
}
