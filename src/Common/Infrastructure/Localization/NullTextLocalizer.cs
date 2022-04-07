using System.Globalization;

namespace WebApp.Common.Infrastructure.Localization;

public sealed class NullTextLocalizer : ITextLocalizer
{
    public static readonly NullTextLocalizer Instance = new NullTextLocalizer();

    private NullTextLocalizer() { }

    public string this[string hint] => hint;

    public string this[string hint, params object[] args] => string.Format(CultureInfo.CurrentCulture, hint, args);
}
