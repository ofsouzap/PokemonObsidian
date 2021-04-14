using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Menus;

namespace FreeRoaming.Menu.PlayerMenus
{
    public abstract class PlayerMenuController : MenuController
    {

        protected virtual void Start()
        {

            SetUp();

        }

        protected abstract void SetUp();

        protected virtual void Update()
        {
            
            if (Input.GetButtonDown("Menu") || Input.GetButtonDown("Cancel"))
            {
                CloseMenu();
            }

        }

        protected virtual void CloseMenu()
        {

            EventSystem.current.enabled = false; //So that it doesn't interfere with any EventSystem that will be loaded

            GameSceneManager.ClosePlayerMenuScene();

        }

    }
}
