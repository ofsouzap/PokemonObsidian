using System;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace FreeRoaming.Menu.PlayerMenus.SaveMenu
{
    public class UISaveSlotController : MonoBehaviour
    {

        public const string noSaveFileDisplayText = "(No Data)";

        public Text saveSlotIndexText;
        public Text saveTimeText;
        public Button overwriteButton;
        public Button loadButton;

        private int saveSlotIndex;

        public void SetUp(int saveSlotIndex,
            Saving.LoadedData data,
            Action<int> overwriteListener,
            Action<int> loadListener)
        {

            this.saveSlotIndex = saveSlotIndex;

            saveSlotIndexText.text = this.saveSlotIndex.ToString();

            if (!data.dataExists)
            {
                saveTimeText.text = noSaveFileDisplayText;
            }
            else
            {
                saveTimeText.text = EpochTime.EpochTimeToDateTime(data.saveTime)
                    .ToString(Saving.saveTimeDisplayFormat);
            }

            overwriteButton.onClick.RemoveAllListeners();
            overwriteButton.onClick.AddListener(() => overwriteListener.Invoke(this.saveSlotIndex));

            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => loadListener.Invoke(this.saveSlotIndex));

        }

    }
}