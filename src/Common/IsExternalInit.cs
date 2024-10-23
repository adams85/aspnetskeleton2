// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// source: https://github.com/dotnet/runtime/blob/v8.0.10/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/IsExternalInit.cs

using System.ComponentModel;

namespace System.Runtime.CompilerServices;

#if !NET5_0_OR_GREATER
/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit
{
}
#endif
