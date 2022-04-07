using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebApp.Service;
using WebApp.Service.Settings;

namespace WebApp.Api.Infrastructure.ModelBinding;

public class ListQueryModelBinder : IModelBinder
{
    private readonly IModelBinder _complexObjectModelBinder;
    private readonly ISettingsProvider _settingsProvider;

    public ListQueryModelBinder(IModelBinder complexObjectModelBinder, ISettingsProvider settingsProvider)
    {
        _complexObjectModelBinder = complexObjectModelBinder;
        _settingsProvider = settingsProvider;
    }

    private void EnsurePaging(ModelBindingContext bindingContext)
    {
        if (bindingContext.Model is IListQuery listQuery)
            listQuery.EnsurePaging(_settingsProvider.MaxPageSize());
    }

    Task IModelBinder.BindModelAsync(ModelBindingContext bindingContext)
    {
        var bindModelTask = _complexObjectModelBinder.BindModelAsync(bindingContext);

        if (bindModelTask.IsCompletedSuccessfully)
        {
            EnsurePaging(bindingContext);
            return Task.CompletedTask;
        }
        else
        {
            return Await(bindModelTask, bindingContext);
        }

        async Task Await(Task bindModelTask, ModelBindingContext bindingContext)
        {
            await bindModelTask;
            EnsurePaging(bindingContext);
        }
    }
}
