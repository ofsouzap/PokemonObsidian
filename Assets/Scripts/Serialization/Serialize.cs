using System;
using System.Collections.Generic;
using System.Linq;

namespace Serialization
{
    public static class Serialize
    {

        /// <summary>
        /// Serializers for each save file version
        /// </summary>
        private static readonly Dictionary<ushort, Serializer> serializers = new Dictionary<ushort, Serializer>()
        {
            { 1, new Serializer_v1() }
        };

        private static ushort defaultSerializerVersion
            => serializers.Keys.Max();

        public static ushort GetSaveDataVersionNumber(byte[] data, int startOffset)
        {

            if (data.Length - startOffset < 10)
                throw new ArgumentException("Not enough data provided to read version number");

            return BitConverter.ToUInt16(data, startOffset + 8); //First 8 bytes are the file signature

        }

        public static byte[] SerializeData()
            => SerializeData(defaultSerializerVersion);

        public static byte[] SerializeData(ushort versionNumber)
        {

            if (!serializers.ContainsKey(versionNumber))
            {
                throw new ArgumentException($"No serializer for provided version number ({versionNumber})");
            }
            else
            {
                return serializers[versionNumber].SerializeData();
            }

        }

        public static void DeserializeData(byte[] data, int startOffset,
            out long saveTime,
            out PlayerData playerData,
            out GameSettings gameSettings,
            out GameSceneManager.SceneStack sceneStack,
            out int byteLength)
            => DeserializeData(GetSaveDataVersionNumber(data, startOffset), data, startOffset,
            out saveTime,
            out playerData,
            out gameSettings,
            out sceneStack,
            out byteLength);

        public static void DeserializeData(ushort versionNumber, byte[] data, int startOffset,
            out long saveTime,
            out PlayerData playerData,
            out GameSettings gameSettings,
            out GameSceneManager.SceneStack sceneStack,
            out int byteLength)
        {

            if (!serializers.ContainsKey(versionNumber))
            {
                throw new ArgumentException($"No serializer for provided version number ({versionNumber})");
            }
            else
            {
                serializers[versionNumber].DeserializeData(data, startOffset,
                    out saveTime,
                    out playerData,
                    out gameSettings,
                    out sceneStack,
                    out byteLength);
            }

        }

    }
}
