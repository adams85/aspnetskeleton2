using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;
using ProtoBuf;

namespace WebApp.Api.Infrastructure.Serialization
{
    // based on: https://github.com/dotnet/aspnetcore/blob/v3.1.6/src/Mvc/Mvc.NewtonsoftJson/src/NewtonsoftJsonInputFormatter.cs
    public sealed class ProtoBufInputFormatter : InputFormatter
    {
        private const int DefaultMemoryThreshold = 1024 * 30;

        private readonly MvcOptions _mvcOptions;
        private readonly ILogger _logger;

        public ProtoBufInputFormatter(ProtoBufFormatterOptions options, MvcOptions mvcOptions, ILogger<ProtoBufInputFormatter> logger)
        {
            _mvcOptions = mvcOptions;
            _logger = logger ?? (ILogger)NullLogger.Instance;

            foreach (var contentType in options.SupportedContentTypes)
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(contentType));
        }

        protected override bool CanReadType(Type type) => ApiContractSerializer.MetadataProvider.CanSerialize(type);

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;

            var readStream = request.Body;
            FileBufferingReadStream? fileBufferingReadStream = null;

            if (readStream.CanSeek)
            {
                var position = request.Body.Position;
                await readStream.DrainAsync(CancellationToken.None);
                readStream.Position = position;
            }
            else if (!_mvcOptions.SuppressOutputFormatterBuffering)
            {
                var memoryThreshold = DefaultMemoryThreshold;
                var contentLength = request.ContentLength.GetValueOrDefault();
                if (contentLength > 0 && contentLength < memoryThreshold)
                    memoryThreshold = (int)contentLength;

                readStream = fileBufferingReadStream = new FileBufferingReadStream(request.Body, memoryThreshold);
            }

            object? model = null;
            Exception? exception = null;

            await using (fileBufferingReadStream)
            {
                if (fileBufferingReadStream != null)
                {
                    await readStream.DrainAsync(CancellationToken.None);
                    readStream.Seek(0, SeekOrigin.Begin);
                }

                try { model = ApiContractSerializer.ProtoBuf.Deserialize(readStream, context.ModelType); }
                catch (ProtoException ex) { exception = ex; }
            }

            if (exception != null)
            {
                _logger.LogDebug(exception, "ProtoBuf input formatter threw an exception.");
                return InputFormatterResult.Failure();
            }

            return InputFormatterResult.Success(model);
        }
    }
}
