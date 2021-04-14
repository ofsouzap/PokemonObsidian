using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Menus;

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
            if (buttonSave.GetComponent<Button>() == null) Debug.LogError("No Button component in buttonSave");

            SetUp();

        }

        private void SetUp()
        {

            foreach (FreeRoamMenuButtonController mainButtonController in mainButtons)
                mainButtonController.SetUpListener(this);

            buttonBack.GetComponent<Button>().onClick.AddListener(() => Hide());

            //TODO - save button listener

        }

        #region Visibility

        public bool IsShown => isShown;

        public override void Show()
        {

            base.Show();

            EventSystem.current.SetSelectedGameObject(buttonBack.gameObject);

        }

        #endregion

        public void LaunchMenuScene(string sceneIdentifier)
        {

            GameSceneManager.LaunchPlayerMenuScene(sceneIdentifier);

        }

    }
}
