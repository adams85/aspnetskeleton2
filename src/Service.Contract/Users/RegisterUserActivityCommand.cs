﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users;

[DataContract]
public record class RegisterUserActivityCommand : ICommand
{
    [Required]
    [DataMember(Order = 1)] public string UserName { get; init; } = null!;

    [DataMember(Order = 2)] public bool? SuccessfulLogin { get; init; }

    [DataMember(Order = 3)] public bool UIActivity { get; init; }
}
