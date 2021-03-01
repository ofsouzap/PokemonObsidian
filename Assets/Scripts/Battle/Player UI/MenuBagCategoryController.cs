using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle.PlayerUI;

namespace Battle.PlayerUI
{
    public class MenuBagCategoryController : MonoBehaviour
    {

        public Button buttonBack;
        public Button buttonPrevious;
        public Button buttonNext;

        public Button[] itemButtons;
        private Text[] itemButtonTexts;
        private Image[] itemButtonImages;

        private void Start()
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

        public struct ItemButtonProperties { public bool isSet; public string name; public string iconPath; }

        public void SetItemButtonProperties(ItemButtonProperties[] properties)
        {

            if (properties.Length != itemButtons.Length)
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
                    itemButtonImages[i].sprite = (Sprite)Resources.Load(properties[i].iconPath);

                }
                else
                {

                    itemButtonTexts[i].text = "";
                    itemButtonImages[i].gameObject.SetActive(false);

                }

            }

        }

    }
}
