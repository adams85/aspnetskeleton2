using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public class ResetPasswordCommand : ICommand
    {
        [Required]
        [DataMember(Order = 1)] public string UserName { get; set; } = null!;

        [DataMember(Order = 2)] public TimeSpan TokenExpirationTimeSpan { get; set; }
    }
}
