using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Menus;
using Items;

namespace FreeRoaming.Menu
{
    public class ItemsListItemController : ScrollListItemController<KeyValuePair<Item, int>>
    {

        public Text textName;
        public Text textQuantity;
        public const string quantityPrefix = "x";

        public override void SetValues(KeyValuePair<Item, int> itemVs)
        {

            string name = itemVs.Key.itemName;
            int quantity = itemVs.Value;

            textName.text = name;
            if (quantity >= 0)
            {
                textQuantity.text = quantityPrefix + quantity.ToString();
            }
            else
            {
                textQuantity.text = "";
            }
        }

    }
}