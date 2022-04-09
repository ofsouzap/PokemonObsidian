using System;
using System.Collections.Generic;
using UnityEngine;
using Items;

namespace FreeRoaming.NPCs
{
    public class GenericNPCData
    {

        const string dataPath = "Data/genericNpcs";
        const bool ignoreDataFirstLine = true;

        #region Registry Data

        public class GenericNPCDetails : IHasId
        {

            public int id { get; private set; }

            public int GetId() => id;

            public string name { get; private set; }

            private string[] initialDialogs;
            private string[] mainDialogs;

            public string[] GetInitialDialogs()
                => initialDialogs != null && initialDialogs.Length > 0 ? initialDialogs : mainDialogs;

            public string[] GetMainDialogs()
                => mainDialogs;

            private int? itemGivenId;
            public Item ItemGiven
                => itemGivenId != null ? Item.GetItemById(id) : null;
            public uint itemGivenQuantity { get; private set; }

            public GenericNPCDetails(int id, string name, string[] initialDialogs, string[] mainDialogs, int? itemGivenId, uint itemGivenQuantity)
            {
                this.id = id;
                this.name = name;
                this.initialDialogs = initialDialogs;
                this.mainDialogs = mainDialogs;
                this.itemGivenId = itemGivenId;
                this.itemGivenQuantity = itemGivenQuantity;
            }

        }

        private static Registry<GenericNPCDetails> registry = new Registry<GenericNPCDetails>();

        public static GenericNPCDetails GetGenericNPCDetailsByNPCId(int id) => registry.BinarySearch(id);

        #endregion

        /* Data CSV Columns:
         * id (int)
         * npc name (string)
         * initial dialogs (strings separated by ';')
         *     the dialog messages the npc will use the first time that the player talks to them (if blank, the npc will always use the main dialogs)
         * main dialogs (strings separated by ';')
         *     the dialogs the npc will use all times but the first time the player talks to them
         * item given id (int or blank for none)
         * item given quantity (uint)
         */

        public static void LoadData()
        {

            List<GenericNPCDetails> details = new List<GenericNPCDetails>();

            string[][] data = CSV.ReadCSVResource(dataPath, ignoreDataFirstLine);

            foreach (string[] entry in data)
            {

                int id;
                string name;
                string[] initialDialogs, mainDialogs;
                int? itemGivenId;
                uint itemGivenQuantity;

                if (entry.Length < 6)
                {
                    Debug.LogWarning("Invalid generic npc details to load - " + entry);
                    continue;
                }

                //Id
                if (!int.TryParse(entry[0], out id))
                {
                    Debug.LogWarning("Invalid id for generic npc details - " + entry[0]);
                    continue;
                }

                //Name
                name = entry[1];

                //Initial dialogs
                string initialDialogsEntry = entry[2];
                if (initialDialogsEntry == "")
                    initialDialogs = new string[0];
                else
                    initialDialogs = initialDialogsEntry.Split(';');

                //Main dialogs
                mainDialogs = entry[3].Split(';');

                //Item given id
                string itemGivenIdEntry = entry[4];

                if (itemGivenIdEntry == "")
                    itemGivenId = null;
                else
                {
                    if (int.TryParse(itemGivenIdEntry, out int itemGivenIdNotNull))
                        itemGivenId = itemGivenIdNotNull;
                    else
                    {
                        itemGivenId = null;
                        Debug.LogError("Invalid item given id for generic npc details id - " + id);
                    }

                }

                //Item given quantity
                string itemGivenQuantityEntry = entry[5];

                if (itemGivenId == null)
                    itemGivenQuantity = 0;
                else if (itemGivenQuantityEntry == "")
                    itemGivenQuantity = 1;
                else if (!uint.TryParse(itemGivenQuantityEntry, out itemGivenQuantity))
                {
                    Debug.LogError("Invalid item given quantity for generic npc details id - " + id);
                    itemGivenQuantity = 1;
                }

                //Add to list
                details.Add(new GenericNPCDetails(
                    id: id,
                    name: name,
                    initialDialogs: initialDialogs,
                    mainDialogs: mainDialogs,
                    itemGivenId: itemGivenId,
                    itemGivenQuantity: itemGivenQuantity));

            }

            registry.SetValues(details.ToArray());

        }

    }
}
