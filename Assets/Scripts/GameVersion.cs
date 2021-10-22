using System;

public static class GameVersion
{
    public const ushort version = 0x0000;
    public static byte[] VersionBytes => BitConverter.GetBytes(version);
}