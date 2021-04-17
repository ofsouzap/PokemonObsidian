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

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        //TODO

    }
}