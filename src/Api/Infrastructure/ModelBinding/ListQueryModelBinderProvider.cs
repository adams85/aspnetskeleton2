using System;
using Karambolo.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using WebApp.Service;
using WebApp.Service.Settings;

namespace WebApp.Api.Infrastructure.ModelBinding;

/// <summary>
/// Ensures paging in list queries.
/// </summary>
/// <remarks>
/// An instance of this class must be added to the <see cref="MvcOptions.ModelBinderProviders"/> list and it must be placed before
/// the built-in <see cref="ComplexTypeModelBinderProvider"/>.
/// </remarks>
public class ListQueryModelBinderProvider : IModelBinderProvider
{
    private readonly IModelBinderProvider _complexObjectModelBinderProvider;
    private readonly ISettingsProvider _settingsProvider;

    public ListQueryModelBinderProvider(IModelBinderProvider complexObjectModelBinderProvider, ISettingsProvider settingsProvider)
    {
        _complexObjectModelBinderProvider = complexObjectModelBinderProvider;
        _settingsProvider = settingsProvider;
    }

    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (context.Metadata.IsComplexType && !context.Metadata.IsCollectionType && context.Metadata.ModelType.HasInterface(typeof(IListQuery)))
            return new ListQueryModelBinder(_complexObjectModelBinderProvider.GetBinder(context)!, _settingsProvider);

        return null;
    }
}
