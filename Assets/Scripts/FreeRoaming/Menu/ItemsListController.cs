using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Menus;
using Items;

namespace FreeRoaming.Menu
{
    public class ItemsListController : ScrollListController<KeyValuePair<Item, int>>
    {

        public UnityEvent<int> itemIndexSelected;

        protected override GameObject GenerateListItem(KeyValuePair<Item, int> itemVs,
            int index)
        {

            GameObject newItem = Instantiate(scrollListItemPrefab, itemsListContentTransform);
            ItemsListItemController controller = newItem.GetComponent<ItemsListItemController>();

            newItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                itemIndexSelected.Invoke(index);
                SetCurrentSelectionIndex(index);
                itemSelectedAction(index);
            });

            controller.SetPositionIndex(index, itemsPadding);
            controller.SetValues(itemVs);

            return newItem;

        }

    }
}