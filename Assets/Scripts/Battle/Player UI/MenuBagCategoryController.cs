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

        public Button buttonBack;
        public Button buttonPrevious;
        public Button buttonNext;

        public Button[] itemButtons;
        private Text[] itemButtonTexts;
        private Image[] itemButtonImages;

        private int ItemButtonCount { get => itemButtons.Length; }

        protected override MenuSelectableController[] GetSelectables()
        {
            MenuSelectableController[] output = new MenuSelectableController[itemButtons.Length + 3];
            Array.Copy(
                itemButtons.Select(x => x.GetComponent<MenuSelectableController>()).ToArray(),
                output,
                itemButtons.Length);
            output[output.Length - 3] = buttonBack.GetComponent<MenuSelectableController>();
            output[output.Length - 2] = buttonPrevious.GetComponent<MenuSelectableController>();
            output[output.Length - 1] = buttonNext.GetComponent<MenuSelectableController>();
            return output;
        }

        public void SetUp()
        {

            itemButtonTexts = new Text[itemButtons.Length];
            itemButtonImages = new Image[itemButtons.Length];

            for (int i = 0; i < itemButtons.Length; i++)
            {

                Text _text = itemButtons[i].GetComponentInChildren<Text>();
                Image _image = itemButtons[i].GetComponentInChildren<Image>();

                if (_text == null || _image == null)
                {
                    Debug.LogError("Invalid item button cihld format for index " + i);
                    continue;
                }

                itemButtonTexts[i] = _text;
                itemButtonImages[i] = _image;

            }

        }

        public struct ItemButtonProperties { public bool isSet; public string name; public Sprite icon; }

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

                    itemButtonTexts[i].text = properties[i].name;
                    itemButtonImages[i].gameObject.SetActive(true);
                    itemButtonImages[i].sprite = properties[i].icon;
                    itemButtons[i].interactable = true;

                }
                else
                {

                    itemButtonTexts[i].text = "";
                    itemButtonImages[i].gameObject.SetActive(false);
                    itemButtons[i].interactable = false;

                }

            }

        }

        #region Multi-Page Scrolling

        private int currentPageIndex = 0;

        private void RefreshPage()
        {

            ItemButtonProperties[] itemButtonProperties = new ItemButtonProperties[itemButtons.Length];
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
                        icon = item.LoadSprite()
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
                ItemButtonCount);
            return output;

        }

        public void SetItems(Item[] items)
        {

            this.items = items;
            currentPageIndex = 0;
            RefreshPage();

        }

        #endregion

    }
}
