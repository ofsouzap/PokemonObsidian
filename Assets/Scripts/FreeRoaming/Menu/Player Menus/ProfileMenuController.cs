using Menus;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FreeRoaming.Menu.PlayerMenus
{
    public class ProfileMenuController : PlayerMenuController
    {

        public Text textName;
        public Text textMoney;

        protected override MenuSelectableController[] GetSelectables()
            => new MenuSelectableController[0];

        protected override void SetUp()
        {

            textName.text = PlayerData.singleton.profile.name;
            textMoney.text = "₽" + PlayerData.singleton.profile.money.ToString();

        }

    }
}
