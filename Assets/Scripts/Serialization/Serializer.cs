using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Pokemon;
using Items;
using Battle;

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

        public abstract ushort GetVersionCode();

        #region Serialization

        public void SerializeDetails(Stream stream)
        {

            byte[] buffer;

            stream.Write(fileSignatureBytes, 0, 8);

            buffer = BitConverter.GetBytes(GetVersionCode());
            stream.Write(buffer, 0, 2);

            buffer = BitConverter.GetBytes(EpochTime.SecondsNow);
            stream.Write(buffer, 0, 8);

        }

        public abstract void SerializeData(Stream stream, PlayerData player = null);

        public abstract void SerializePokemonInstance(Stream stream, PokemonInstance pokemon);
        public abstract void SerializeInventorySection(Stream stream, PlayerData.Inventory.InventorySection section);
        public abstract void SerializeInventoryItem(Stream stream, int itemId, int quantity);
        public abstract void SerializePokedex(Stream stream, PlayerData.Pokedex pokedex);
        public abstract void SerializePlayerPartyAndStorageSystemPokemon(Stream stream, PlayerData player = null);
        public abstract void SerializePlayerTradeReceivedPokemonGuids(Stream stream, Guid[] guids);
        public abstract void SerializeString(Stream stream, string s);
        public abstract void SerializeBattleAction(Stream stream, BattleParticipant.Action action);

        public static void SerializeHeldItem(Stream stream, PokemonInstance pokemon)
        {

            byte[] buffer;

            if (pokemon.heldItem == null)
                buffer = BitConverter.GetBytes((int)-1);
            else
                buffer = BitConverter.GetBytes(pokemon.heldItem.GetId());

            stream.Write(buffer, 0, 4);

        }

        public abstract void SerializeSceneStack(Stream stream, GameSceneManager.SceneStack stack);

        #endregion

        #region Stats

        public static void SerializeByteStats(Stream stream, Stats<byte> stats)
        {

            byte[] buffer = new byte[6];

            buffer[0] = stats.attack;
            buffer[1] = stats.defense;
            buffer[2] = stats.specialAttack;
            buffer[3] = stats.specialDefense;
            buffer[4] = stats.speed;
            buffer[5] = stats.health;

            stream.Write(buffer, 0, 6);

        }

        public Stats<byte> DeserializeByteStats(Stream stream)
        {

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

        public static void SerializeIntStats(Stream stream, Stats<int> stats)
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

        public Stats<int> DeserializeIntStats(Stream stream)
        {

            int attack, defense, specialAttack, specialDefense, speed, health;

            byte[] buffer = new byte[4];

            stream.Read(buffer, 0, 4);
            attack = BitConverter.ToInt32(buffer, 0);

            stream.Read(buffer, 0, 4);
            defense = BitConverter.ToInt32(buffer, 0);

            stream.Read(buffer, 0, 4);
            specialAttack = BitConverter.ToInt32(buffer, 0);

            stream.Read(buffer, 0, 4);
            specialDefense = BitConverter.ToInt32(buffer, 0);

            stream.Read(buffer, 0, 4);
            speed = BitConverter.ToInt32(buffer, 0);

            stream.Read(buffer, 0, 4);
            health = BitConverter.ToInt32(buffer, 0);

            return new Stats<int>()
            {
                attack = attack,
                defense = defense,
                specialAttack = specialAttack,
                specialDefense = specialDefense,
                speed = speed,
                health = health
            };

        }

        #endregion

        #region Custom Value Serialization

        #region bool

        public const byte booleanByteTrue = 0xff;
        public const byte booleanByteFalse = 0x00;

        public void SerializeBool(Stream stream, bool state)
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

        public bool DeserializeBool(Stream stream)
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

        public void SerializeNullableBool(Stream stream, bool? state)
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

        public bool? DeserializeNullableBool(Stream stream)
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

        public static void DeserializeDetails(Stream stream,
            out ushort saveFileVersion,
            out long saveTime)
        {

            byte[] buffer;

            //File Signature
            buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            long fileSignature = BitConverter.ToInt64(buffer, 0);
            if (fileSignature != Serializer.fileSignature)
                throw new ArgumentException("Invalid file signature of provided data");

            //Save File Version
            buffer = new byte[2];
            stream.Read(buffer, 0, 2);
            saveFileVersion = BitConverter.ToUInt16(buffer, 0);

            //Save Time
            buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            saveTime = BitConverter.ToInt64(buffer, 0);

        }

        public abstract void DeserializeData(Stream stream,
            out long saveTime,
            out PlayerData playerData,
            out GameSettings gameSettings,
            out GameSceneManager.SceneStack sceneStack);

        public abstract PokemonInstance DeserializePokemonInstance(Stream stream);

        public abstract Dictionary<int, int> DeserializeInventorySection(Stream stream);

        public abstract void DeserializeInventoryItem(Stream stream,
            out int itemId,
            out int quantity);

        public abstract PlayerData.Pokedex DeserializePokedex(Stream stream);

        public abstract void DeserializePlayerPartyAndStorageSystemPokemon(Stream stream,
            out PokemonInstance[] partyPokemon,
            out PlayerData.PokemonStorageSystem storageSystem);

        public abstract void DeserializePlayerTradeReceivedPokemonGuids(Stream stream,
            out Guid[] guids);

        public abstract string DeserializeString(Stream stream);

        public abstract BattleParticipant.Action DeserializeBattleAction(Stream stream,
            BattleParticipant user,
            BattleParticipant target);

        public static Item DeserializeItem(Stream stream)
        {

            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);

            int itemId = BitConverter.ToInt32(buffer, 0);

            if (itemId < 0)
                return null;
            else
                return Item.GetItemById(itemId);

        }

        public abstract GameSceneManager.SceneStack DeserializeSceneStack(Stream stream);

        #endregion

    }

}