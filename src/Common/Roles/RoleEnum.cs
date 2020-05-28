using System.ComponentModel;
using System.Runtime.Serialization;

namespace WebApp.Common.Roles
{
    [DataContract]
    public enum RoleEnum
    {
        [Description("Administators")]
        [EnumMember] Administators = 1,
    }
}
