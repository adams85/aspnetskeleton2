﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users
{
    [DataContract]
    public class UnlockUserCommand : ICommand
    {
        [Required]
        [DataMember(Order = 1)] public string UserName { get; set; } = null!;
    }
}
