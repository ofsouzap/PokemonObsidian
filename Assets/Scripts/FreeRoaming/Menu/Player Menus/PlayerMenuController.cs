using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Menus;

namespace FreeRoaming.Menu.PlayerMenus
{
    public abstract class PlayerMenuController : MenuController
    {

        [Tooltip("Any canvases that are part of the menu. These are hidden when the menu needs to be hidden")]
        public Canvas[] mainCanvases;
        private List<Canvas> canvasesToShowOnMenuShow = new List<Canvas>();

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

        protected virtual void HideMenu()
        {
            foreach (Canvas canvas in mainCanvases)
            {
                if (canvas.enabled)
                {
                    canvasesToShowOnMenuShow.Add(canvas);
                    canvas.enabled = false;
                }
            }
        }

        protected virtual void ShowMenu()
        {

            foreach (Canvas canvas in canvasesToShowOnMenuShow)
            {
                canvas.enabled = true;
            }

            canvasesToShowOnMenuShow.Clear();

        }

        protected virtual void CloseMenu()
        {

            EventSystem.current.enabled = false; //So that it doesn't interfere with any EventSystem that will be loaded

            GameSceneManager.ClosePlayerMenuScene();

        }

    }
}
