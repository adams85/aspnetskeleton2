using Grpc.Core;

namespace WebApp.Service.Host;

public static class ServiceHostConstants
{
    public const string IdentityAuthenticationTypeHeaderName = "Identity-Authentication-Type";
    public const string IdentityNameHeaderName = "Identity-Name" + Metadata.BinaryHeaderSuffix;
    public const string CultureNameHeaderName = "Culture-Name";
    public const string UICultureNameHeaderName = "UI-Culture-Name";
}
