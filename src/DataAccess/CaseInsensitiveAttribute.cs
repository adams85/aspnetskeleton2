using System;

namespace WebApp.DataAccess
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CaseInsensitiveAttribute : Attribute { }
}
