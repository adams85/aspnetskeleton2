using System.Collections.Generic;
using WebApp.Service.Infrastructure.Templating;
using WebApp.Service.Settings;

namespace WebApp.Service.Mailing.Users
{
    internal sealed class PasswordResetMailMessageProducer : MailMessageProducer<PasswordResetMailModel>
    {
        private readonly string _sender;

        public PasswordResetMailMessageProducer(ITemplateRenderer templateRenderer, ISettingsProvider settingsProvider)
            : base(templateRenderer)
        {
            _sender = settingsProvider.NoReplyMailAddress();
        }

        protected override string GetSender(PasswordResetMailModel model) => _sender;

        protected override IEnumerable<string> GetTo(PasswordResetMailModel model) => new[] { model.Email };

        protected override string GenerateSubject(PasswordResetMailModel model) => "Password Reset";
    }
}
