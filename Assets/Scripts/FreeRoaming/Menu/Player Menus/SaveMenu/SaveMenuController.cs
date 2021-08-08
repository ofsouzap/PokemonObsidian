using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace FreeRoaming.Menu.PlayerMenus.SaveMenu
{
    public class SaveMenuController : PlayerMenuController
    {

        public Button backButton;

        public UISaveSlotController[] uiSaveSlots;

        protected override GameObject GetDefaultSelectedGameObject()
            => backButton.gameObject;

        protected override MenuSelectableController[] GetSelectables()
        {

            List<MenuSelectableController> selectables = new List<MenuSelectableController>();

            selectables.Add(backButton.GetComponent<MenuSelectableController>());

            foreach (UISaveSlotController slotController in uiSaveSlots)
            {
                selectables.Add(slotController.overwriteButton.GetComponent<MenuSelectableController>());
                selectables.Add(slotController.loadButton.GetComponent<MenuSelectableController>());
            }

            return selectables.ToArray();

        }

        protected override void SetUp()
        {

            backButton.onClick.AddListener(CloseMenu);

            RefreshUISaveSlots();

        }

        private void RefreshUISaveSlots()
        {

            int slotIndex = 0;
            foreach (Saving.LoadedData data in Saving.LoadAllSlotDatas())
            {

                uiSaveSlots[slotIndex].SetUp(slotIndex,
                    data,
                    OverwriteSave,
                    LoadSave);

                slotIndex++;

            }

        }

        private void OverwriteSave(int slotIndex)
        {

            Saving.SaveData(slotIndex);
            RefreshUISaveSlots();
            
        }

        private void LoadSave(int slotIndex)
        {

            Saving.LoadedData data = Saving.LoadData(slotIndex);

            if (!data.dataExists)
                return;

            PlayerData.singleton = data.playerData;
            GameSceneManager.LoadSceneStack(data.sceneStack);

        }

    }
}