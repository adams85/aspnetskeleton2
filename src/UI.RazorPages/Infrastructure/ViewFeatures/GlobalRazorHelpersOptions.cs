using System;
using System.Collections.Generic;

namespace WebApp.UI.Infrastructure.ViewFeatures;

public class GlobalRazorHelpersOptions
{
    public Dictionary<Type, string> HelpersTypeViewPathMappings { get; } = new Dictionary<Type, string>();
}
