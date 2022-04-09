using System;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace StartUp
{
    public class StartUpAutosaveSlot : MonoBehaviour
    {

        public Text saveTimeText;
        public Button loadButton;

        public void SetUp(string saveTimeDisplay,
            Action loadListener)
        {

            if (loadButton.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("Load button doesn't have MenuSelectableController");

            saveTimeText.text = saveTimeDisplay;

            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => loadListener?.Invoke());

        }

    }
}