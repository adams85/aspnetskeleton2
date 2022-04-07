using System;
using System.Reflection;

namespace WebApp.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public sealed class AssociatedAssemblyNameAttribute : Attribute
{
    public AssociatedAssemblyNameAttribute(string assemblyName)
    {
        AssemblyName = new AssemblyName(assemblyName);
    }

    public AssemblyName AssemblyName { get; }
}
