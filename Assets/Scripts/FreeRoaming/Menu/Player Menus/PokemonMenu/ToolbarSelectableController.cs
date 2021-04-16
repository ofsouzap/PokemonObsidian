using System.Collections;
using UnityEngine;
using Menus;
using UnityEngine.EventSystems;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu
{
    public class ToolbarSelectableController : MenuSelectableController
    {

        public ToolbarController toolbarController;

        [SerializeField]
        protected byte index;

        public override void OnSelect(BaseEventData eventData)
        {

            base.OnSelect(eventData);

            toolbarController.SetDetailsPaneIndex(index);

        }

    }
}