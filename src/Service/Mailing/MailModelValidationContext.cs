using System;
using WebApp.DataAccess;
using WebApp.Service.Infrastructure.Validation;

namespace WebApp.Service.Mailing;

internal class MailModelValidationContext : DynamicValidationContext<MailModel>
{
    public MailModelValidationContext(Func<string>? getBaseMemberPath = null)
        : base(getBaseMemberPath) { }

    public MailModel Model { get; init; } = null!;
    public WritableDataContext DbContext { get; init; } = null!;
}
