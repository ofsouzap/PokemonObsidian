using System;

public static class EpochTime
{

    public const string defaultFormat = "dd/MM/yy HH:mm";

    public static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    public static string EpochTimeToFormattedLocalTime(long epochTime,
        string format = defaultFormat)
        => GetLocalTime(EpochTimeToDateTime(epochTime)).ToString(format);

    public static DateTime EpochTimeToDateTime(long epochTime)
    {
        
        DateTime time = epoch.AddSeconds(epochTime);
        
        return time;

    }

    public static DateTime GetLocalTime(DateTime dt)
        => dt.ToLocalTime();

    public static long SecondsNow
        => DateTimeOffset.Now.ToUnixTimeSeconds();

}