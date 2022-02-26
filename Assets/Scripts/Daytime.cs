using System;

public static class Daytime
{

    public static int HourNow => DateTime.Now.Hour;

    public const int minDaytimeHour = 6;
    public const int maxDaytimeHour = 9;

    public static bool IsDaytime => HourNow > minDaytimeHour && HourNow < maxDaytimeHour;

}