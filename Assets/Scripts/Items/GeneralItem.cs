using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;

namespace Items
{
    public class GeneralItem : Item
    {

        public bool usableFromBag;

        public override bool CanBeUsedFromBag()
            => usableFromBag;

        public Predicate<PokemonInstance> usageCompatibilityCheck = null;

        public override bool CheckCompatibility(PokemonInstance pokemon)
        {

            if (usageCompatibilityCheck == null)
            {
                Debug.LogError("No usage combatibility check set for item but compatibility being requested");
                return false;
            }
            else
                return usageCompatibilityCheck(pokemon);

        }

        public Func<PokemonInstance, ItemUsageEffects> itemUsageEffectsFunction = null;

        public override ItemUsageEffects GetUsageEffects(PokemonInstance pokemon)
        {

            if (itemUsageEffectsFunction == null)
            {
                Debug.LogError("No item usage effects function set for item but is being requested");
                return new ItemUsageEffects();
            }
            else
                return itemUsageEffectsFunction(pokemon);

        }

        #region Registry

        public static GeneralItem GetGeneralItemItemById(int id,
            bool addTypeId = false)
        {
            int queryId = addTypeId ? id + typeIdGeneral : id;
            return (GeneralItem)registry.LinearSearch(queryId);
        }

        public const string dataPath = "Data/generalItems";

        /* Data CSV columns:
         * id (int) (excluding general item type id)
         * name (string)
         * resource name (string)
         * description (string)
         * can be used from bag? (bool)
         */

        /// <summary>
        /// Loads the basic data of some general items from the CSV file and also sets any special properties of them then returns them
        /// </summary>
        public static Item[] GetRegistryItems()
        {

            List<Item> items = new List<Item>();

            string[][] stringData = CSV.ReadCSVResource(dataPath, true);

            foreach (string[] entry in stringData)
            {

                int id;
                string itemName, resourceName, description;
                bool usableFromBag;

                #region id

                if (!int.TryParse(entry[0], out id))
                {
                    Debug.LogError("Invalid entry id found - " + entry[0]);
                    id = -1;
                }

                id += typeIdGeneral;

                #endregion

                #region names and description

                itemName = entry[1];
                resourceName = entry[2];
                description = entry[3];

                #endregion

                #region usableFromBag

                switch (ParseBooleanProperty(entry[4]))
                {

                    case true:
                        usableFromBag = true;
                        break;

                    case false:
                    case null:
                        usableFromBag = false;
                        break;

                }

                #endregion

                items.Add(new GeneralItem()
                {
                    id = id,
                    itemName = itemName,
                    resourceName = resourceName,
                    description = description,
                    usableFromBag = usableFromBag
                });

            }

            #region Special Property Setting

            //Items should be searched for by id



            #endregion

            return items.ToArray();

        }

        private static bool? ParseBooleanProperty(string value) //Copied form PokemonMoveData
        {

            switch (value)
            {

                case "0":
                case "false":
                case "no":
                    return false;

                case "1":
                case "true":
                case "yes":
                    return true;

                default:
                    return null;

            }

        }

        #endregion

    }
}
