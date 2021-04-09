using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

namespace Battle.PlayerUI
{
    public class MenuBagCategoryController : MenuController
    {

        public enum BagCategory
        {
            HPPPRestore,
            StatusRestore,
            PokeBalls,
            BattleItems
        }

        public Button buttonBack;
        public Button buttonPrevious;
        public Button buttonNext;

        public MenuButtonItemController[] itemButtons;

        private int ItemButtonCount { get => itemButtons.Length; }

        protected override MenuSelectableController[] GetSelectables()
        {
            MenuSelectableController[] output = new MenuSelectableController[itemButtons.Length + 3];
            Array.Copy(
                itemButtons,
                output,
                itemButtons.Length);
            output[output.Length - 3] = buttonBack.GetComponent<MenuSelectableController>();
            output[output.Length - 2] = buttonPrevious.GetComponent<MenuSelectableController>();
            output[output.Length - 1] = buttonNext.GetComponent<MenuSelectableController>();
            return output;
        }

        public void SetUp() { }

        public struct ItemButtonProperties { public bool isSet; public string name; public Sprite icon; public int quantity; }

        private void SetItemButtonProperties(ItemButtonProperties[] properties)
        {

            if (properties.Length != ItemButtonCount)
            {
                Debug.LogError("Invalid item button properties length");
                return;
            }

            for (int i = 0; i < properties.Length; i++)
            {

                if (properties[i].isSet)
                {

                    itemButtons[i].SetInteractable(true);
                    itemButtons[i].SetValues(properties[i].name, properties[i].icon, properties[i].quantity);

                }
                else
                {

                    itemButtons[i].SetInteractable(false);

                }

            }

        }

        #region Multi-Page Scrolling

        private int currentPageIndex = 0;

        private void RefreshPage()
        {

            ItemButtonProperties[] itemButtonProperties = new ItemButtonProperties[ItemButtonCount];
            Item[] pageItems = GetItemPage(currentPageIndex);

            for (int i = 0; i < ItemButtonCount; i++)
            {

                Item item = pageItems[i];

                if (item == null)
                    itemButtonProperties[i] = new ItemButtonProperties()
                    {
                        isSet = false
                    };
                else
                    itemButtonProperties[i] = new ItemButtonProperties()
                    {
                        isSet = true,
                        name = item.itemName,
                        icon = item.LoadSprite(),
                        quantity = PlayerData.singleton.inventory.GetItemInventorySection(item).GetQuantity(item.id)
                    };

            }

            SetItemButtonProperties(itemButtonProperties);

        }

        public void NextPage()
        {
            currentPageIndex = (currentPageIndex + 1) % ItemPageCount;
            RefreshPage();
        }

        public void PreviousPage()
        {
            currentPageIndex = (currentPageIndex - 1 + ItemPageCount) % ItemPageCount;
            RefreshPage();
        }

        #endregion

        #region Items

        private Item[] items;

        private int ItemPageCount
        {
            get => items.Length % ItemButtonCount == 0
                ? items.Length / ItemButtonCount
                : items.Length / ItemButtonCount + 1;
        }

        private Item[] GetItemPage(int index)
        {

            Item[] output = new Item[ItemButtonCount];
            Array.Copy(items,
                index * ItemButtonCount,
                output,
                0,
                items.Length - (index * ItemButtonCount));
            return output;

        }

        public void SetItems(Item[] items)
        {

            this.items = items;
            currentPageIndex = 0;
            RefreshPage();

        }

        public Item GetPageItem(int pageItemIndex)
            => GetItemPage(currentPageIndex)[pageItemIndex];

        #endregion

    }
}
