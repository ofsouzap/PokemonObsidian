using System.Collections.Generic;
using UnityEngine;
using Items;

public class DroppedItem : IHasId
{

    public const string dataFileResourcesPath = "Data/droppedItems";

    #region Registry

    public static Registry<DroppedItem> registry = new Registry<DroppedItem>();

    private static bool registryLoaded = false;

    public static void TryLoadRegistry()
    {
        if (!registryLoaded)
            LoadRegistry();
    }

    private static void LoadRegistry()
    {

        List<DroppedItem> droppedItemList = new List<DroppedItem>();

        string[][] droppedItemData = CSV.ReadCSVResource(dataFileResourcesPath, true);

        foreach (string[] entry in droppedItemData)
        {

            int id, itemId;
            uint quantity;
            
            id = int.Parse(entry[0]);

            itemId = int.Parse(entry[1]);
            quantity = uint.Parse(entry[2]);

            droppedItemList.Add(new DroppedItem()
            {
                id = id,
                itemId = itemId,
                quantity = quantity
            });

        }

        registry.SetValues(droppedItemList.ToArray());

        registryLoaded = true;

    }

    #endregion

    public int id;

    public int GetId() => id;

    /// <summary>
    /// The dropped item's id
    /// </summary>
    public int itemId;

    /// <summary>
    /// How many of the dropped item to give the player
    /// </summary>
    public uint quantity = 1;

}
