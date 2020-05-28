using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using ProtoBuf;

namespace WebApp.Service.Users
{
    [DataContract]
    [ProtoInclude(1, typeof(Key))]
    [ProtoInclude(2, typeof(Name))]
    [ProtoInclude(3, typeof(Email))]
    public abstract class UserIdentifier
    {
        [DataContract]
        public class Key : UserIdentifier
        {
            [DataMember(Order = 1)] public int Value { get; set; }
        }

        [DataContract]
        public class Name : UserIdentifier
        {
            [Required]
            [DataMember(Order = 1)] public string Value { get; set; } = null!;
        }

        [DataContract]
        public class Email : UserIdentifier
        {
            [Required]
            [DataMember(Order = 1)] public string Value { get; set; } = null!;
        }
    }
}
