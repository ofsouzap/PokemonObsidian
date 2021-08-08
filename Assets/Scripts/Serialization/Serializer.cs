using System;
using System.Collections.Generic;
using System.Text;
using Pokemon;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

namespace Serialization {

    public abstract class Serializer
    {

        public const long fileSignature = 0x4f4e4d500225251c; //N.B. will be reversed because little-/big-endian (one of the two, dunno which)
        public static byte[] fileSignatureBytes => BitConverter.GetBytes(fileSignature);

        protected abstract ushort GetVersionCode();

        #region Serialization

        public abstract byte[] SerializeData(PlayerData player = null);

        protected abstract byte[] SerializePokemonInstance(PokemonInstance pokemon);
        protected abstract byte[] SerializeInventorySection(PlayerData.Inventory.InventorySection section);
        protected abstract byte[] SerializeInventoryItem(int itemId, int quantity);
        protected abstract byte[] SerializePlayerPartyAndStorageSystemPokemon(PlayerData player = null);

        /// <param name="bytes">The string as a byte array</param>
        /// <returns>The length of the byte array</returns>
        protected abstract int SerializeString(string s, out byte[] bytes);

        protected static byte[] SerializeHeldItem(PokemonInstance pokemon)
        {
            if (pokemon.heldItem == null)
                return BitConverter.GetBytes((int)-1);
            else
                return BitConverter.GetBytes(pokemon.heldItem.GetId());
        }

        protected abstract byte[] SerializeSceneStack(GameSceneManager.SceneStack stack);

        #endregion

        #region Stats

        protected static byte[] SerializeByteStats(Stats<byte> stats)
        {

            List<byte> bytes = new List<byte>();
            bytes.Add(stats.attack);
            bytes.Add(stats.defense);
            bytes.Add(stats.specialAttack);
            bytes.Add(stats.specialDefense);
            bytes.Add(stats.speed);
            bytes.Add(stats.health);
            return bytes.ToArray();

        }

        protected Stats<byte> DeserializeByteStats(byte[] bytes,
            int startOffset = 0)
        {

            if (bytes.Length < 6)
                throw new ArgumentException("Incompatible bytes provided");

            return new Stats<byte>()
            {
                attack = bytes[startOffset + 0],
                defense = bytes[startOffset + 1],
                specialAttack = bytes[startOffset + 2],
                specialDefense = bytes[startOffset + 3],
                speed = bytes[startOffset + 4],
                health = bytes[startOffset + 5]
            };

        }

        protected static byte[] SerializeIntStats(Stats<int> stats)
        {

            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(stats.attack));
            bytes.AddRange(BitConverter.GetBytes(stats.defense));
            bytes.AddRange(BitConverter.GetBytes(stats.specialAttack));
            bytes.AddRange(BitConverter.GetBytes(stats.specialDefense));
            bytes.AddRange(BitConverter.GetBytes(stats.speed));
            bytes.AddRange(BitConverter.GetBytes(stats.health));
            return bytes.ToArray();

        }

        protected Stats<int> DeserializeIntStats(byte[] bytes,
            int startOffset = 0)
        {

            if (bytes.Length < 24) //6 * 4 = 24, 6 stat values, 4 bytes per int value
                throw new ArgumentException("Incompatible bytes provided");

            //Return output
            return new Stats<int>()
            {
                attack = BitConverter.ToInt32(bytes, startOffset + 0),
                defense = BitConverter.ToInt32(bytes, startOffset + 4),
                specialAttack = BitConverter.ToInt32(bytes, startOffset + 8),
                specialDefense = BitConverter.ToInt32(bytes, startOffset + 12),
                speed = BitConverter.ToInt32(bytes, startOffset + 16),
                health = BitConverter.ToInt32(bytes, startOffset + 20),
            };

        }

        #endregion

        #region Custom Value Serialization

        #region bool

        public const byte booleanByteTrue = 0xff;
        public const byte booleanByteFalse = 0x00;

        protected static byte SerializeBool(bool state)
        {

            return state switch
            {
                true => booleanByteTrue,
                false => booleanByteFalse
            };

        }

        protected static bool DeserializeBool(byte b)
        {
            if (b == booleanByteTrue)
                return true;
            else if (b == booleanByteFalse)
                return false;
            else
                throw new ArgumentException("Invalid byte provided");
        }

        #endregion

        #region bool?

        public const byte nullableBooleanByteNull = 0xc3;

        protected static byte SerializeNullableBool(bool? state)
        {
            return state switch
            {
                true => booleanByteTrue,
                false => booleanByteFalse,
                null => nullableBooleanByteNull
            };
        }

        protected static bool? DeserializeNullableBool(byte b)
        {

            if (b == booleanByteTrue)
                return true;
            else if (b == booleanByteFalse)
                return false;
            else if (b == nullableBooleanByteNull)
                return null;
            else
                throw new ArgumentException("Invalid byte provided");

        }

        #endregion

        #endregion

        #region Deserialization

        public abstract void DeserializeData(byte[] data,
            int startOffset,
            out long saveTime,
            out PlayerData playerData,
            out GameSceneManager.SceneStack sceneStack,
            out int byteLength);

        protected abstract PokemonInstance DeserializePokemonInstance(byte[] data, int startOffset, out int byteLength);

        protected abstract Dictionary<int, int> DeserializeInventorySection(byte[] data, int startOffset, out int byteLength);

        protected abstract void DeserializeInventoryItem(byte[] data, int startOffset,
            out int itemId,
            out int quantity,
            out int byteLength);

        protected abstract void DeserializePlayerPartyAndStorageSystemPokemon(byte[] data, int startOffset,
            out PokemonInstance[] partyPokemon,
            out PlayerData.PokemonStorageSystem storageSystem,
            out int byteLength);

        protected abstract string DeserializeString(byte[] data, int startOffset, out int byteLength);

        protected static Item DeserializeItem(byte[] data, int startOffset, out int byteLength)
        {

            int itemId = BitConverter.ToInt32(data, startOffset);
            byteLength = 4;

            if (itemId < 0)
                return null;
            else
                return Item.GetItemById(itemId);

        }

        protected abstract GameSceneManager.SceneStack DeserializeSceneStack(byte[] data, int startOffset, out int byteLength);

        #endregion

    }

}