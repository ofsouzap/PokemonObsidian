using System;
using System.Collections.Generic;
using System.IO;
using Pokemon;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

namespace Serialization {

    public abstract class Serializer
    {

        public class SerializerVersionMismatchException : Exception
        {

            public int IntendedSerializerVersion { get; protected set; }
            public int UsedSerializerVersion { get; protected set; }

            public SerializerVersionMismatchException() : base() { }
            public SerializerVersionMismatchException(string message) : base(message) { }
            public SerializerVersionMismatchException(string message, Exception inner) : base(message, inner) { }
            public SerializerVersionMismatchException(string message, int intendedSerializerVersion, int usedSerializerVersion) : base(message)
            {
                IntendedSerializerVersion = intendedSerializerVersion;
                UsedSerializerVersion = usedSerializerVersion;
            }

        }

        public const long fileSignature = 0x4f4e4d500225251c; //N.B. will be reversed because little-/big-endian (one of the two, dunno which)
        public static byte[] fileSignatureBytes => BitConverter.GetBytes(fileSignature);

        protected abstract ushort GetVersionCode();

        #region Serialization

        public abstract void SerializeData(Stream stream, PlayerData player = null);

        protected abstract void SerializePokemonInstance(Stream stream, PokemonInstance pokemon);
        protected abstract void SerializeInventorySection(Stream stream, PlayerData.Inventory.InventorySection section);
        protected abstract void SerializeInventoryItem(Stream stream, int itemId, int quantity);
        protected abstract void SerializePokedex(Stream stream, PlayerData.Pokedex pokedex);
        protected abstract void SerializePlayerPartyAndStorageSystemPokemon(Stream stream, PlayerData player = null);
        protected abstract void SerializeString(Stream stream, string s);

        protected static void SerializeHeldItem(Stream stream, PokemonInstance pokemon)
        {

            byte[] buffer;

            if (pokemon.heldItem == null)
                buffer = BitConverter.GetBytes((int)-1);
            else
                buffer = BitConverter.GetBytes(pokemon.heldItem.GetId());

            stream.Write(buffer, 0, 4);

        }

        protected abstract void SerializeSceneStack(Stream stream, GameSceneManager.SceneStack stack);

        #endregion

        #region Stats

        protected static void SerializeByteStats(Stream stream, Stats<byte> stats)
        {

            byte[] buffer = new byte[1];

            buffer[0] = stats.attack;
            stream.Write(buffer, 0, 1);

            buffer[0] = stats.defense;
            stream.Write(buffer, 0, 1);

            buffer[0] = stats.specialAttack;
            stream.Write(buffer, 0, 1);

            buffer[0] = stats.specialDefense;
            stream.Write(buffer, 0, 1);

            buffer[0] = stats.speed;
            stream.Write(buffer, 0, 1);

            buffer[0] = stats.health;
            stream.Write(buffer, 0, 1);

        }

        protected Stats<byte> DeserializeByteStats(Stream stream)
        {

            if (stream.Length < 6)
                throw new ArgumentException("Stream too short");

            byte[] buffer = new byte[6];
            stream.Read(buffer, 0, 6);

            return new Stats<byte>()
            {
                attack = buffer[0],
                defense = buffer[1],
                specialAttack = buffer[2],
                specialDefense = buffer[3],
                speed = buffer[4],
                health = buffer[5]
            };

        }

        protected static void SerializeIntStats(Stream stream, Stats<int> stats)
        {

            byte[] buffer;

            buffer = BitConverter.GetBytes(stats.attack);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(stats.defense);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(stats.specialAttack);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(stats.specialDefense);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(stats.speed);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(stats.health);
            stream.Write(buffer, 0, 4);

        }

        protected Stats<int> DeserializeIntStats(Stream stream)
        {

            if (stream.Length < 24) //6 * 4 = 24, 6 stat values, 4 bytes per int value
                throw new ArgumentException("Stream too short");

            byte[] buffer = new byte[24];
            stream.Read(buffer, 0, 24);

            //Return output
            return new Stats<int>()
            {
                attack = BitConverter.ToInt32(buffer, 0),
                defense = BitConverter.ToInt32(buffer, 4),
                specialAttack = BitConverter.ToInt32(buffer, 8),
                specialDefense = BitConverter.ToInt32(buffer, 12),
                speed = BitConverter.ToInt32(buffer, 16),
                health = BitConverter.ToInt32(buffer, 20),
            };

        }

        #endregion

        #region Custom Value Serialization

        #region bool

        public const byte booleanByteTrue = 0xff;
        public const byte booleanByteFalse = 0x00;

        protected static void SerializeBool(Stream stream, bool state)
        {

            byte[] buffer = new byte[1]
            {
                state switch
                {
                    true => booleanByteTrue,
                    false => booleanByteFalse
                }
            };

            stream.Write(buffer, 0, 1);

        }

        protected static bool DeserializeBool(Stream stream)
        {

            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, 1);

            if (buffer[0] == booleanByteTrue)
                return true;
            else if (buffer[0] == booleanByteFalse)
                return false;
            else
                throw new ArgumentException("Invalid byte provided");

        }

        #endregion

        #region bool?

        public const byte nullableBooleanByteNull = 0xc3;

        protected static void SerializeNullableBool(Stream stream, bool? state)
        {

            byte[] buffer = new byte[1]
            {
                state switch
                {
                    true => booleanByteTrue,
                    false => booleanByteFalse,
                    null => nullableBooleanByteNull
                }
            };

            stream.Write(buffer, 0, 1);

        }

        protected static bool? DeserializeNullableBool(Stream stream)
        {

            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, 1);

            if (buffer[0] == booleanByteTrue)
                return true;
            else if (buffer[0] == booleanByteFalse)
                return false;
            else if (buffer[0] == nullableBooleanByteNull)
                return null;
            else
                throw new ArgumentException("Invalid byte provided");

        }

        #endregion

        #endregion

        #region Deserialization

        public abstract void DeserializeData(Stream stream,
            out long saveTime,
            out PlayerData playerData,
            out GameSettings gameSettings,
            out GameSceneManager.SceneStack sceneStack);

        protected abstract PokemonInstance DeserializePokemonInstance(Stream stream);

        protected abstract Dictionary<int, int> DeserializeInventorySection(Stream stream);

        protected abstract void DeserializeInventoryItem(Stream stream,
            out int itemId,
            out int quantity);

        protected abstract PlayerData.Pokedex DeserializePokedex(Stream stream);

        protected abstract void DeserializePlayerPartyAndStorageSystemPokemon(Stream stream,
            out PokemonInstance[] partyPokemon,
            out PlayerData.PokemonStorageSystem storageSystem);

        protected abstract string DeserializeString(Stream stream);

        protected static Item DeserializeItem(Stream stream)
        {

            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);

            int itemId = BitConverter.ToInt32(buffer, 0);

            if (itemId < 0)
                return null;
            else
                return Item.GetItemById(itemId);

        }

        protected abstract GameSceneManager.SceneStack DeserializeSceneStack(Stream stream);

        #endregion

    }

}