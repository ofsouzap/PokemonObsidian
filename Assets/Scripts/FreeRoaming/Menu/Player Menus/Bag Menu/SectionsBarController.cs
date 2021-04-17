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