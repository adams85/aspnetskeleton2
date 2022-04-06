using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using MimeKit.Text;
using WebApp.Service.Infrastructure.Templating;

namespace WebApp.Service.Mailing
{
    internal abstract class MailMessageProducer<TModel> : IMailMessageProducer
        where TModel : MailModel
    {
        private readonly ITemplateRenderer _templateRenderer;

        protected MailMessageProducer(ITemplateRenderer templateRenderer)
        {
            _templateRenderer = templateRenderer;
        }

        protected abstract string GenerateSubject(TModel model);

        protected virtual string GetBodyTemplatePath(TModel model) => $"Mails/{model.MailType}";

        protected virtual Task<string> GenerateBodyAsync(TModel model, CancellationToken cancellationToken)
        {
            var templatePath = GetBodyTemplatePath(model);

            return _templateRenderer.RenderAsync(templatePath, model, CultureInfo.GetCultureInfo(model.Culture), CultureInfo.GetCultureInfo(model.UICulture), cancellationToken);
        }

        protected abstract string GetSender(TModel model);

        protected virtual string GetFrom(TModel model) => GetSender(model);

        protected abstract IEnumerable<string> GetTo(TModel model);

        protected virtual IEnumerable<string> GetCc(TModel model) => Enumerable.Empty<string>();

        protected virtual IEnumerable<string> GetBcc(TModel model) => Enumerable.Empty<string>();

        protected virtual bool IsBodyHtml => true;

        public async Task<MimeMessage> ProduceAsync(MailModel model, CancellationToken cancellationToken)
        {
            if (model is not TModel concreteModel)
                throw new ArgumentException($"Model instance is not compatible with type {typeof(TModel)}.", nameof(model));

            var result = new MimeMessage();

            result.Sender = MailboxAddress.Parse(GetSender(concreteModel));
            result.From.Add(MailboxAddress.Parse(GetFrom(concreteModel)));
            AddAddressesToCollection(result.To, GetTo(concreteModel));
            AddAddressesToCollection(result.Cc, GetCc(concreteModel));
            AddAddressesToCollection(result.Bcc, GetBcc(concreteModel));

            result.Subject = GenerateSubject(concreteModel);

            result.Body = new TextPart(IsBodyHtml ? TextFormat.Html : TextFormat.Plain)
            {
                Text = await GenerateBodyAsync(concreteModel, cancellationToken).ConfigureAwait(false)
            };

            return result;

            static void AddAddressesToCollection(InternetAddressList collection, IEnumerable<string> addresses)
            {
                foreach (var address in addresses)
                    collection.Add(MailboxAddress.Parse(address));
            }
        }
    }
}
