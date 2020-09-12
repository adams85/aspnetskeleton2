using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Karambolo.Common;
using WebApp.Core.Helpers;

namespace WebApp.Service.Infrastructure.Validation
{
    /// <summary>
    /// A helper class for basic (data annotations based) validation of service data objects (especially command/query objects).
    /// </summary>
    /// <remarks>
    /// The implementation is based on <see cref="Validator"/> and is supposed to match the behavior of ASP.NET Core MVC's model validation.
    /// It recursively traverses the object graph and besides the data annotiation attributes it also respects nullable reference type annotations.
    /// NNRT support has some limitations though: nullability of array/collection elements or properties of generic types are not checked (in accordance with MVC model validation).
    /// These issues can be worked around by applying data annotation attributes to the problematic properties.
    /// </remarks>
    public static class DataAnnotationsValidator
    {
        private const string MemberPathBaseExceptionDataKey = "MemberPathBase";

        private static readonly RequiredAttribute s_implicitRequiredAttribute = new RequiredAttribute() { AllowEmptyStrings = true };

        private static ValidationException CreateValidationException(ValidationResult validationResult, ValidationAttribute? validationAttribute, object? value, string memberPathBase) =>
            new ValidationException(validationResult, validationAttribute, value)
            {
                Data = { [MemberPathBaseExceptionDataKey] = memberPathBase }
            };

        public static string GetMemberPath(this ValidationException exception, string? memberName = null)
        {
            var memberPathBase = exception.Data?[MemberPathBaseExceptionDataKey] as string ?? string.Empty;
            return string.IsNullOrEmpty(memberName) ? memberPathBase : GetPropertyMemberPath(memberPathBase, memberName);
        }

        internal static string GetItemMemberPath(string memberPathBase, int index) => memberPathBase + "[" + index + "]";

        internal static string GetPropertyMemberPath(string memberPathBase, string propertyName) => memberPathBase.Length > 0 ? memberPathBase + "." + propertyName : propertyName;

        private static void ResetValidationContext(ValidationContext validationContext, string? memberName)
        {
            validationContext.MemberName = memberName;
            // display name should be gathered from metadata, but it is irrelevant in our use case
            validationContext.DisplayName = validationContext.MemberName ?? validationContext.ObjectType.Name;
        }

        private static void ValidateProperty(ValidationContext validationContext, PropertyMetadata propertyMetadata, string memberPathBase, int level, DataAnnotationsValidatorOptions options,
            ref List<(PropertyMetadata, object)>? propertiesToVisit)
        {
            ResetValidationContext(validationContext, propertyMetadata.PropertyName);

            var value = propertyMetadata.ValueAccessor(validationContext.ObjectInstance);
            ValidationResult validationResult;

            RequiredAttribute? requiredAttribute = propertyMetadata.ValidationAttributes.OfType<RequiredAttribute>().FirstOrDefault();

            if (requiredAttribute == null &&
                (options & DataAnnotationsValidatorOptions.SuppressImplicitRequiredAttributeForNonNullableRefTypes) == 0 &&
                propertyMetadata.IsNonNullableRefType)
                requiredAttribute = s_implicitRequiredAttribute;

            if (requiredAttribute != null)
            {
                validationResult = requiredAttribute.GetValidationResult(value, validationContext);
                if (validationResult != ValidationResult.Success)
                    throw CreateValidationException(validationResult, requiredAttribute, value, memberPathBase);
            }

            for (int i = 0, n = propertyMetadata.ValidationAttributes.Count; i < n; i++)
            {
                var validationAttribute = propertyMetadata.ValidationAttributes[i];
                if (validationAttribute == requiredAttribute)
                    continue;

                validationResult = validationAttribute.GetValidationResult(value, validationContext);
                if (validationResult != ValidationResult.Success)
                    throw CreateValidationException(validationResult, validationAttribute, value, memberPathBase);
            }

            if (ShouldVisitObject(value, level))
            {
                propertiesToVisit ??= new List<(PropertyMetadata, object)>();
                propertiesToVisit.Add((propertyMetadata, value));
            }
        }

        private static void ValidateObject(ValidationContext validationContext, TypeMetadata typeMetadata, string memberPathBase, DataAnnotationsValidatorOptions options)
        {
            ResetValidationContext(validationContext, null);

            for (int i = 0, n = typeMetadata.ValidationAttributes.Count; i < n; i++)
            {
                var validationAttribute = typeMetadata.ValidationAttributes[i];

                var validationResult = validationAttribute.GetValidationResult(validationContext.ObjectInstance, validationContext);
                if (validationResult != ValidationResult.Success)
                    throw CreateValidationException(validationResult, validationAttribute, validationContext.ObjectInstance, memberPathBase);
            }

            if (validationContext.ObjectInstance is IValidatableObject validatableObject)
            {
                var validationResult = validatableObject.Validate(validationContext)?
                    .FirstOrDefault(validationResult => validationResult != null && validationResult != ValidationResult.Success);

                if (validationResult != null)
                    throw CreateValidationException(validationResult, (validationResult as ExtendedValidationResult)?.ValidationAttribute, validationContext.ObjectInstance, memberPathBase);
            }
        }

        private static bool ShouldVisitObject(object? instance, int level)
        {
            if (instance == null || level <= 0)
                return false;

            var type = instance.GetType();
            if (type.IsValueType)
            {
                return
                    !type.IsPrimitive &&
                    !type.IsEnum &&
                    type != typeof(decimal) &&
                    type != typeof(DateTime) &&
                    type != typeof(DateTimeOffset) &&
                    type != typeof(TimeSpan) &&
                    type != typeof(Guid);
            }
            else
            {
                return
                    type != typeof(object) &&
                    type != typeof(string) &&
                    type != typeof(Uri) &&
                    !(instance is Delegate) &&
                    !(instance is MemberInfo);
            }
        }

        private static void VisitObject(object instance, IServiceProvider? serviceProvider, string memberPathBase, int level, DataAnnotationsValidatorOptions options)
        {
            int i, n;

            switch (instance)
            {
                case IList list:
                    for (i = 0, n = list.Count; i < n; i++)
                    {
                        instance = list[i]!;
                        if (ShouldVisitObject(instance, level))
                            VisitObject(instance, serviceProvider, GetItemMemberPath(memberPathBase, i), level - 1, options);
                    }
                    return;
                case IEnumerable enumerable:
                    var enumerator = enumerable.GetEnumerator();
                    using (enumerator as IDisposable)
                        for (i = 0; enumerator.MoveNext(); i++)
                        {
                            instance = enumerator.Current!;
                            if (ShouldVisitObject(instance, level))
                                VisitObject(instance, serviceProvider, GetItemMemberPath(memberPathBase, i), level - 1, options);
                        }
                    return;
            }

            // contrary to Validator, we re-use the ValidationContext instance to reduce GC pressure
            var validationContext = new ValidationContext(instance, serviceProvider, null);
            var typeMetadata = TypeMetadata.For(validationContext.ObjectType);

            List<(PropertyMetadata, object)>? propertiesToVisit = null;
            PropertyMetadata propertyMetadata;

            for (i = 0, n = typeMetadata.Properties.Count; i < n; i++)
            {
                propertyMetadata = typeMetadata.Properties[i];

                ValidateProperty(validationContext, propertyMetadata, memberPathBase, level, options, ref propertiesToVisit);
            }

            ValidateObject(validationContext, typeMetadata, memberPathBase, options);

            if (propertiesToVisit != null)
                for (i = 0, n = propertiesToVisit.Count; i < n; i++)
                {
                    (propertyMetadata, instance) = propertiesToVisit[i];
                    VisitObject(instance, serviceProvider, GetPropertyMemberPath(memberPathBase, propertyMetadata.PropertyName), level - 1, options);
                }
        }

        public static void Validate(object instance, IServiceProvider? serviceProvider = null, int maxDepth = 32, DataAnnotationsValidatorOptions options = DataAnnotationsValidatorOptions.None)
        {
            if (ShouldVisitObject(instance, maxDepth))
                VisitObject(instance, serviceProvider, string.Empty, maxDepth, options);
        }

        private sealed class PropertyMetadata
        {
            private static readonly ConcurrentDictionary<(Type, string), PropertyMetadata> s_cache = new ConcurrentDictionary<(Type, string), PropertyMetadata>();

            public static PropertyMetadata For(PropertyInfo property, Dictionary<Type, bool> nonNullableContextCache) =>
                s_cache.GetOrAdd((property.DeclaringType!, property.Name), _ => new PropertyMetadata(property, nonNullableContextCache));

            private PropertyMetadata(PropertyInfo property, Dictionary<Type, bool> nonNullableContextCache)
            {
                PropertyName = property.Name;

                IsNonNullableRefType =
                    property.IsNonNullableRefType() ??
                    // for properties declared on generic types determining reference type nullability is pretty hard (for root objects is impossible AFAIK),
                    // so we skip the check and treat these properties nullable (just as ASP.NET Core's DataAnnotationsMetadataProvider does)
                    !property.DeclaringType!.IsGenericType && property.DeclaringType.IsNonNullableContext(nonNullableContextCache);

                ValidationAttributes = (ValidationAttribute[])Attribute.GetCustomAttributes(property, typeof(ValidationAttribute));

                ValueAccessor = property.MakeFastGetter<object, object>();
            }

            public string PropertyName { get; }

            public bool IsNonNullableRefType { get; }

            public IReadOnlyList<ValidationAttribute> ValidationAttributes { get; }

            public Func<object, object> ValueAccessor { get; }
        }

        private sealed class TypeMetadata
        {
            private static readonly ConcurrentDictionary<Type, TypeMetadata> s_cache = new ConcurrentDictionary<Type, TypeMetadata>();

            public static TypeMetadata For(Type type) =>
                s_cache.GetOrAdd(type, type => new TypeMetadata(type));

            private TypeMetadata(Type type)
            {
                ValidationAttributes = (ValidationAttribute[])Attribute.GetCustomAttributes(type, typeof(ValidationAttribute));

                var nonNullableContextCache = new Dictionary<Type, bool>();

                Properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(property => property.DeclaringType != null && property.GetMethod != null && property.GetIndexParameters().Length == 0)
                    .Select(property => PropertyMetadata.For(property, nonNullableContextCache))
                    .ToArray();
            }

            public IReadOnlyList<ValidationAttribute> ValidationAttributes { get; }

            public IReadOnlyList<PropertyMetadata> Properties { get; }
        }
    }
}
