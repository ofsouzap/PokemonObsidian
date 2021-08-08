using System;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace StartUp
{
    public class StartUpSaveSlot : MonoBehaviour
    {

        public const string noSaveFileDisplayText = "(No Data)";

        public Text slotIndexText;
        public Text saveTimeText;
        public Button loadButton;

        private int slotIndex;

        public void SetUp(int slotIndex,
            string saveTimeDisplay,
            Action<int> loadListener)
        {

            if (loadButton.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("Load button doesn't have MenuSelectableController");

            this.slotIndex = slotIndex;

            slotIndexText.text = this.slotIndex.ToString();
            saveTimeText.text = saveTimeDisplay;

            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => loadListener?.Invoke(this.slotIndex));

        }

    }
}