using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Mailing;

[DataContract]
public record class EnqueueMailCommand : ICommand
{
    [Required]
    [DataMember(Order = 1)] public MailModel Model { get; init; } = null!;
}
