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

        //Original item ids:
        //https://bulbapedia.bulbagarden.net/wiki/List_of_items_by_index_number_(Generation_IV)

        #region Type Ids

        public const int typeIdGeneral = 0x00000000;
        public const int typeIdPokeBall = 0x10000000;
        public const int typeIdBattleItem = 0x20000000;
        public const int typeIdMedicine = 0x30000000;
        public const int typeIdTM = 0x40000000;
        public const int typeIdBerry = 0x50000000;
        public const int typeIdKeyItem = 0x60000000;

        #region Enumerator

        public enum ItemType
        {
            General,
            PokeBall,
            BattleItem,
            Medicine,
            TM,
            Berry,
            KeyItem
        }

        private static Dictionary<ItemType, int> itemTypeIds = new Dictionary<ItemType, int>()
        {
            { ItemType.General, typeIdGeneral },
            { ItemType.PokeBall, typeIdPokeBall },
            { ItemType.BattleItem, typeIdBattleItem },
            { ItemType.Medicine, typeIdMedicine },
            { ItemType.TM, typeIdTM },
            { ItemType.Berry, typeIdBerry },
            { ItemType.KeyItem, typeIdKeyItem }
        };

        private static Dictionary<string, ItemType> itemTypeNames = new Dictionary<string, ItemType>()
        {
            { "general", ItemType.General },
            { "pokeBall", ItemType.PokeBall },
            { "battleItem", ItemType.BattleItem },
            { "medicine", ItemType.Medicine },
            { "tm", ItemType.TM },
            { "berry", ItemType.Berry },
            { "keyItem", ItemType.KeyItem }
        };

        public static int GetItemTypeId(ItemType itemType)
        {
            if (itemTypeIds.ContainsKey(itemType))
            {
                return itemTypeIds[itemType];
            }
            else
            {
                throw new ArgumentException("No item type id set for ItemType " + itemType);
            }
        }

        public static bool TryParseItemType(string s,
            out ItemType r)
        {

            if (itemTypeNames.ContainsKey(s))
            {
                r = itemTypeNames[s];
                return true;
            }
            else
            {
                r = default;
                return false;
            }

        }

        #endregion

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

            items.AddRange(PokeBall.GetRegistryItems());
            items.AddRange(BattleItem.GetRegistryItems());
            items.AddRange(MedicineItem.GetRegistryItems());
            items.AddRange(TMItem.GetRegistryItems());
            items.AddRange(GeneralItem.GetRegistryItems());

            Item[] itemsArray = items.ToArray();

            SetItemPrices(ref itemsArray);

            registry.SetValues(itemsArray);

            registrySet = true;

        }

        private static void SetItemPrices(ref Item[] items)
        {

            Dictionary<string, int> itemPrices = LoadItemPrices();

            foreach (Item item in items)
            {

                if (itemPrices.ContainsKey(item.resourceName))
                {
                    item.price = itemPrices[item.resourceName];
                }
                else if (item is TMItem)
                {
                    item.price = TMItem.defaultPrice;
                }
                else if (item is BattleItem)
                {
                    item.price = BattleItem.defaultPrice;
                }
                else
                {
                    Debug.LogWarning("No price found for item named " + item.resourceName);
                    item.price = 0;
                }

            }

        }

        //Item prices file should have two columns: item resource name and item price (respectively)
        //Resource names are used as item ids would have to include the item type prefix

        private const string itemPricesDataPath = "Data/itemPrices";

        private static Dictionary<string, int> LoadItemPrices()
        {

            string[][] data = CSV.ReadCSVResource(itemPricesDataPath, true);

            Dictionary<string, int> prices = new Dictionary<string, int>();

            foreach (string[] entry in data)
            {

                string itemName;
                int price;

                itemName = entry[0];

                if (prices.ContainsKey(itemName))
                {
                    Debug.LogError("Duplicated item name found: " + itemName);
                    continue;
                }

                if (!int.TryParse(entry[1], out price))
                {
                    Debug.LogError("Invalid price found for item named " + itemName);
                    price = 0;
                }

                if (price < 0)
                {
                    Debug.LogError("Price lesser than 0 found for item named " + itemName);
                    price = 0;
                }

                prices.Add(itemName, price);

            }

            return prices;

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

        /// <summary>
        /// How much the item should be bought or sold for
        /// </summary>
        public int price;

        /// <summary>
        /// A method to get whether this item can be used directly from the bag menu in free-roaming. This is usually specific to item types and so they can implement the method
        /// </summary>
        public abstract bool CanBeUsedFromBag();

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
            public byte friendshipGained = 0;

        }

        public abstract ItemUsageEffects GetUsageEffects(PokemonInstance pokemon);

        #endregion

        public abstract bool CheckCompatibility(PokemonInstance pokemon);

    }
}
