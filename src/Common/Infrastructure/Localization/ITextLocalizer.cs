namespace WebApp.Common.Infrastructure.Localization;

public interface ITextLocalizer
{
    string this[string hint] { get; }
    string this[string hint, params object[] args] { get; }
}
