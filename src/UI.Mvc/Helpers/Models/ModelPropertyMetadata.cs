using System;
using System.Linq.Expressions;
using Karambolo.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApp.UI.Helpers.Models
{
    public readonly struct ModelPropertyMetadata<TModel>
    {
        private readonly Func<TModel, object?> _valueAccessor;

        public ModelPropertyMetadata(Expression<Func<TModel, object?>> pathExpression)
        {
            PathExpression = pathExpression;
            Path = Lambda.MemberPath(pathExpression);
            _valueAccessor = pathExpression.Compile();
        }

        public Expression<Func<TModel, object?>> PathExpression { get; }
        public string Path { get; }

        private ModelMetadata GetModelMetadata(IModelMetadataProvider modelMetadataProvider)
        {
            var property = Lambda.Property(PathExpression);
            return modelMetadataProvider.GetMetadataForProperty(property.DeclaringType, property.Name);
        }

        public string? GetDisplayName(IModelMetadataProvider modelMetadataProvider) =>
            PathExpression != null ? GetModelMetadata(modelMetadataProvider).DisplayName : null;

        public object? GetValue(TModel model) => _valueAccessor?.Invoke(model);
    }
}
