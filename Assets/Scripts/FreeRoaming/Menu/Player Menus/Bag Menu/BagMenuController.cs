using Menus;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Items;

namespace FreeRoaming.Menu.PlayerMenus.BagMenu
{
    public class BagMenuController : PlayerMenuController
    {

        public SectionsBarController sectionsBarController;
        public ActionsBarController actionsBarController;
        public ItemsListController itemsListController;
        public DisplayAreaController displayAreaController;

        protected override GameObject GetDefaultSelectedGameObject()
            => null;

        protected override MenuSelectableController[] GetSelectables()
            => new MenuSelectableController[0];

        public GameObject fullBorderPrefab;

        private enum Mode { Section, Action }
        private Mode currentMode;

        private int currentSectionIndex;
        public int SectionCount => sectionsBarController.sectionIcons.Length;
        private int currentActionIndex;
        public int ActionCount => actionsBarController.actionIcons.Length;
        private int currentItemIndex;

        private Item[] currentItems;

        private Item CurrentItem => currentItems.Length > currentItemIndex ? currentItems[currentItemIndex] : null;

        protected override void SetUp()
        {

            currentSectionIndex = 0;
            currentActionIndex = 0;
            currentItemIndex = 0;
            currentItems = new Item[0];

            actionsBarController.SetUp(fullBorderPrefab);
            sectionsBarController.SetUp(fullBorderPrefab);
            itemsListController.SetUp(fullBorderPrefab);

            ChangeToSectionSelection();

        }

        #region Control

        private float lastNavigationMove = float.MinValue;
        [SerializeField]
        private float navigationMoveDelay = 0.25f;

        protected override void Update()
        {

            base.Update();

            if (Time.time - lastNavigationMove >= navigationMoveDelay)
            {

                if (Input.GetAxis("Horizontal") != 0)
                {

                    sbyte moveAmount = Input.GetAxis("Horizontal") > 0 ? (sbyte)1 : (sbyte)-1;

                    if (currentMode == Mode.Section)
                        ChangeSectionIndex(moveAmount);
                    else if (currentMode == Mode.Action)
                        ChangeActionIndex(moveAmount);

                    lastNavigationMove = Time.time;

                }

                if (Input.GetAxis("Vertical") != 0 && currentItems.Length > 0)
                {

                    if (currentMode == Mode.Action)
                        ChangeToSectionSelection();

                    ChangeItemIndex(Input.GetAxis("Vertical") > 0 ? (sbyte)-1 : (sbyte)1); //N.B. opposite directions used as items list indexing works in different direction

                    lastNavigationMove = Time.time;

                }

            }

            //TODO - selecting current item (Input.GetButtonDown("Submit"))

        }

        private void ChangeSectionIndex(sbyte amount)
        {

            currentSectionIndex = (currentSectionIndex + amount + SectionCount) % SectionCount;

            SetCurrentSection(currentSectionIndex);

        }

        private void ChangeActionIndex(sbyte amount)
        {

            currentActionIndex = (currentActionIndex + amount + ActionCount) % ActionCount;

            SetCurrentAction(currentActionIndex);

        }

        private void ChangeItemIndex(sbyte amount)
        {

            currentItemIndex = (currentItemIndex + amount + currentItems.Length) % currentItems.Length;

            RefreshCurrentItem();

        }

        #endregion

        private void ChangeToSectionSelection()
        {

            currentMode = Mode.Section;

            //TODO

            sectionsBarController.Show();
            actionsBarController.Hide();

            SetCurrentSection(0);

        }

        private void ChangeToActionSelection()
        {

            currentMode = Mode.Action;

            //TODO

            sectionsBarController.Hide();
            actionsBarController.Show();
            actionsBarController.SetCurrentSelectionIndex(0);

        }

        private void SetCurrentSection(int index)
        {

            currentSectionIndex = index;
            sectionsBarController.SetCurrentSelectionIndex(index);
            SetItemsList(index);

        }

        public void SetCurrentAction(int index)
        {

            currentActionIndex = index;
            actionsBarController.SetCurrentSelectionIndex(index);
            //TODO

        }

        private void RefreshCurrentItem()
        {

            if (CurrentItem != null)
            {
                displayAreaController.SetShownState(true);
                displayAreaController.SetItem(CurrentItem);
                itemsListController.SetCurrentSelectionIndex(currentItemIndex);
            }
            else
            {
                displayAreaController.SetShownState(false);
            }

        }

        private void SetItemsList(int sectionIndex)
        {

            PlayerData.Inventory.InventorySection inventorySection = GetInventorySectionByIndex(PlayerData.singleton.inventory, sectionIndex);

            if (inventorySection == null)
            {
                Debug.LogError("No inventory section found for index " + sectionIndex);
                return;
            }

            Item[] items = inventorySection.GetItems();
            int[] quantities = new int[items.Length];

            for (int i = 0; i < items.Length; i++)
                quantities[i] = inventorySection.GetQuantity(items[i].id);

            SetItemsList(items, quantities);

        }

        private static PlayerData.Inventory.InventorySection GetInventorySectionByIndex(PlayerData.Inventory inventory,
            int index)
            => index switch
            {
                0 => inventory.medicineItems,
                1 => inventory.pokeBalls,
                2 => inventory.tmItems,
                3 => inventory.battleItems,
                _ => null
            };

        private void SetItemsList(Item[] items,
            int[] quantities)
        {

            currentItems = items;
            itemsListController.SetItems(items, quantities);
            currentItemIndex = 0;
            RefreshCurrentItem();

        }

    }
}
