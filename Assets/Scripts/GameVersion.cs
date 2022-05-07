using System;

public static class GameVersion
{

    /* Versions:
     * 0x0000 - v1.0.0a1-v1.0.0a3
     * 0x0001 - v1.0.0a4-v1.0.0b1
     */

    public const ushort version = 0x0001;
    public static byte[] VersionBytes => BitConverter.GetBytes(version);

}