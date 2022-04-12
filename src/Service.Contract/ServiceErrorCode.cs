using System.ComponentModel;
using System.Runtime.Serialization;

namespace WebApp.Service;

[DataContract]
public enum ServiceErrorCode
{
    [EnumMember] Unknown = 0,

    [Description("Value for parameter {0} was not specified.")]
    [EnumMember] ParamNotSpecified,

    [Description("Value of parameter {0} is not valid.")]
    [EnumMember] ParamNotValid,

    [Description("Entity identified by parameter {0} was not found.")]
    [EnumMember] EntityNotFound,

    [Description("Entity identified by parameter {0} is not unique.")]
    [EnumMember] EntityNotUnique,

    [Description("Entity identified by parameter {0} has dependencies.")]
    [EnumMember] EntityDependent,
}
