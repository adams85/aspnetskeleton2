using System.ComponentModel;
using System.Runtime.Serialization;

namespace WebApp.Common.Roles;

[DataContract]
public enum RoleEnum
{
    [Description("Administrators")]
    [EnumMember] Administrators = 1,
}
