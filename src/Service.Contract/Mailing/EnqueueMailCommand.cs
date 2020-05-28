using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Mailing
{
    [DataContract]
    public class EnqueueMailCommand : ICommand
    {
        [Required]
        [DataMember(Order = 1)] public MailModel Model { get; set; } = null!;
    }
}
