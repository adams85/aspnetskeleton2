namespace WebApp.UI.Infrastructure.Security;

public enum CreateUserStatus
{
    UnexpectedError = -1,
    Success,
    InvalidUserName,
    DuplicateUserName,
    InvalidEmail,
    DuplicateEmail,
    InvalidPassword,
}
