using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StartUp.ChoosePlayerData.ChoosePlayerSprite
{
    public class ChoosePlayerSpriteController : GenericImageOptionsChoiceController<string>
    {

        private byte GetPlayerSpriteIdBySpriteName(string s)
        {

            foreach (byte id in PlayerData.Profile.playerSpriteIdNames.Keys)
                if (PlayerData.Profile.playerSpriteIdNames[id] == s)
                    return id;

            throw new ArgumentException("Provided sprite name not present in player sprites dictionary (" + s + ")");

        }

        protected override void SetPlayerDataValue(string value)
        {

            PlayerData.singleton.profile.spriteId = GetPlayerSpriteIdBySpriteName(value);

        }

    }
}