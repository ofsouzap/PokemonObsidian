using Menus;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FreeRoaming.Menu.PlayerMenus
{
    public class ProfileMenuController : PlayerMenuController
    {

        public Button buttonBack;

        public Text textName;
        public Text textMoney;

        protected override MenuSelectableController[] GetSelectables()
            => new MenuSelectableController[0];

        protected override GameObject GetDefaultSelectedGameObject()
             => buttonBack.gameObject;

        protected override void SetUp()
        {

            buttonBack.onClick.AddListener(() => CloseMenu());

            textName.text = PlayerData.singleton.profile.name;
            textMoney.text = PlayerData.currencySymbol + PlayerData.singleton.profile.money.ToString();

        }

    }
}
