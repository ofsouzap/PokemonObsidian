using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Items;

namespace FreeRoaming.Menu.PlayerMenus.BagMenu
{
    public class ActionsBarController : BagBarController
    {

        public GameObject[] actionIcons;
        protected override GameObject[] GetItems()
            => actionIcons;

        public override void SetUp(BagMenuController menuController, GameObject borderPrefab)
        {

            base.SetUp(menuController, borderPrefab);

            for (int i = 0; i < actionIcons.Length; i++)
            {

                if (actionIcons[i].GetComponent<Button>() == null)
                {
                    Debug.LogError("Action icon index " + i.ToString() + " has no Button component");
                    continue;
                }

                int iconIndex = i;
                actionIcons[i].GetComponent<Button>().onClick.AddListener(() => menuController.OnActionChosen(iconIndex));

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