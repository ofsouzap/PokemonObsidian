using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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

        public static ushort defaultSerializerVersion
            => serializers.Keys.Max();

        public static Serializer DefaultSerializer
            => serializers[defaultSerializerVersion];

        public static ushort GetSaveDataVersionNumber(Stream stream)
        {

            if (stream.Length < 10)
                throw new ArgumentException("Not enough data provided to read version number");

            byte[] buffer;

            //First 8 bytes are the file signature
            buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            //Next 2 btyes are the version number
            buffer = new byte[2];
            stream.Read(buffer, 0, 2);
            return BitConverter.ToUInt16(buffer, 0);

        }

        public static void SerializeData(Stream stream)
            => SerializeData(stream, defaultSerializerVersion);

        public static void SerializeData(Stream stream, ushort versionNumber)
        {

            if (!serializers.ContainsKey(versionNumber))
            {
                throw new ArgumentException($"No serializer for provided version number ({versionNumber})");
            }
            else
            {
                serializers[versionNumber].SerializeData(stream);
            }

        }

        public static void DeserializeData(Stream stream,
            out long saveTime,
            out PlayerData playerData,
            out GameSettings gameSettings,
            out GameSceneManager.SceneStack sceneStack)
            => DeserializeData(defaultSerializerVersion, stream,
                out saveTime,
                out playerData,
                out gameSettings,
                out sceneStack);

        public static void DeserializeData(ushort versionNumber, Stream stream,
            out long saveTime,
            out PlayerData playerData,
            out GameSettings gameSettings,
            out GameSceneManager.SceneStack sceneStack)
        {

            if (!serializers.ContainsKey(versionNumber))
            {
                throw new ArgumentException($"No serializer for provided version number ({versionNumber})");
            }
            else
            {
                serializers[versionNumber].DeserializeData(stream,
                    out saveTime,
                    out playerData,
                    out gameSettings,
                    out sceneStack);
            }

        }

    }
}
