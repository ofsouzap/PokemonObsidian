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

        #endregion

    }
}