using System.Runtime.Serialization;
using ProtoBuf;
using WebApp.Service.Infrastructure.Events;
using WebApp.Service.Settings;

namespace WebApp.Service
{
    [DataContract]
    [ProtoInclude(1, typeof(StreamEvent))]
    [ProtoInclude(2, typeof(ProgressEvent))]
    [ProtoInclude(3, typeof(SettingsChangedEvent))]
    public abstract class Event { }
}
