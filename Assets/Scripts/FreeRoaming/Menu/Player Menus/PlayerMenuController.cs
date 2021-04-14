using System.Collections;
using UnityEngine;
using Menus;

namespace FreeRoaming.Menu.Menus
{
    public abstract class PlayerMenuController : MenuController
    {

        private void Update()
        {
            
            if (Input.GetButtonDown("Menu") || Input.GetButtonDown("Cancel"))
            {
                GameSceneManager.ClosePlayerMenuScene();
            }

        }

    }
}
