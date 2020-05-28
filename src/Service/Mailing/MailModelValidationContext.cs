using System;
using WebApp.DataAccess;
using WebApp.Service.Infrastructure.Validation;

namespace WebApp.Service.Mailing
{
    internal class MailModelValidationContext : DynamicValidationContext<MailModel>
    {
        public MailModelValidationContext(Func<string>? getBaseMemberPath = null)
            : base(getBaseMemberPath) { }

        public MailModel Model { get; set; } = null!;
        public DataContext DbContext { get; set; } = null!;
    }
}
