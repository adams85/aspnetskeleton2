﻿using System;
using System.Collections.Generic;
using Karambolo.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApp.Service;
using WebApp.Service.Settings;

namespace WebApp.Api.Infrastructure.ModelBinding
{
    // based on: https://github.com/dotnet/aspnetcore/blob/v3.1.18/src/Mvc/Mvc.Core/src/ModelBinding/Binders/ComplexTypeModelBinderProvider.cs
    public class ListQueryModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.Metadata.IsComplexType && !context.Metadata.IsCollectionType && context.Metadata.ModelType.HasInterface(typeof(IListQuery)))
            {
                var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
                for (int i = 0, n = context.Metadata.Properties.Count; i < n; i++)
                {
                    var property = context.Metadata.Properties[i];
                    propertyBinders.Add(property, context.CreateBinder(property));
                }

                return new ListQueryModelBinder(
                    propertyBinders,
                    context.Services.GetRequiredService<ISettingsProvider>(),
                    context.Services.GetRequiredService<ILoggerFactory>(),
                    allowValidatingTopLevelNodes: true);
            }

            return null;
        }
    }
}
