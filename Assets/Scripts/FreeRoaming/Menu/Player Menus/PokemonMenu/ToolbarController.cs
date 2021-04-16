using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu
{
    public class ToolbarController : MonoBehaviour
    {

        public PokemonMenuController pokemonMenuController;

        public Button backButton;
        public ToolbarSelectableController[] detailsPaneSelectables;

        public MenuSelectableController[] GetSelectables()
        {

            List<MenuSelectableController> selectables = new List<MenuSelectableController>();

            selectables.AddRange(detailsPaneSelectables);
            selectables.Add(backButton.GetComponent<MenuSelectableController>());

            return selectables.ToArray();

        }

        public void SetUp()
        {

            if (backButton.GetComponent<MenuSelectableController>() == null)
                Debug.LogError("No menu selectable controller on back button");

            backButton.onClick.AddListener(() => GameSceneManager.ClosePlayerMenuScene());

            foreach (ToolbarSelectableController s in detailsPaneSelectables)
                s.toolbarController = this;

        }

        public void SetDetailsPaneIndex(int index)
        {
            pokemonMenuController.SetCurrentDetailsPaneIndex(index);
        }

    }
}