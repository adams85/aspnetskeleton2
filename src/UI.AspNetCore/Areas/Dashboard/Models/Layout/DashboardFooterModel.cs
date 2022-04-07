using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.UI.Areas.Dashboard.Models.Layout
{
    public class DashboardFooterModel
    {
        private static readonly string? s_defaultCopyright = typeof(Program).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

        public Func<IUrlHelper, string>? GetApplicationUrl { get; init; }
        public string ApplicationName { get; init; } = Program.ApplicationName;
        public string ApplicationVersion { get; init; } = Program.ApplicationVersion;
        public string? Copyright { get; init; } = s_defaultCopyright;
    }
}
