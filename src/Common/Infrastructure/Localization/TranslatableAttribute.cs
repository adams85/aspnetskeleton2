using System;

namespace WebApp.Common.Infrastructure.Localization
{
    /// <summary>
    /// Custom attribute which supports localizable text extraction from source code (POTools).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum, Inherited = false)]
    public sealed class TranslatableAttribute : Attribute
    {
        public string? PluralId { get; set; }
        public string? TextContext { get; set; }
    }
}
