using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Karambolo.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApp.UI.Models.DataTables
{
    public sealed class DataTableColumnBinding
    {
        public static DataTableColumnBinding For<TItem>() => new DataTableColumnBinding(typeof(TItem), Array.Empty<PropertyInfo>(), Identity<object>.Func);

        public static DataTableColumnBinding For<TItem>(Expression<Func<TItem, object?>> pathExpression)
        {
            var propertyPath = Lambda.GetMemberPath(pathExpression).Cast<PropertyInfo>().ToArray();
            var valueAccessor = pathExpression.Compile();
            return new DataTableColumnBinding(typeof(TItem), propertyPath, item => valueAccessor((TItem)item));
        }

        private readonly Func<object, object?> _valueAccessor;

        private DataTableColumnBinding(Type itemType, IReadOnlyList<PropertyInfo> propertyPath, Func<object, object?> valueAccessor)
        {
            ItemType = itemType;
            PropertyPath = propertyPath;
            PropertyPathString = string.Join('.', PropertyPath.Select(property => property.Name));
            _valueAccessor = valueAccessor;
        }

        public Type ItemType { get; }

        public IReadOnlyList<PropertyInfo> PropertyPath { get; }

        public string PropertyPathString { get; }

        public string? GetDisplayName(IModelMetadataProvider modelMetadataProvider)
        {
            var modelMetadata =
                PropertyPath.Count > 0 ?
                modelMetadataProvider.GetMetadataForProperty(PropertyPath.Count > 1 ? PropertyPath[^2].PropertyType : ItemType, PropertyPath[^1].Name) :
                modelMetadataProvider.GetMetadataForType(ItemType);

            return modelMetadata.DisplayName;
        }

        public object? GetValue(object item) => _valueAccessor.Invoke(item);
    }
}
