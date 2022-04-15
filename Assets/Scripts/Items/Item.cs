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

        #region Registries

        public static Registry<Item> registry = new Registry<Item>();

        private static bool registrySet = false;

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

            Dictionary<string, ItemPrices> itemPrices = LoadItemPrices();

            foreach (Item item in items)
            {

                if (itemPrices.ContainsKey(item.resourceName))
                {
                    item.prices = itemPrices[item.resourceName];
                }
                else if (item is TMItem)
                {
                    item.prices = TMItem.defaultPrices;
                }
                else if (item is BattleItem)
                {
                    item.prices = BattleItem.defaultPrices;
                }
                else
                {
                    Debug.LogWarning("No price found for item named " + item.resourceName);
                    item.prices = ItemPrices.Zero;
                }

            }

        }

        //Item prices file should have two columns: item resource name and item price (respectively)
        //Resource names are used as item ids would have to include the item type prefix

        private const string itemPricesDataPath = "Data/itemPrices";

        public struct ItemPrices
        {

            public static ItemPrices Zero = new ItemPrices(0, 0);

            public int buyPrice;
            public int sellPrice;

            public bool CanBuy => buyPrice > 0;
            public bool CanSell => sellPrice > 0;

            public ItemPrices(int buyPrice, int sellPrice)
            {
                this.buyPrice = buyPrice;
                this.sellPrice = sellPrice;
            }

        }

        private static Dictionary<string, ItemPrices> LoadItemPrices()
        {

            string[][] data = CSV.ReadCSVResource(itemPricesDataPath, true);

            Dictionary<string, ItemPrices> prices = new Dictionary<string, ItemPrices>();

            foreach (string[] entry in data)
            {

                string itemName;
                int buyPrice, sellPrice;

                //ID

                itemName = entry[0];

                if (prices.ContainsKey(itemName))
                {
                    Debug.LogError("Duplicated item name found: " + itemName);
                    continue;
                }

                //Buy Price

                string buyPriceEntry = entry[1];

                if (buyPriceEntry == "")
                {
                    buyPrice = -1;
                }
                else
                {

                    if (!int.TryParse(buyPriceEntry, out buyPrice))
                    {
                        Debug.LogError("Invalid buy price found for item named " + itemName);
                        buyPrice = -1;
                    }

                    if (buyPrice < 0)
                    {
                        Debug.LogError("Price lesser than 0 found for item named " + itemName);
                        buyPrice = -1;
                    }

                }

                //Sell Price

                string sellPriceEntry = entry[2];

                if (sellPriceEntry == "")
                {
                    sellPrice = -1;
                }
                else
                {

                    if (!int.TryParse(sellPriceEntry, out sellPrice))
                    {
                        Debug.LogError("Invalid sell price found for item named " + itemName);
                        sellPrice = -1;
                    }

                    if (sellPrice < 0)
                    {
                        Debug.LogError("Price lesser than 0 found for item named " + itemName);
                        sellPrice = -1;
                    }

                }

                //Add

                prices.Add(itemName, new ItemPrices(buyPrice, sellPrice));

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
        /// How much the item can be bought and sold for
        /// </summary>
        public ItemPrices prices;

        /// <summary>
        /// How much the item should be bought for
        /// </summary>
        public int BuyPrice
            => prices.buyPrice;

        public bool CanBuy => prices.CanBuy;

        /// <summary>
        /// How much the item should be sold for
        /// </summary>
        public int SellPrice
            => prices.sellPrice;

        public bool CanSell => prices.CanSell;

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
