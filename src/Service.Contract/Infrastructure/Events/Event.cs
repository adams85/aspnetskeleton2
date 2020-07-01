using System.Runtime.Serialization;
using ProtoBuf;
using WebApp.Service.Infrastructure.Events;
using WebApp.Service.Settings;
using WebApp.Service.Translations;

namespace WebApp.Service
{
    [DataContract]
    [ProtoInclude(1, typeof(StreamEvent))]
    [ProtoInclude(2, typeof(ProgressEvent))]
    [ProtoInclude(3, typeof(SettingsChangedEvent))]
    [ProtoInclude(4, typeof(TranslationsChangedEvent))]
    public abstract class Event { }
}
