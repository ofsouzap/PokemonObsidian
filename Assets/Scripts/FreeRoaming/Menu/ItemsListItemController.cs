using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items;

namespace FreeRoaming.Menu
{
    [RequireComponent(typeof(Button))]
    public class ItemsListItemController : MonoBehaviour
    {

        public Text textName;
        public Text textQuantity;
        public const string quantityPrefix = "x";

        public void SetPositionIndex(int index,
            float padding)
        {

            RectTransform rt = GetComponent<RectTransform>();

            float yAnchoredPos = -1 * (((2 * padding) + index * (rt.rect.height + padding)) + (rt.rect.height / 2));

            rt.anchoredPosition = new Vector2(
                rt.anchoredPosition.x,
                yAnchoredPos);

        }

        public void SetValues(string name,
            int quantity)
        {
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