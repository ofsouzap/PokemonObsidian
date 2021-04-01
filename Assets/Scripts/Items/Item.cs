using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items;

namespace Items
{
    public abstract class Item : IHasId
    {

        #region Registry

        public static Registry<Item> registry = new Registry<Item>();

        public static Item GetItemById(int id)
        {
            return registry.StartingIndexSearch(id, id - 1);
        }

        #endregion

        #region Sprites

        private static Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        private static bool spritesLoaded = false;

        private const string itemSpritePrefix = "itemsprite_";

        /// <summary>
        /// Load the sprites if they haven't already been loaded
        /// </summary>
        public static void TryLoad()
        {
            if (!spritesLoaded)
                LoadAll();
        }

        private static void LoadAll()
        {

            List<Sprite> spritesToStore = new List<Sprite>();

            spritesToStore.AddRange(FreeRoaming.GameCharacterSpriteStorage.LoadSpriteSheet("sprite_sheet_itemsprites"));

            foreach (Sprite sprite in spritesToStore)
            {

                if (!sprites.ContainsKey(sprite.name))
                {
                    sprites.Add(sprite.name, sprite);
                }
                else
                {
                    Debug.LogError("Duplicate sprite name found - " + sprite.name);
                }

            }

            spritesLoaded = true;

        }

        public static Sprite LoadItemSprite(string itemResourceName)
        {

            string spriteName = itemSpritePrefix + itemResourceName;
            if (sprites.ContainsKey(spriteName))
                return sprites[spriteName];
            else
                return null;

        }

        public Sprite LoadItemSprite()
            => LoadItemSprite(resourceName);

        #endregion

        #region Type Ids

        public const int typeIdGeneral = 0x00000000;
        public const int typeIdPokeBall = 0x10000000;
        public const int typeIdBattleItem = 0x20000000;
        public const int typeIdMedicine = 0x30000000;
        public const int typeIdTM = 0x40000000;
        public const int typeIdBerry = 0x50000000;
        public const int typeIdKeyItem = 0x60000000;

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

    }
}