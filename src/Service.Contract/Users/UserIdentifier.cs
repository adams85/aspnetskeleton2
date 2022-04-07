using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service.Users
{
    [DataContract]
    [ProtoInclude(1, typeof(Key))]
    [ProtoInclude(2, typeof(Name))]
    [ProtoInclude(3, typeof(Email))]
    public abstract record class UserIdentifier
    {
        [DataContract]
        public record class Key : UserIdentifier
        {
            [DataMember(Order = 1)] public int Value { get; init; }
        }

        [DataContract]
        public record class Name : UserIdentifier
        {
            [Required]
            [DataMember(Order = 1)] public string Value { get; init; } = null!;
        }

        [DataContract]
        public record class Email : UserIdentifier
        {
            [Required]
            [DataMember(Order = 1)] public string Value { get; init; } = null!;
        }
    }
}
