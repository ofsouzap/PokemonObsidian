using System;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace FreeRoaming.Menu
{
    public class QuitGamePanelController : MenuController
    {

        public GameObject GetDefaultSelectedGameObject() => cancelButton.gameObject;

        public MenuSelectableController cancelButton;
        public MenuSelectableController quitGameButton;

        protected override MenuSelectableController[] GetSelectables()
        {
            return new MenuSelectableController[] { cancelButton, quitGameButton };
        }

        public void SetUp(Action cancelButtonPressAction,
            Action quitGameButtonPressAction) //FreeRoamMenuController calls this method
        {

            if (cancelButton.GetComponent<Button>() == null) Debug.LogError("No Button component on cancelButton");
            if (quitGameButton.GetComponent<Button>() == null) Debug.LogError("No Button component on quitGameButton");

            cancelButton.GetComponent<Button>().onClick.AddListener(() => cancelButtonPressAction?.Invoke());
            quitGameButton.GetComponent<Button>().onClick.AddListener(() => quitGameButtonPressAction?.Invoke());

        }

    }
}