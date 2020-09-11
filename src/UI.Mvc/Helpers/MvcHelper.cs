using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace WebApp.UI.Helpers
{
    public static class MvcHelper
    {
        private static readonly ConcurrentDictionary<Type, string> s_controllerNames = new ConcurrentDictionary<Type, string>();

        private static string GetControllerNameCore(Type controllerType) => s_controllerNames.GetOrAdd(controllerType, type =>
        {
            var name = type.Name;
            const string controllerPostfix = "Controller";
            return
                name.EndsWith(controllerPostfix) ?
                name.Substring(0, name.Length - controllerPostfix.Length) :
                name;
        });

        public static string GetControllerName(Type controllerType)
        {
            if (!controllerType.IsSubclassOf(typeof(ControllerBase)))
                throw new ArgumentException($"{controllerType} is not a subclass of {typeof(ControllerBase)}.", nameof(controllerType));

            return GetControllerNameCore(controllerType);
        }

        public static string GetControllerName<TController>() where TController : ControllerBase =>
            GetControllerNameCore(typeof(TController));

        public static  (string Action, string Controller, string Area) GetCurrentRouteValues(this HttpContext httpContext) =>
            ((string)httpContext.GetRouteValue("action"), (string)httpContext.GetRouteValue("controller"), (string?)httpContext.GetRouteValue("area") ?? string.Empty);
    }
}
