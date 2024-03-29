﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users;

[DataContract]
public record class ResetPasswordCommand : ICommand
{
    [Required]
    [DataMember(Order = 1)] public string UserName { get; init; } = null!;

    [DataMember(Order = 2)] public TimeSpan TokenExpirationTimeSpan { get; init; }
}
