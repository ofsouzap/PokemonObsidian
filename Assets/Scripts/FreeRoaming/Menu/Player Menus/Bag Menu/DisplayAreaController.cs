using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items;

namespace FreeRoaming.Menu.PlayerMenus.BagMenu
{
    public class DisplayAreaController : MonoBehaviour
    {

        public Image iconImage;
        public Text descriptionText;

        public void SetItem(int itemId)
            => SetItem(Item.GetItemById(itemId));

        public void SetShownState(bool state)
        {

            if (state)
            {
                iconImage.gameObject.SetActive(true);
            }
            else
            {
                iconImage.gameObject.SetActive(false);
                descriptionText.text = "";
            }

        }
        
        public void SetItem(Item item)
        {

            if (item != null)
            {

                SetShownState(true);
                iconImage.sprite = item.LoadSprite();
                descriptionText.text = item.description;

            }
            else
            {

                SetShownState(false);

            }

        }

    }
}