using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StartUp.ChoosePlayerData.ChoosePlayerSprite
{
    public class ChoosePlayerSpriteController : ChoosePlayerDataController
    {

        public GameObject defaultEventSystemSelection;

        public SpriteOptionController[] spriteOptions;

        private byte GetPlayerSpriteIdBySpriteName(string s)
        {

            foreach (byte id in PlayerData.Profile.playerSpriteIdNames.Keys)
                if (PlayerData.Profile.playerSpriteIdNames[id] == s)
                    return id;

            throw new ArgumentException("Provided sprite name not present in player sprites dictionary (" + s +")");

        }

        private byte? selectedSpriteId = null;

        protected override IEnumerator MainCoroutine()
        {

            selectedSpriteId = null;

            if (defaultEventSystemSelection != null)
                EventSystem.current.SetSelectedGameObject(defaultEventSystemSelection);

            foreach (SpriteOptionController controller in spriteOptions)
            {

                string spriteName = controller.spriteName;

                controller.OnClick.AddListener(() => OnSpriteSelected(spriteName));

            }

            yield return new WaitUntil(() => selectedSpriteId != null);

            PlayerData.singleton.profile.spriteId = (byte)selectedSpriteId;

            CloseScene();

        }

        private void OnSpriteSelected(string spriteName)
        {

            selectedSpriteId = GetPlayerSpriteIdBySpriteName(spriteName);

        }

    }
}