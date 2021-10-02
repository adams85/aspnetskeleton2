using System;

namespace WebApp.UI.Models
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class StaticPageDescriptorProviderAttribute : Attribute
    {
        public StaticPageDescriptorProviderAttribute(string memberName)
        {
            MemberName = memberName;
        }

        public string MemberName { get; }
    }
}
