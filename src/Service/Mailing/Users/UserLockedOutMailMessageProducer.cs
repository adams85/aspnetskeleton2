using System.Collections.Generic;
using WebApp.Service.Infrastructure.Templating;
using WebApp.Service.Settings;

namespace WebApp.Service.Mailing.Users;

internal sealed class UserLockedOutMailMessageProducer : MailMessageProducer<UserLockedOutMailModel>
{
    private readonly string _sender;

    public UserLockedOutMailMessageProducer(ITemplateRenderer templateRenderer, ISettingsProvider settingsProvider)
        : base(templateRenderer)
    {
        _sender = settingsProvider.NoReplyMailAddress();
    }

    protected override string GetSender(UserLockedOutMailModel model) => _sender;

    protected override IEnumerable<string> GetTo(UserLockedOutMailModel model) => new[] { model.Email };

    protected override string GenerateSubject(UserLockedOutMailModel model) => "Account Lockout";
}
