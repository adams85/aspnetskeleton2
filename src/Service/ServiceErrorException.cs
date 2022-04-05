using System;
using System.ComponentModel;
using System.Reflection;

namespace WebApp.Service
{
    public partial class ServiceErrorException : ApplicationException
    {
        internal static ServiceErrorException From(ServiceErrorData data) =>
            new ServiceErrorException(data.ErrorCode, data.Args != null ? Array.ConvertAll(data.Args, arg => arg.ValueUntyped) : null);

        internal ServiceErrorException(ServiceErrorCode errorCode)
            : this(errorCode, null) { }

        internal ServiceErrorException(ServiceErrorCode errorCode, params object[]? args)
        {
            ErrorCode = errorCode;
            Args = args ?? Array.Empty<object>();
        }

        public ServiceErrorCode ErrorCode { get; }
        public object[] Args { get; }

        private string GetDefaultErrorDescription() => $"Operation failed with error code {ErrorCode}.";

        public string GetErrorDescription()
        {
            var name = Enum.GetName(typeof(ServiceErrorCode), ErrorCode);
            if (name == null)
                return GetDefaultErrorDescription();

            var field = typeof(ServiceErrorCode).GetField(name);

            var description = field!.GetCustomAttribute<DescriptionAttribute>()?.Description;
            return description != null ? string.Format(description, Args) : GetDefaultErrorDescription();
        }

        public sealed override string Message => GetErrorDescription();

        public ServiceErrorData ToData() => new ServiceErrorData
        {
            ErrorCode = ErrorCode,
            Args = Array.ConvertAll(Args, ServiceErrorArgData.From)
        };
    }
}
