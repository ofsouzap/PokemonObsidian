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

        public QuitGamePanelController quitGamePanelController;

        public GameObject selectableSelectionPrefab;

        public FreeRoamMenuButtonController[] mainButtons;

        public MenuSelectableController buttonBack;
        public MenuSelectableController buttonQuit;

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
            if (buttonQuit.GetComponent<Button>() == null) Debug.LogError("No Button component in buttonQuit");

            SetUp();

            quitGamePanelController.SetUp(
                cancelButtonPressAction: () => CancelQuitGame(),
                quitGameButtonPressAction: () => QuitGame());

            Hide();
            quitGamePanelController.Hide();

        }

        private void SetUp()
        {

            foreach (FreeRoamMenuButtonController mainButtonController in mainButtons)
                mainButtonController.SetUpListener(this);

            buttonBack.GetComponent<Button>().onClick.AddListener(() => CloseMenu());
            buttonQuit.GetComponent<Button>().onClick.AddListener(() => QuitButtonPressed());

        }

        #region Quit Game

        private void QuitButtonPressed()
        {
            quitGamePanelController.Show();
            EventSystem.current.SetSelectedGameObject(quitGamePanelController.GetDefaultSelectedGameObject());
        }

        private void CancelQuitGame()
        {
            quitGamePanelController.Hide();
            EventSystem.current.SetSelectedGameObject(buttonQuit.gameObject);
        }

        private void QuitGame()
        {
            Application.Quit();
        }

        #endregion

        #region Visibility

        public bool IsShown => isShown;

        public override void Show()
        {

            base.Show();

            EventSystem.current.SetSelectedGameObject(buttonBack.gameObject);

        }

        #endregion

        public void CloseMenu()
        {
            Hide();
            quitGamePanelController.Hide();
            FreeRoamSceneController.GetFreeRoamSceneController(gameObject.scene).SetSceneRunningState(true);
        }

        public void LaunchMenuScene(string sceneIdentifier)
        {

            Hide();

            GameSceneManager.LaunchPlayerMenuScene(sceneIdentifier);

        }

    }
}
