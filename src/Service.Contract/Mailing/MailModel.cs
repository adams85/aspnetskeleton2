using System.Runtime.Serialization;
using ProtoBuf;
using WebApp.Service.Mailing.Users;

namespace WebApp.Service.Mailing
{
    [DataContract]
    [ProtoInclude(1, typeof(PasswordResetMailModel))]
    [ProtoInclude(2, typeof(UnapprovedUserCreatedMailModel))]
    [ProtoInclude(3, typeof(UserLockedOutMailModel))]
    public abstract class MailModel
    {
        public abstract string MailType { get; }
    }
}
