using System;

namespace WebApp.Common.Infrastructure.Localization
{
    /// <summary>
    /// Custom attribute which supports localizable text extraction from source code (POTools).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public sealed class LocalizedAttribute : Attribute
    {
        public string? PluralId { get; init; }
        public string? ContextId { get; init; }
    }
}
