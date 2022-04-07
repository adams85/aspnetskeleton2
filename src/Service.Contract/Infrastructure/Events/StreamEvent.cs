using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service.Infrastructure.Events
{
    /// <summary>
    /// Base class for special events which contain information about the state of an event stream.
    /// </summary>
    /// <remarks>
    /// <see cref="StreamBusEventsQuery"/>
    /// </remarks>
    [DataContract]
    [ProtoInclude(1, typeof(Init))]
    public abstract record class StreamEvent : Event
    {
        /// <summary>
        /// Event which is sent before the actual events (to signal successful connection).
        /// </summary>
        [DataContract]
        public sealed record class Init : StreamEvent { }
    }
}
