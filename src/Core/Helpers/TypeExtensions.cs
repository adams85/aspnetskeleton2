using System;
using System.Collections.Concurrent;
using System.Reflection;
using Karambolo.Common;

namespace WebApp.Core.Helpers
{
    public static class TypeExtensions
    {
        private static readonly Assembly s_coreLibAssembly = typeof(object).Assembly;

        private static readonly ConcurrentDictionary<Type, string> s_fullNameCache = new ConcurrentDictionary<Type, string>();
        private static readonly ConcurrentDictionary<Type, string?> s_associatedAssemblyNameCache = new ConcurrentDictionary<Type, string?>();

        private static string? GetSimpleAssemblyName(Assembly assembly) =>
            assembly != s_coreLibAssembly ? assembly.GetName().Name : null;

        public static string FullNameWithoutAssemblyDetails(this Type type) =>
            s_fullNameCache.GetOrAdd(type, type => new TypeNameBuilder(type, GetSimpleAssemblyName) { AssemblyName = null }.ToString());

        public static string AssemblyQualifiedNameWithoutAssemblyDetails(this Type type)
        {
            var fullName = type.FullNameWithoutAssemblyDetails();

            var simpleAssemblyName = GetSimpleAssemblyName(type.Assembly);
            if (simpleAssemblyName != null)
                fullName += ", " + simpleAssemblyName;

            return fullName;
        }

        public static string? GetAssociatedAssemblyName(this Type type) =>
            s_associatedAssemblyNameCache.GetOrAdd(type, type => type.GetCustomAttribute<AssociatedAssemblyNameAttribute>()?.AssemblyName.Name);
    }
}
