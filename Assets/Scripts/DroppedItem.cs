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
            Item.ItemType itemType;
            uint quantity;
            
            id = int.Parse(entry[0]);
            
            if (!Item.TryParseItemType(entry[1], out itemType))
            {
                Debug.LogError("Unable to parse item type for dropped item id " + id.ToString());
            }

            itemId = int.Parse(entry[2]);
            quantity = uint.Parse(entry[3]);

            droppedItemList.Add(new DroppedItem()
            {
                id = id,
                itemType = itemType,
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
    /// The type of the dropped item (used to get a type id)
    /// </summary>
    public Item.ItemType itemType;

    public int ItemTypeId => Item.GetItemTypeId(itemType);

    /// <summary>
    /// The dropped item's id excluding its type id
    /// </summary>
    public int itemId;

    /// <summary>
    /// How many of the dropped item to give the player
    /// </summary>
    public uint quantity = 1;

}
