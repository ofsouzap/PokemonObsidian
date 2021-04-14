using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle.PlayerUI;
using Menus;

namespace Battle.PlayerUI
{
    public class MenuRootController : MenuController
    {

        public Button buttonRun;
        public Button buttonBag;
        public Button buttonParty;
        public Button buttonFight;

        private void Start()
        {

            if (buttonRun.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("No MenuSelectableController in run button");

            if (buttonBag.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("No MenuSelectableController in bag button");

            if (buttonParty.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("No MenuSelectableController in party button");

            if (buttonFight.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("No MenuSelectableController in fight button");

        }

        protected override MenuSelectableController[] GetSelectables() => new MenuSelectableController[]
        {
            buttonRun.GetComponent<MenuSelectableController>(),
            buttonBag.GetComponent<MenuSelectableController>(),
            buttonParty.GetComponent<MenuSelectableController>(),
            buttonFight.GetComponent<MenuSelectableController>()
        };

    }
}
