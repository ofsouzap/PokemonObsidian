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

            GameObject defaultGameObject = GetDefaultSelectedGameObject();
            
            if (defaultGameObject != null)
                EventSystem.current.SetSelectedGameObject(defaultGameObject);

        }

        protected abstract void SetUp();

        protected abstract GameObject GetDefaultSelectedGameObject();

        protected virtual bool GetClosesOnCancel() => true;

        protected virtual void Update()
        {
            
            if (Input.GetButtonDown("Menu") || (Input.GetButtonDown("Cancel") && GetClosesOnCancel()))
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
