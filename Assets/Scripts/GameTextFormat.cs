using System;
using System.Collections.Generic;

public static class GameTextFormat
{

    private const string replacementPrefix = "{";
    private const string replacementSuffix = "}";

    private static readonly Dictionary<string, Func<string>> replacements = new Dictionary<string, Func<string>>()
    {
        { "playername", () => PlayerData.singleton.profile.name },
        { "currencysymbol", () => PlayerData.currencySymbol }
    };

    public static string Format(string s)
    {

        foreach (KeyValuePair<string, Func<string>> pair in replacements)
        {

            string rawTarget = pair.Key;
            string exactTarget = replacementPrefix + rawTarget + replacementSuffix;

            string replacement = pair.Value();

            s = s.Replace(exactTarget, replacement);

        }

        return s;

    }

}
