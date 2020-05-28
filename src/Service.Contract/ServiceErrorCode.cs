using System.ComponentModel;

namespace WebApp.Service
{
    public enum ServiceErrorCode
    {
        Unknown = 0,

        [Description("Value for parameter {0} was not specified.")]
        ParamNotSpecified,

        [Description("Value of parameter {0} is not valid.")]
        ParamNotValid,

        [Description("Entity identified by parameter {0} was not found.")]
        EntityNotFound,

        [Description("Entity identified by parameter {0} is not unique.")]
        EntityNotUnique,

        [Description("Entity identified by parameter {0} has dependencies.")]
        EntityDependent,
    }
}
