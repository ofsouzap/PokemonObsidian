using System.Collections.Generic;
using UnityEngine;
using Items;

namespace FreeRoaming.PokeMart
{
    public static class PokeMartData
    {

        public const string dataResourcesPath = "Data/pokeMartInventories";

        public static Dictionary<int, int[]> pokeMartInventories { get; private set; }

        /* Data CSV Columns:
         * id (int)
         *     Negative ids should be used for data only for testing
         *     Positive ids should be usde for release
         *     0 should never be used
         * general item ids
         *     Item ids should be *without* their type id. This applies to all id fields in this file
         *     Seperated by ';'. This applies to all id fields in this file
         * medicine item ids
         * poke ball ids
         * tm item ids
         * battle item ids
         */

        public static void LoadData()
        {

            pokeMartInventories = new Dictionary<int, int[]>();

            string[][] entries = CSV.ReadCSVResource(dataResourcesPath, true);

            foreach (string[] entry in entries)
            {

                int id;
                int[] generalItemIds, medicineItemIds, pokeBallItemIds, tmItemIds, battleItemIds;

                if (!int.TryParse(entry[0], out id))
                {
                    Debug.LogError("Invalid id - " + entry[0]);
                    continue;
                }

                if (id == 0)
                {
                    Debug.LogError("Poke mart id for inventory CSV shouldn't be 0");
                    continue;
                }

                TryParseIdList(entry[1], out generalItemIds);
                TryParseIdList(entry[2], out medicineItemIds);
                TryParseIdList(entry[3], out pokeBallItemIds);
                TryParseIdList(entry[4], out tmItemIds);
                TryParseIdList(entry[5], out battleItemIds);

                if (!pokeMartInventories.ContainsKey(id))
                {

                    SetPokeMartInventory(id,
                        generalItems: generalItemIds,
                        medicineItems: medicineItemIds,
                        pokeBallItems: pokeBallItemIds,
                        tmItems: tmItemIds,
                        battleItems: battleItemIds);

                }
                else
                {

                    Debug.LogError("Duplicate poke mart inventory id found - " + id);

                }

            }

        }

        private static void TryParseIdList(string list, out int[] ids, char delimeter = ';')
        {

            List<int> idsList = new List<int>();

            foreach (string part in list.Split(delimeter))
            {
                if (!int.TryParse(part, out int newId))
                {
                    ids = new int[0];
                    Debug.LogError("Invalid item id found");
                    return;
                }
                else
                {
                    idsList.Add(newId);
                }
            }

            ids = idsList.ToArray();

        }

        private static void SetPokeMartInventory(int id,
            IEnumerable<int> generalItems,
            IEnumerable<int> medicineItems,
            IEnumerable<int> pokeBallItems,
            IEnumerable<int> tmItems,
            IEnumerable<int> battleItems
            )
        {

            List<int> itemIds = new List<int>();

            foreach (int itemId in generalItems)
                itemIds.Add(Item.typeIdGeneral + itemId);

            foreach (int itemId in medicineItems)
                itemIds.Add(Item.typeIdMedicine + itemId);

            foreach (int itemId in pokeBallItems)
                itemIds.Add(Item.typeIdPokeBall + itemId);

            foreach (int itemId in tmItems)
                itemIds.Add(Item.typeIdTM + itemId);

            foreach (int itemId in battleItems)
                itemIds.Add(Item.typeIdBattleItem + itemId);

            SetPokeMartInventory(id, itemIds.ToArray());

        }

        private static void SetPokeMartInventory(int id, int[] items)
        {

            if (pokeMartInventories.ContainsKey(id))
            {
                pokeMartInventories[id] = items;
            }
            else
            {
                pokeMartInventories.Add(id, items);
            }

        }

    }
}
