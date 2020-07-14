namespace WebApp.UI.Infrastructure.Security
{
    public enum CreateUserResult
    {
        Success,
        UnexpectedError,
        InvalidUserName,
        DuplicateUserName,
        InvalidEmail,
        DuplicateEmail,
        InvalidPassword,
    }
}
