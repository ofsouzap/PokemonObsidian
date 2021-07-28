﻿using Menus;
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

        public Text textBadgesList;

        protected override MenuSelectableController[] GetSelectables()
            => new MenuSelectableController[0];

        protected override GameObject GetDefaultSelectedGameObject()
             => buttonBack.gameObject;

        protected override void SetUp()
        {

            buttonBack.onClick.AddListener(() => CloseMenu());

            textName.text = PlayerData.singleton.profile.name;
            textMoney.text = PlayerData.currencySymbol + PlayerData.singleton.profile.money.ToString();

            textBadgesList.text = GetBadgesListTextContent(PlayerData.singleton);

        }

        protected string GetBadgesListTextContent(PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            string output = "";

            foreach (int gymId in player.profile.defeatedGymIds)
            {

                Gym gym = Gym.registry.LinearSearch(gymId);

                output = output + gym.badgeName + "\n";

            }

            return output;

        }

    }
}
