using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebApp.Api.Infrastructure.ModelBinding;

/// <summary>
/// Makes <see cref="IModelMetadataProvider"/> respect <see cref="DataContractAttribute"/> and <see cref="DataMemberAttribute"/>.
/// </summary>
/// <remarks>
/// This class is necessary for correct Swagger JSON generation as operation parameters are generated based on model metadata.
/// An instance of this class must be added to the <see cref="MvcOptions.ModelMetadataDetailsProviders"/> list and it must be placed after
/// the built-in <see cref="DefaultBindingMetadataProvider"/> and <see cref="DefaultValidationMetadataProvider"/> (preferably, at the end of the list).
/// </remarks>
public sealed class DataContractMetadataDetailsProvider : IBindingMetadataProvider, IValidationMetadataProvider
{
    private static readonly IPropertyValidationFilter s_validateNeverFilter = new ValidateNeverAttribute();

    private static bool IsSerializedProperty(ModelMetadataIdentity propertyKey)
    {
        var property = propertyKey.PropertyInfo!;

        foreach (var member in ApiContractSerializer.MetadataProvider.GetMembers(propertyKey.ContainerType!, out var _))
            if (member.Member.HasSameMetadataDefinitionAs(property))
                return true;

        return false;
    }

    public void CreateBindingMetadata(BindingMetadataProviderContext context)
    {
        if (context.Key.MetadataKind == ModelMetadataKind.Property && context.BindingMetadata.IsBindingAllowed &&
            ApiContractSerializer.MetadataProvider.CanSerialize(context.Key.ContainerType!) && !IsSerializedProperty(context.Key))
        {
            context.BindingMetadata.IsBindingAllowed = false;
        }
    }

    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        if (context.Key.MetadataKind == ModelMetadataKind.Property &&
            ApiContractSerializer.MetadataProvider.CanSerialize(context.Key.ContainerType!) && !IsSerializedProperty(context.Key))
        {
            context.ValidationMetadata.PropertyValidationFilter = s_validateNeverFilter;
        }
    }
}
