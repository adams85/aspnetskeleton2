﻿using System.Runtime.Serialization;
using ProtoBuf;
using WebApp.Service.Mailing.Users;

namespace WebApp.Service.Mailing;

[DataContract]
[ProtoInclude(11, typeof(PasswordResetMailModel))]
[ProtoInclude(12, typeof(UnapprovedUserCreatedMailModel))]
[ProtoInclude(13, typeof(UserLockedOutMailModel))]
public abstract record class MailModel
{
    public abstract string MailType { get; }

    [DataMember(Order = 1)] public string Culture { get; init; } = null!;
    [DataMember(Order = 2)] public string UICulture { get; init; } = null!;
}
