using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Karambolo.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;

namespace WebApp.Api.Infrastructure.ModelBinding
{
    /// <summary>
    /// Makes <see cref="IModelMetadataProvider"/> respect <see cref="DataContractAttribute"/> and <see cref="DataMemberAttribute"/>.
    /// </summary>
    /// <remarks>
    /// This class is necessary for correct Swagger JSON generation as operation parameters are generated based on model metadata.
    /// </remarks>
    public sealed class CustomModelMetadataProvider : DefaultModelMetadataProvider
    {
        public CustomModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider) : base(detailsProvider) { }

        public CustomModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor) : base(detailsProvider, optionsAccessor) { }

        private DefaultMetadataDetails CreateSinglePropertyDetails(ModelMetadataIdentity propertyKey, PropertyInfo property)
        {
            var containerType = propertyKey.ContainerType;

            var attributes = ModelAttributes.GetAttributesForProperty(containerType, property, propertyKey.ModelType);

            var propertyEntry = new DefaultMetadataDetails(propertyKey, attributes);
            if (property.CanRead && property.GetMethod?.IsPublic == true)
                propertyEntry.PropertyGetter = property.MakeFastGetter<object, object>();

            if (property.CanWrite && property.SetMethod?.IsPublic == true && !containerType.IsValueType)
                propertyEntry.PropertySetter = property.MakeFastSetter<object, object>();

            return propertyEntry;
        }

        protected override DefaultMetadataDetails[] CreatePropertyDetails(ModelMetadataIdentity key)
        {
            if (!ApiContractSerializer.MetadataProvider.CanSerialize(key.ModelType))
                return base.CreatePropertyDetails(key);

            var metaMembers = ApiContractSerializer.MetadataProvider.GetMembers(key.ModelType, out var metaType);
            if (metaType == null)
                return base.CreatePropertyDetails(key);

            var propertyEntries = new List<DefaultMetadataDetails>();

            foreach (var metaMember in metaMembers)
                if (metaMember.Member is PropertyInfo property)
                {
                    var propertyKey = ModelMetadataIdentity.ForProperty(property, property.PropertyType, key.ModelType);
                    var propertyEntry = CreateSinglePropertyDetails(propertyKey, property);

                    if (property.Name != metaMember.Name)
                        propertyEntry.BindingMetadata = new BindingMetadata { BinderModelName = metaMember.Name };

                    propertyEntries.Add(propertyEntry);
                }

            return propertyEntries.ToArray();
        }
    }
}
