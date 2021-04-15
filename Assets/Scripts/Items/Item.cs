using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;
using Items.PokeBalls;
using Items.MedicineItems;

namespace Items
{
    public abstract class Item : IHasId
    {

        #region Type Ids

        public const int typeIdGeneral = 0x00000000;
        public const int typeIdPokeBall = 0x10000000;
        public const int typeIdBattleItem = 0x20000000;
        public const int typeIdMedicine = 0x30000000;
        public const int typeIdTM = 0x40000000;
        public const int typeIdBerry = 0x50000000;
        public const int typeIdKeyItem = 0x60000000;

        protected static readonly Dictionary<System.Type, int> typeIds = new Dictionary<System.Type, int>()
        {
            //TODO - once more item types made (eg. general, key item etc.), add entries for them
            { typeof(PokeBall), typeIdPokeBall },
            { typeof(BattleItem), typeIdBattleItem},
            { typeof(MedicineItem), typeIdMedicine },
            { typeof(TMItem), typeIdTM }
        };

        public static int GetItemIdTypeId(int itemId)
            => itemId >> 28;

        public static int GetItemTypeId(Item item)
            => GetItemIdTypeId(item.id);

        protected static System.Type GetItemTypeById(int id)
        {

            int typeId = GetItemIdTypeId(id);

            foreach (System.Type t in typeIds.Keys)
                if (typeIds[t] == typeId)
                    return t;

            Debug.LogError("Unknown type id - " + typeId);
            return null;

        }

        protected static int GetItemTypeId(System.Type type)
        {
            
            foreach (System.Type t in typeIds.Keys)
                if (t == type)
                    return typeIds[t];
            
            Debug.LogError("Unknown type: " + type.FullName);
            return 0;

        }

        #endregion

        #region Registries

        public static Registry<Item> registry = new Registry<Item>();

        private static bool registrySet = false;

        public static Item GetItemById(System.Type type, int id)
            => GetItemById(GetItemTypeId(type) + id);

        public static Item GetItemById(int typeId, int id)
            => GetItemById(typeId + id);

        public static Item GetItemById(int id)
        {

            if (!registrySet)
                CreateRegistry();

            return registry.LinearSearch(id);

        }

        public static void TrySetRegistry()
        {
            if (!registrySet)
                CreateRegistry();
        }

        private static void CreateRegistry()
        {

            List<Item> items = new List<Item>();

            //TODO - once other item types added (eg. general, key items etc.), add them to registry here
            items.AddRange(PokeBall.GetRegistryItems());
            items.AddRange(BattleItem.GetRegistryItems());
            items.AddRange(MedicineItem.GetRegistryItems());
            items.AddRange(TMItem.GetRegistryItems());

            registry.SetValues(items.ToArray());

            registrySet = true;

        }

        #endregion

        #region Sprites

        public Sprite LoadSprite() => SpriteStorage.GetItemSprite(resourceName);

        #endregion

        #region Properties

        public int id;
        public int GetId() => id;

        /// <summary>
        /// The name to show to the user for the item
        /// </summary>
        public string itemName;

        /// <summary>
        /// The name of the item's resources used to get its sprite
        /// </summary>
        public string resourceName;

        /// <summary>
        /// A description of the item to provide the user with
        /// </summary>
        public string description;

        #endregion

        #region ItemUsageEffects

        public class ItemUsageEffects
        {

            public int healthRecovered = 0;
            public byte[] ppIncreases = new byte[4];
            public bool nvscCured = false;
            public Stats<sbyte> statModifierChanges = new Stats<sbyte>();
            public sbyte evasionModifierChange = 0;
            public sbyte accuracyModifierChange = 0;
            public bool increaseCritChance = false;

        }

        public abstract ItemUsageEffects GetUsageEffects(PokemonInstance pokemon);

        #endregion

        public abstract bool CheckCompatibility(PokemonInstance pokemon);

    }
}
