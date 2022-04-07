using System.Collections.Generic;

namespace WebApp.Api.Infrastructure.Serialization;

public class ProtoBufFormatterOptions
{
    public const string DefaultContentType = "application/x-protobuf";

    public HashSet<string> SupportedContentTypes { get; } = new HashSet<string> { DefaultContentType, "application/protobuf", "application/x-google-protobuf" };
    public HashSet<string> SupportedExtensions { get; } = new HashSet<string> { "proto" };
}
