using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WebApp.Service.Infrastructure.Validation;

namespace WebApp.Service
{
    public partial class ServiceErrorException : ApplicationException
    {
        internal static ServiceErrorException From(ValidationException exception)
        {
            var errorCode = exception.ValidationAttribute switch
            {
                RequiredAttribute _ => ServiceErrorCode.ParamNotSpecified,
                _ => ServiceErrorCode.ParamNotValid
            };

            var memberPath = exception.GetMemberPath(exception.ValidationResult?.MemberNames?.FirstOrDefault());

            return new ServiceErrorException(errorCode, memberPath);
        }
    }
}
