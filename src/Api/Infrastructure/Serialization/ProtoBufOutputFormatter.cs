using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace WebApp.Api.Infrastructure.Serialization
{
    // based on: https://github.com/dotnet/aspnetcore/blob/v3.1.18/src/Mvc/Mvc.NewtonsoftJson/src/NewtonsoftJsonOutputFormatter.cs
    public sealed class ProtoBufOutputFormatter : OutputFormatter
    {
        private readonly MvcOptions _mvcOptions;

        public ProtoBufOutputFormatter(ProtoBufFormatterOptions options, MvcOptions mvcOptions)
        {
            _mvcOptions = mvcOptions;

            foreach (var contentType in options.SupportedContentTypes)
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(contentType));
        }

        protected override bool CanWriteType(Type type) => ApiContractSerializer.MetadataProvider.CanSerialize(type);

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var response = context.HttpContext.Response;

            var writeStream = response.Body;
            FileBufferingWriteStream? fileBufferingWriteStream = null;

            if (!_mvcOptions.SuppressOutputFormatterBuffering)
                writeStream = fileBufferingWriteStream = new FileBufferingWriteStream();

            await using (fileBufferingWriteStream)
            {
                ApiContractSerializer.ProtoBuf.Serialize(writeStream, context.Object, context.ObjectType);

                if (fileBufferingWriteStream != null)
                {
                    response.ContentLength = fileBufferingWriteStream.Length;
                    await fileBufferingWriteStream.DrainBufferAsync(response.Body);
                }
            }
        }
    }
}

