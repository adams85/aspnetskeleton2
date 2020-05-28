using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace WebApp.Tests.Helpers
{
    public class OptionsProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _options;

        public OptionsProvider(params object[] options)
        {
            _options = options
                .Select(option => (option, optionType: option.GetType()))
                .ToDictionary(
                    item => typeof(IOptions<>).MakeGenericType(item.optionType),
                    item => Activator.CreateInstance(typeof(OptionsWrapper<>).MakeGenericType(item.optionType), item.option)!);
        }

        public object GetService(Type serviceType) =>
            _options.TryGetValue(serviceType, out var option) ?
            option :
            throw new ArgumentOutOfRangeException(nameof(serviceType));
    }
}
