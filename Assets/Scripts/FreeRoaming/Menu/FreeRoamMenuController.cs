using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Menus;
using FreeRoaming.AreaControllers;

namespace FreeRoaming.Menu
{
    public class FreeRoamMenuController : MenuController
    {

        public static FreeRoamMenuController singleton;

        public GameObject selectableSelectionPrefab;

        public FreeRoamMenuButtonController[] mainButtons;

        public MenuSelectableController buttonSave;
        public MenuSelectableController buttonBack;

        protected override MenuSelectableController[] GetSelectables()
        {
            List<MenuSelectableController> selectables = new List<MenuSelectableController>();
            selectables.AddRange(mainButtons);
            selectables.Add(buttonBack);
            return selectables.ToArray();
        }

        public void TrySetSingleton()
        {

            if (singleton != null && singleton != this)
            {
                Debug.LogError("Free-roaming menu singleton already set. Destroying self.");
                Destroy(this);
            }
            else
            {
                singleton = this;
            }

        }

        private void Awake()
        {

            TrySetSingleton();

        }

        private void Start()
        {

            foreach (FreeRoamMenuButtonController mainButtonController in mainButtons)
                if (mainButtonController.GetComponent<Button>() == null)
                    Debug.LogError("No Button component found in a main button");

            if (buttonBack.GetComponent<Button>() == null) Debug.LogError("No Button component in buttonBack");
            //if (buttonSave.GetComponent<Button>() == null) Debug.LogError("No Button component in buttonSave");

            SetUp();

            Hide();

        }

        private void SetUp()
        {

            foreach (FreeRoamMenuButtonController mainButtonController in mainButtons)
                mainButtonController.SetUpListener(this);

            buttonBack.GetComponent<Button>().onClick.AddListener(() => CloseMenu());

            //TODO - save button on-click listener

        }

        #region Visibility

        public bool IsShown => isShown;

        public override void Show()
        {

            base.Show();

            EventSystem.current.SetSelectedGameObject(buttonBack.gameObject);

        }

        #endregion

        private void CloseMenu()
        {
            Hide();
            FreeRoamSceneController.GetFreeRoamSceneController(gameObject.scene).SetSceneRunningState(true);
        }

        public void LaunchMenuScene(string sceneIdentifier)
        {

            Hide();

            GameSceneManager.LaunchPlayerMenuScene(sceneIdentifier);

        }

    }
}
