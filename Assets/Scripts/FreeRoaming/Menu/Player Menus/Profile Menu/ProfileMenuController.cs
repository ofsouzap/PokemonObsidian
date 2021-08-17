﻿using System;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace FreeRoaming.Menu.PlayerMenus.ProfileMenu
{
    public class ProfileMenuController : PlayerMenuController
    {

        public Button buttonBack;

        public Text textName;
        public Text textMoney;

        public Text textBadgesList;

        public Image cheatsUsedImage;

        public StatContainerController distanceWalkedContainer;
        public StatContainerController npcsTalkedContainer;
        public StatContainerController timePlayedContainer;
        public StatContainerController gameStartedContainer;

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

            distanceWalkedContainer.SetValue(PlayerData.singleton.stats.distanceWalked.ToString());
            npcsTalkedContainer.SetValue(PlayerData.singleton.stats.npcsTalkedTo.ToString());
            timePlayedContainer.SetValue(PlayerData.singleton.stats.timePlayed.ToString());
            gameStartedContainer.SetValue(GetGameStartedContainerContent());

            cheatsUsedImage.enabled = PlayerData.singleton.stats.cheatsUsed;

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

        protected string GetTimePlayerContainerContent(PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            return (player.stats.timePlayed / 60).ToString() + " mins";

        }

        protected string GetGameStartedContainerContent(PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            DateTime dt = EpochTime.EpochTimeToDateTime(Convert.ToInt64(player.stats.gameStartTime)).ToLocalTime();

            return dt.ToString("dd/MM/yyyy HH:mm");

        }

    }
}