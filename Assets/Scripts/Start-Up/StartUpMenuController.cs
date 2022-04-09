using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Menus;

namespace StartUp
{
    public class StartUpMenuController : MenuController
    {

        [Header("Main Menu")]

        public GameObject defaultSelectedGO;

        public StartUpSaveSlotsController saveSlotsController;

        public Button newGameButton;

        protected override MenuSelectableController[] GetSelectables()
        {

            List<MenuSelectableController> selectables = new List<MenuSelectableController>();

            selectables.AddRange(saveSlotsController.GetAllSelectables());
            selectables.Add(newGameButton.GetComponent<MenuSelectableController>());

            return selectables.ToArray();

        }

        public void SetUp(Action<int> loadSaveSlotListener,
            Action loadAutosaveListener,
            Action newGameListener)
        {

            EventSystem.current.SetSelectedGameObject(defaultSelectedGO);

            saveSlotsController.SetUp((i) => loadSaveSlotListener?.Invoke(i),
                () => loadAutosaveListener?.Invoke());

            newGameButton.onClick.AddListener(() => newGameListener?.Invoke());

        }

    }
}