using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace POTools.Helpers;

public static class PluralFormHelper
{
    // source: http://docs.translatehouse.org/projects/localization-guide/en/latest/l10n/pluralforms.html
    private static readonly FrozenDictionary<string, (int, string)> s_pluralForms = new KeyValuePair<string, (int, string)>[]
    {
        new("ach", (2, "(n > 1)")),
        new("af", (2, "(n != 1)")),
        new("ak", (2, "(n > 1)")),
        new("am", (2, "(n > 1)")),
        new("an", (2, "(n != 1)")),
        new("anp", (2, "(n != 1)")),
        new("ar", (6, "(n==0 ? 0 : n==1 ? 1 : n==2 ? 2 : n%100>=3 && n%100<=10 ? 3 : n%100>=11 ? 4 : 5)")),
        new("arn", (2, "(n > 1)")),
        new("as", (2, "(n != 1)")),
        new("ast", (2, "(n != 1)")),
        new("ay", (1, "0")),
        new("az", (2, "(n != 1)")),
        new("be", (3, "(n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2)")),
        new("bg", (2, "(n != 1)")),
        new("bn", (2, "(n != 1)")),
        new("bo", (1, "0")),
        new("br", (2, "(n > 1)")),
        new("brx", (2, "(n != 1)")),
        new("bs", (3, "(n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2)")),
        new("ca", (2, "(n != 1)")),
        new("cgg", (1, "0")),
        new("cs", (3, "(n==1) ? 0 : (n>=2 && n<=4) ? 1 : 2")),
        new("csb", (3, "(n==1) ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2")),
        new("cy", (4, "(n==1) ? 0 : (n==2) ? 1 : (n != 8 && n != 11) ? 2 : 3")),
        new("da", (2, "(n != 1)")),
        new("de", (2, "(n != 1)")),
        new("doi", (2, "(n != 1)")),
        new("dz", (1, "0")),
        new("el", (2, "(n != 1)")),
        new("en", (2, "(n != 1)")),
        new("eo", (2, "(n != 1)")),
        new("es", (2, "(n != 1)")),
        new("es-AR", (2, "(n != 1)")),
        new("et", (2, "(n != 1)")),
        new("eu", (2, "(n != 1)")),
        new("fa", (2, "(n > 1)")),
        new("ff", (2, "(n != 1)")),
        new("fi", (2, "(n != 1)")),
        new("fil", (2, "(n > 1)")),
        new("fo", (2, "(n != 1)")),
        new("fr", (2, "(n > 1)")),
        new("fur", (2, "(n != 1)")),
        new("fy", (2, "(n != 1)")),
        new("ga", (5, "n==1 ? 0 : n==2 ? 1 : (n>2 && n<7) ? 2 :(n>6 && n<11) ? 3 : 4")),
        new("gd", (4, "(n==1 || n==11) ? 0 : (n==2 || n==12) ? 1 : (n > 2 && n < 20) ? 2 : 3")),
        new("gl", (2, "(n != 1)")),
        new("gu", (2, "(n != 1)")),
        new("gun", (2, "(n > 1)")),
        new("ha", (2, "(n != 1)")),
        new("he", (2, "(n != 1)")),
        new("hi", (2, "(n != 1)")),
        new("hne", (2, "(n != 1)")),
        new("hr", (3, "(n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2)")),
        new("hu", (2, "(n != 1)")),
        new("hy", (2, "(n != 1)")),
        new("ia", (2, "(n != 1)")),
        new("id", (1, "0")),
        new("is", (2, "(n%10!=1 || n%100==11)")),
        new("it", (2, "(n != 1)")),
        new("ja", (1, "0")),
        new("jbo", (1, "0")),
        new("jv", (2, "(n != 0)")),
        new("ka", (1, "0")),
        new("kk", (2, "(n != 1)")),
        new("kl", (2, "(n != 1)")),
        new("km", (1, "0")),
        new("kn", (2, "(n != 1)")),
        new("ko", (1, "0")),
        new("ku", (2, "(n != 1)")),
        new("kw", (4, "(n==1) ? 0 : (n==2) ? 1 : (n == 3) ? 2 : 3")),
        new("ky", (2, "(n != 1)")),
        new("lb", (2, "(n != 1)")),
        new("ln", (2, "(n > 1)")),
        new("lo", (1, "0")),
        new("lt", (3, "(n%10==1 && n%100!=11 ? 0 : n%10>=2 && (n%100<10 || n%100>=20) ? 1 : 2)")),
        new("lv", (3, "(n%10==1 && n%100!=11 ? 0 : n != 0 ? 1 : 2)")),
        new("mai", (2, "(n != 1)")),
        new("me", (3, "n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2")),
        new("mfe", (2, "(n > 1)")),
        new("mg", (2, "(n > 1)")),
        new("mi", (2, "(n > 1)")),
        new("mk", (2, "n==1 || n%10==1 ? 0 : 1")),
        new("ml", (2, "(n != 1)")),
        new("mn", (2, "(n != 1)")),
        new("mni", (2, "(n != 1)")),
        new("mnk", (3, "(n==0 ? 0 : n==1 ? 1 : 2)")),
        new("mr", (2, "(n != 1)")),
        new("ms", (1, "0")),
        new("mt", (4, "(n==1 ? 0 : n==0 || ( n%100>1 && n%100<11) ? 1 : (n%100>10 && n%100<20 ) ? 2 : 3)")),
        new("my", (1, "0")),
        new("nah", (2, "(n != 1)")),
        new("nap", (2, "(n != 1)")),
        new("nb", (2, "(n != 1)")),
        new("ne", (2, "(n != 1)")),
        new("nl", (2, "(n != 1)")),
        new("nn", (2, "(n != 1)")),
        new("no", (2, "(n != 1)")),
        new("nso", (2, "(n != 1)")),
        new("oc", (2, "(n > 1)")),
        new("or", (2, "(n != 1)")),
        new("pa", (2, "(n != 1)")),
        new("pap", (2, "(n != 1)")),
        new("pl", (3, "(n==1 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2)")),
        new("pms", (2, "(n != 1)")),
        new("ps", (2, "(n != 1)")),
        new("pt", (2, "(n != 1)")),
        new("pt-BR", (2, "(n > 1)")),
        new("rm", (2, "(n != 1)")),
        new("ro", (3, "(n==1 ? 0 : (n==0 || (n%100 > 0 && n%100 < 20)) ? 1 : 2)")),
        new("ru", (3, "(n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2)")),
        new("rw", (2, "(n != 1)")),
        new("sah", (1, "0")),
        new("sat", (2, "(n != 1)")),
        new("sco", (2, "(n != 1)")),
        new("sd", (2, "(n != 1)")),
        new("se", (2, "(n != 1)")),
        new("si", (2, "(n != 1)")),
        new("sk", (3, "(n==1) ? 0 : (n>=2 && n<=4) ? 1 : 2")),
        new("sl", (4, "(n%100==1 ? 0 : n%100==2 ? 1 : n%100==3 || n%100==4 ? 2 : 3)")),
        new("so", (2, "(n != 1)")),
        new("son", (2, "(n != 1)")),
        new("sq", (2, "(n != 1)")),
        new("sr", (3, "(n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2)")),
        new("su", (1, "0")),
        new("sv", (2, "(n != 1)")),
        new("sw", (2, "(n != 1)")),
        new("ta", (2, "(n != 1)")),
        new("te", (2, "(n != 1)")),
        new("tg", (2, "(n > 1)")),
        new("th", (1, "0")),
        new("ti", (2, "(n > 1)")),
        new("tk", (2, "(n != 1)")),
        new("tr", (2, "(n > 1)")),
        new("tt", (1, "0")),
        new("ug", (1, "0")),
        new("uk", (3, "(n%10==1 && n%100!=11 ? 0 : n%10>=2 && n%10<=4 && (n%100<10 || n%100>=20) ? 1 : 2)")),
        new("ur", (2, "(n != 1)")),
        new("uz", (2, "(n > 1)")),
        new("vi", (1, "0")),
        new("wa", (2, "(n > 1)")),
        new("wo", (1, "0")),
        new("yo", (2, "(n != 1)")),
        new("zh", (1, "0")),
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static bool TryGetPluralForm(CultureInfo culture, out int pluralFormCount, [MaybeNullWhen(false)] out string pluralFormSelector)
    {
        for (; ; )
        {
            if (s_pluralForms.TryGetValue(culture.Name, out var pluralForm))
            {
                (pluralFormCount, pluralFormSelector) = pluralForm;
                return true;
            }

            var parentCulture = culture.Parent;
            if (culture == parentCulture)
            {
                (pluralFormCount, pluralFormSelector) = (default, default);
                return false;
            }

            culture = parentCulture;
        }
    }
}
