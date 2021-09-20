using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Logging;
using WebApp.Service;
using WebApp.Service.Settings;

namespace WebApp.Api.Infrastructure.ModelBinding
{
    public class ListQueryModelBinder : ComplexTypeModelBinder, IModelBinder
    {
        private readonly ISettingsProvider _settingsProvider;

        public ListQueryModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders, ISettingsProvider settingsProvider, ILoggerFactory loggerFactory, bool allowValidatingTopLevelNodes)
            : base(propertyBinders, loggerFactory, allowValidatingTopLevelNodes)
        {
            _settingsProvider = settingsProvider;
        }

        private void EnsurePaging(ModelBindingContext bindingContext)
        {
            if (bindingContext.Model is IListQuery listQuery)
                listQuery.EnsurePaging(_settingsProvider.MaxPageSize());
        }

        Task IModelBinder.BindModelAsync(ModelBindingContext bindingContext)
        {
            var bindModelTask = BindModelAsync(bindingContext);

            if (bindModelTask.IsCompletedSuccessfully)
            {
                EnsurePaging(bindingContext);
                return Task.CompletedTask;
            }
            else
                return Await(bindModelTask, bindingContext);

            async Task Await(Task bindModelTask, ModelBindingContext bindingContext)
            {
                await bindModelTask;
                EnsurePaging(bindingContext);
            }
        }
    }
}
