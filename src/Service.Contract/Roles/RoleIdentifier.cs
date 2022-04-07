using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service.Users
{
    [DataContract]
    [ProtoInclude(1, typeof(Key))]
    [ProtoInclude(2, typeof(Name))]
    public abstract record class RoleIdentifier
    {
        [DataContract]
        public record class Key : RoleIdentifier
        {
            [DataMember(Order = 1)] public int Value { get; init; }
        }

        [DataContract]
        public record class Name : RoleIdentifier
        {
            [Required]
            [DataMember(Order = 1)] public string Value { get; init; } = null!;
        }
    }
}
