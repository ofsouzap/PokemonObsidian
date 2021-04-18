using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items;

namespace FreeRoaming.Menu.PlayerMenus.BagMenu
{
    public class SectionsBarController : BagBarController
    {

        public GameObject[] sectionIcons;
        protected override GameObject[] GetItems()
            => sectionIcons;

        public override void SetUp(BagMenuController menuController, GameObject borderPrefab)
        {

            base.SetUp(menuController, borderPrefab);

            for (int i = 0; i < sectionIcons.Length; i++)
            {

                if (sectionIcons[i].GetComponent<Button>() == null)
                {
                    Debug.LogError("Section icon index " + i.ToString() + " has no Button component");
                    continue;
                }

                int iconIndex = i;
                sectionIcons[i].GetComponent<Button>().onClick.AddListener(() => menuController.SetCurrentSection(iconIndex));

            }

        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

    }
}