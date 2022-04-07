using System.Collections.ObjectModel;

namespace System.CodeDom.Compiler;

public class CompilerError
{
    public string? ErrorText { get; set; }
    public bool IsWarning { get; set; }
}

public class CompilerErrorCollection : Collection<CompilerError> { }
