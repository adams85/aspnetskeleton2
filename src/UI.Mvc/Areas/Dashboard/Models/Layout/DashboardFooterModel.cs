using System;
using System.Linq;
using System.Reflection;
using Karambolo.Common;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.UI.Areas.Dashboard.Models.Layout
{
    public class DashboardFooterModel
    {
        private static readonly string? s_defaultCopyright = typeof(Program).Assembly.GetAttributes<AssemblyCopyrightAttribute>().FirstOrDefault()?.Copyright;

        public Func<IUrlHelper, string>? GetApplicationUrl { get; set; }
        public string ApplicationName { get; set; } = Program.ApplicationName;
        public string ApplicationVersion { get; set; } = Program.ApplicationVersion;
        public string? Copyright { get; set; } = s_defaultCopyright;
    }
}
