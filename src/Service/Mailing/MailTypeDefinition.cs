using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WebApp.Service.Infrastructure.Serialization;

namespace WebApp.Service.Mailing
{
    internal abstract class MailTypeDefinition<TModel, TProducer> : IMailTypeDefinition
        where TModel : MailModel
        where TProducer : class, IMailMessageProducer
    {
        private static readonly ObjectFactory s_producerFactory = ActivatorUtilities.CreateFactory(typeof(TProducer), Type.EmptyTypes);

        protected MailTypeDefinition() { }

        public abstract string MailType { get; }

        public abstract Task ValidateModelAsync(MailModelValidationContext context, CancellationToken cancellationToken);

        public virtual byte[] SerializeModel(MailModel model) => InternalSerializer.MailModel.Serialize((TModel)model);
        public virtual MailModel DeserializeModel(byte[] bytes) => InternalSerializer.MailModel.Deserialize<TModel>(bytes)!;

        public virtual IMailMessageProducer CreateMailMessageProducer(IServiceProvider serviceProvider) => (TProducer)s_producerFactory(serviceProvider, null);
    }
}
