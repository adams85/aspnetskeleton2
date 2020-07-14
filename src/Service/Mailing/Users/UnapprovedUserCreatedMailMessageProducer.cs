using System.Collections.Generic;
using WebApp.Service.Infrastructure.Templating;
using WebApp.Service.Settings;

namespace WebApp.Service.Mailing.Users
{
    internal sealed class UnapprovedUserCreatedMailMessageProducer : MailMessageProducer<UnapprovedUserCreatedMailModel>
    {
        private readonly string _sender;

        public UnapprovedUserCreatedMailMessageProducer(ITemplateRenderer templateRenderer, ISettingsProvider settingsProvider)
            : base(templateRenderer)
        {
            _sender = settingsProvider.NoReplyMailAddress();
        }

        protected override string GetSender(UnapprovedUserCreatedMailModel model) => _sender;

        protected override IEnumerable<string> GetTo(UnapprovedUserCreatedMailModel model) => new[] { model.Email };

        protected override string GenerateSubject(UnapprovedUserCreatedMailModel model) => "Account Verification";
    }
}
