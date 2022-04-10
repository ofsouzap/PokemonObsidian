using System;

public static class GameVersion
{

    /* Versions:
     * 0x0000 - v1.0.0a1
     */

    public const ushort version = 0x0000;
    public static byte[] VersionBytes => BitConverter.GetBytes(version);

}