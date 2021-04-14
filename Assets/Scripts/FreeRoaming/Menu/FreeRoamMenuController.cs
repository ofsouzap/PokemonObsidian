using System.Collections;
using UnityEngine;
using Menus;

namespace FreeRoaming.Menu
{
    public class FreeRoamMenuController : MenuSelectableController
    {

        public static FreeRoamMenuController singleton;

        public MenuSelectableController buttonPokemon;
        public MenuSelectableController buttonPokedex;
        public MenuSelectableController buttonBag;
        public MenuSelectableController buttonSave;
        public MenuSelectableController buttonSettings;

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
            
            //TODO - check that each button has a Button component

        }

    }
}