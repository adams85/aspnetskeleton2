using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore
{
    public static class ModelBuilderExtensions
    {
        internal const string CaseInsensitiveAnnotationKey = "CaseInsensitive";

        public static PropertyBuilder<T> CaseInsensitive<T>(this PropertyBuilder<T> builder)
        {
            return builder.HasAnnotation(CaseInsensitiveAnnotationKey, true);
        }
    }
}
