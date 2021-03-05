using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle.PlayerUI;

namespace Battle.PlayerUI
{
    public class MenuBagController : MenuController
    {

        public Button buttonBack;
        public Button buttonBattleItems;
        public Button buttonPokeBalls;
        public Button buttonStatusItems;
        [InspectorName("Button HP PP Restore")]
        public Button buttonHPPPRestore;

        protected override MenuSelectableController[] GetSelectables() => new MenuSelectableController[]
        {
            buttonBack.GetComponent<MenuSelectableController>(),
            buttonBattleItems.GetComponent<MenuSelectableController>(),
            buttonPokeBalls.GetComponent<MenuSelectableController>(),
            buttonStatusItems.GetComponent<MenuSelectableController>(),
            buttonHPPPRestore.GetComponent<MenuSelectableController>()
        };

    }
}
