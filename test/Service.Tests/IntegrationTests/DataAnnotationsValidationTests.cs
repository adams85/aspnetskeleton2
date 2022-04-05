using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using WebApp.Common.Infrastructure.Localization;
using WebApp.Common.Infrastructure.Validation;
using WebApp.Service.Infrastructure.Validation;
using WebApp.Service.Tests.Helpers;
using WebApp.Service.Users;
using Xunit;

namespace WebApp.Service.Infrastructure
{
    public class DataAnnotationsValidationTests
    {
        [Fact]
        public void RespectsNullableRefTypeAnnotation()
        {
            var command = new GetUserQuery { };

            var validationEx = Assert.Throws<ValidationException>(() => DataAnnotationsValidator.Validate(command));

            Assert.IsType<RequiredAttribute>(validationEx.ValidationAttribute);
            Assert.Equal(new[] { nameof(command.Identifier) }, validationEx.ValidationResult.MemberNames);

            var serviceErrorEx = ServiceErrorException.From(validationEx);

            Assert.Equal(ServiceErrorCode.ParamNotSpecified, serviceErrorEx.ErrorCode);
            Assert.Equal(new[] { nameof(command.Identifier) }, serviceErrorEx.Args);
        }

        [Fact]
        public void ChecksNestedProperty()
        {
            var command = new GetUserQuery
            {
                Identifier = new UserIdentifier.Name { }
            };

            var validationEx = Assert.Throws<ValidationException>(() => DataAnnotationsValidator.Validate(command));

            Assert.IsType<RequiredAttribute>(validationEx.ValidationAttribute);
            Assert.Equal(new[] { nameof(UserIdentifier.Name.Value) }, validationEx.ValidationResult.MemberNames);

            var serviceErrorEx = ServiceErrorException.From(validationEx);

            Assert.Equal(ServiceErrorCode.ParamNotSpecified, serviceErrorEx.ErrorCode);
            Assert.Equal(new[] { nameof(command.Identifier) + "." + nameof(UserIdentifier.Name.Value) }, serviceErrorEx.Args);
        }

        [Fact]
        public void RespectsIValidatableObjectImplementation()
        {
            var command = new ApproveUserCommand
            {
                UserName = "user",
                Verify = true,
                VerificationToken = null
            };

            var validationEx = Assert.Throws<ValidationException>(() => DataAnnotationsValidator.Validate(command));

            Assert.IsType<ExtendedValidationResult>(validationEx.ValidationResult);
            Assert.IsType<RequiredAttribute>(validationEx.ValidationAttribute);
            Assert.Equal(new[] { nameof(command.VerificationToken) }, validationEx.ValidationResult.MemberNames);

            var serviceErrorEx = ServiceErrorException.From(validationEx);

            Assert.Equal(ServiceErrorCode.ParamNotSpecified, serviceErrorEx.ErrorCode);
            Assert.Equal(new[] { nameof(command.VerificationToken) }, serviceErrorEx.Args);
        }

        [Fact]
        public void ExtendedValidationAttribute_CustomTextLocalizer()
        {
            var command = new ListUsersQuery
            {
                OrderBy = new[] { "" }
            };

            var validationEx = Assert.Throws<ValidationException>(() => DataAnnotationsValidator.Validate(command));

            Assert.IsType<ItemsRequiredAttribute>(validationEx.ValidationAttribute);
            Assert.Equal(new[] { nameof(command.OrderBy) }, validationEx.ValidationResult.MemberNames);

            string? textToLocalize = null;

            var validationAttribute = (ExtendedValidationAttribute)validationEx.ValidationAttribute!;
            var formattedErrorMessage = validationAttribute.FormatErrorMessage(nameof(command.OrderBy), new DelegatedTextLocalizer((hint, args) =>
            {
                textToLocalize = hint;
                return args != null ? NullTextLocalizer.Instance[hint, args] : NullTextLocalizer.Instance[hint];
            }));

            Assert.Equal("The field {0} must contain non-empty strings.", textToLocalize);
            Assert.Equal($"The field {nameof(command.OrderBy)} must contain non-empty strings.", formattedErrorMessage);
        }

        [Fact]
        public void ServiceValidationAttribute_ServiceUnavailableButValidationIgnored()
        {
            var command = new ChangePasswordCommand
            {
                UserName = "user",
                NewPassword = "12345",
            };

            DataAnnotationsValidator.Validate(command);
        }

        [Fact]
        public async Task ServiceValidationAttribute_ServiceAvailable_ComplexityRequirementsMet()
        {
            var services = new ServiceCollection();

            services.Configure<PasswordOptions>(options =>
            {
                options.RequiredLength = 6;
                options.RequireDigit = true;
                options.RequireNonAlphanumeric = true;
                options.RequireLowercase = true;
                options.RequiredUniqueChars = 2;
            });

            services.AddSingleton<IValidator<PasswordAttribute>, PasswordValidator>();

            var command = new ChangePasswordCommand
            {
                UserName = "user",
                NewPassword = "12345+Z",
            };

            await using var sp = services.BuildServiceProvider();

            DataAnnotationsValidator.Validate(command);
        }

        [Fact]
        public async Task ServiceValidationAttribute_ServiceAvailable_ComplexityRequirementsNotMet()
        {
            var services = new ServiceCollection();

            services.Configure<PasswordOptions>(options =>
            {
                options.RequiredLength = 6;
                options.RequireDigit = true;
                options.RequireNonAlphanumeric = true;
                options.RequireLowercase = true;
                options.RequiredUniqueChars = 2;
            });

            services.AddSingleton<IStringLocalizerFactory>(NullStringLocalizerFactory.Instance);
            services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
            services.AddSingleton<IValidator<PasswordAttribute>, PasswordValidator>();

            var command = new ChangePasswordCommand
            {
                UserName = "user",
                NewPassword = "12345",
            };

            await using var sp = services.BuildServiceProvider();

            var validationEx = Assert.Throws<ValidationException>(() => DataAnnotationsValidator.Validate(command, sp));

            Assert.IsType<ExtendedValidationResult>(validationEx.ValidationResult);
            Assert.IsType<PasswordAttribute>(validationEx.ValidationAttribute);
            Assert.Equal(new[] { nameof(command.NewPassword) }, validationEx.ValidationResult.MemberNames);
        }
    }
}
