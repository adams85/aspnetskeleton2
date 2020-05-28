using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using WebApp.Service.Infrastructure.Templating;

namespace WebApp.Service.Mailing.Users
{
    internal sealed class UnapprovedUserCreatedMailMessageProducer : MailMessageProducer<UnapprovedUserCreatedMailModel>
    {
        private readonly string _noReplyMailFrom;

        public UnapprovedUserCreatedMailMessageProducer(ITemplateRenderer templateRenderer, IOptions<MailingOptions> mailingOptions)
            : base(templateRenderer)
        {
            if (mailingOptions?.Value == null)
                throw new ArgumentNullException(nameof(mailingOptions));

            _noReplyMailFrom =
                mailingOptions.Value.NoReplyMailFrom ??
                throw new ArgumentException($"{nameof(MailingOptions.NoReplyMailFrom)} must be specified.", nameof(mailingOptions));
        }

        protected override string GetSender(UnapprovedUserCreatedMailModel model) => _noReplyMailFrom;

        protected override IEnumerable<string> GetTo(UnapprovedUserCreatedMailModel model) => new[] { model.Email };

        protected override string GenerateSubject(UnapprovedUserCreatedMailModel model) => "Account Verification";
    }
}
