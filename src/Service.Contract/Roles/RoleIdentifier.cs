using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service.Users
{
    [DataContract]
    [ProtoInclude(1, typeof(Key))]
    [ProtoInclude(2, typeof(Name))]
    public abstract class RoleIdentifier
    {
        [DataContract]
        public class Key : RoleIdentifier
        {
            [DataMember(Order = 1)] public int Value { get; set; }
        }

        [DataContract]
        public class Name : RoleIdentifier
        {
            [Required]
            [DataMember(Order = 1)] public string Value { get; set; } = null!;
        }
    }
}
