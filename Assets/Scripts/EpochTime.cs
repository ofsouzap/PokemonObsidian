using System;

public static class EpochTime
{

    public static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

    public static DateTime EpochTimeToDateTime(long epochTime)
    {

        DateTime time = epoch.AddSeconds(epochTime);

        return time;

    }

    public static long SecondsNow
        => DateTimeOffset.Now.ToUnixTimeSeconds();

}