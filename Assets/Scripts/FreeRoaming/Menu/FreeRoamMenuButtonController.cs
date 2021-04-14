using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Menus;

namespace FreeRoaming.Menu
{
    [RequireComponent(typeof(Button))]
    public class FreeRoamMenuButtonController : MenuSelectableController
    {

        /// <summary>
        /// The button's menu's scene to launch when it is selected. Can be in any format accepted by the Unity SceneManager
        /// </summary>
        public string menuSceneIdentifier;

        private FreeRoamMenuController freeRoamMenuController;

        public Button Button => GetComponent<Button>();

        public void SetUpListener(FreeRoamMenuController menuController)
        {

            freeRoamMenuController = menuController;

            Button.onClick.AddListener(() => freeRoamMenuController.LaunchMenuScene(menuSceneIdentifier));

        }

    }
}
