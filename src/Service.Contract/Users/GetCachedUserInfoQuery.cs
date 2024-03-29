﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WebApp.Service.Users;

[DataContract]
public record class GetCachedUserInfoQuery : IQuery<CachedUserInfoData?>
{
    [Required]
    [DataMember(Order = 1)] public string UserName { get; init; } = null!;
}
