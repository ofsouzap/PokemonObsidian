using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Menus;

namespace StartUp
{
    public class StartUpSaveSlotsController : MenuController
    {

        public StartUpSaveSlot[] saveSlots;
        public StartUpAutosaveSlot autosaveSlot;

        public MenuSelectableController[] GetAllSelectables() => GetSelectables(); //For StartUpSceneController

        protected override MenuSelectableController[] GetSelectables()
        {

            List<MenuSelectableController> selectables = new List<MenuSelectableController>();
            selectables.AddRange(saveSlots.Select(x => x.loadButton.GetComponent<MenuSelectableController>()).Where(x => x != null));
            return selectables.ToArray();

        }

        public void SetUp(Action<int> loadSaveSlotListener,
            Action loadAutosaveListener)
        {

            int saveIndex = 0;
            foreach (Saving.LoadedData data in Saving.LoadAllSlotDatas())
            {

                saveSlots[saveIndex].SetUp(saveIndex,
                    data.StatusMessage,
                    data.status == Saving.LoadedData.Status.Success ? loadSaveSlotListener : null);
                saveIndex++;

            }

            Saving.LoadedData autosaveData = Saving.LoadAutosave();
            autosaveSlot.SetUp(autosaveData.StatusMessage,
                autosaveData.status == Saving.LoadedData.Status.Success ? loadAutosaveListener : null);

        }

    }
}
