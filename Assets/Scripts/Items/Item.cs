using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;

namespace Items
{
    public abstract class Item : IHasId
    {

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
