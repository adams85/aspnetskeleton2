namespace WebApp.UI.Infrastructure.Security
{
    public enum ChangePasswordStatus
    {
        UnexpectedError = -1,
        Success,
        UserNotExists,
        UserUnapproved,
        UserLockedOut,
        InvalidCredentials,
        InvalidNewPassword,
    }
}
