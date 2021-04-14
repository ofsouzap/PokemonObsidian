using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace FreeRoaming.Menu
{
    public class FreeRoamMenuController : MenuController
    {

        public static FreeRoamMenuController singleton;

        public FreeRoamMenuButtonController[] mainButtons;

        public MenuSelectableController buttonBack;

        protected override MenuSelectableController[] GetSelectables()
        {
            List<MenuSelectableController> selectables = new List<MenuSelectableController>();
            selectables.AddRange(mainButtons);
            selectables.Add(buttonBack);
            return selectables.ToArray();
        }

        private void Awake()
        {

            if (singleton != null)
            {
                Debug.LogError("Free-roaming menu singleton already set. Destroying self.");
                Destroy(this);
            }
            else
            {
                singleton = this;
            }

        }

        private void Start()
        {

            foreach (FreeRoamMenuButtonController mainButtonController in mainButtons)
                if (mainButtonController.GetComponent<Button>() == null)
                    Debug.LogError("No Button component found in a main button");

            if (buttonBack.GetComponent<Button>() == null) Debug.LogError("No Button component in buttonBack");

            SetUp();

        }

        private void SetUp()
        {

            foreach (FreeRoamMenuButtonController mainButtonController in mainButtons)
                mainButtonController.SetUpListener(this);

        }

        public void LaunchMenuScene(string sceneIdentifier)
        {

            //TODO

        }

    }
}
