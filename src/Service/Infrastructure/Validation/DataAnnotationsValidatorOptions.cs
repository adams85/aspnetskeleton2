using System;

namespace WebApp.Service.Infrastructure.Validation
{
    [Flags]
    public enum DataAnnotationsValidatorOptions
    {
        None = 0,
        SuppressImplicitRequiredAttributeForNonNullableRefTypes = 0x1
    }
}
