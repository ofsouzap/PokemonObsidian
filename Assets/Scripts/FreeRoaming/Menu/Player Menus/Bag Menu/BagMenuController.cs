using Menus;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Items;
using Items.MedicineItems;

namespace FreeRoaming.Menu.PlayerMenus.BagMenu
{
    public class BagMenuController : PlayerMenuController
    {

        public SectionsBarController sectionsBarController;
        public ActionsBarController actionsBarController;
        public ItemsListController itemsListController;
        public DisplayAreaController displayAreaController;
        public PokemonSelectionController pokemonSelectionController;

        protected override GameObject GetDefaultSelectedGameObject()
            => null;

        protected override MenuSelectableController[] GetSelectables()
            => new MenuSelectableController[0];

        public GameObject fullBorderPrefab;

        private enum Mode { Section, Action, ChoosePokemon }
        private Mode currentMode;

        private enum PokemonSelectionMode { Use, Give }
        private PokemonSelectionMode pokemonSelectionMode;

        private int currentSectionIndex;
        public int SectionCount => sectionsBarController.sectionIcons.Length;
        private int currentActionIndex;
        public int ActionCount => actionsBarController.actionIcons.Length;
        private int currentItemIndex;

        private Item[] currentItems;

        private Item CurrentItem => currentItems.Length > currentItemIndex ? currentItems[currentItemIndex] : null;

        protected override bool GetClosesOnCancel()
            => false;

        private TextBoxController textBoxController;

        protected override void SetUp()
        {

            textBoxController = TextBoxController.GetTextBoxController(gameObject.scene);

            currentSectionIndex = 0;
            currentActionIndex = 0;
            currentItemIndex = 0;
            currentItems = new Item[0];

            actionsBarController.SetUp(fullBorderPrefab);
            sectionsBarController.SetUp(fullBorderPrefab);
            itemsListController.SetUp(fullBorderPrefab);
            pokemonSelectionController.SetUp(this);

            ChangeToSectionSelection();

        }

        #region Control

        private float lastNavigationMove = float.MinValue;
        [SerializeField]
        private float navigationMoveDelay = 0.25f;

        private float lastSubmitUsage = float.MinValue;
        [SerializeField]
        private float submitUsageDelay = 0.1F;

        protected override void Update()
        {

            base.Update();

            if (Time.time - lastNavigationMove >= navigationMoveDelay)
            {

                if (currentMode == Mode.Section || currentMode == Mode.Action)
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

                        if (currentMode == Mode.Section)
                        {

                            ChangeItemIndex(Input.GetAxis("Vertical") > 0 ? (sbyte)-1 : (sbyte)1); //N.B. opposite directions used as items list indexing works in different direction

                            lastNavigationMove = Time.time;

                        }

                    }

                }

            }

            if (Input.GetButtonDown("Submit"))
            {

                if (Time.time - lastSubmitUsage >= submitUsageDelay)
                {

                    if (currentMode == Mode.Section)
                    {

                        ChangeToActionSelection();

                    }
                    else if (currentMode == Mode.Action)
                    {

                        switch (currentActionIndex)
                        {

                            case 0: //Cancel button
                                ChangeToSectionSelection(false);
                                break;

                            case 1: //Use button
                                if (CurrentItem.CanBeUsedFromBag())
                                {
                                    pokemonSelectionMode = PokemonSelectionMode.Use;
                                    ChangeToPokemonSelection();
                                }
                                else
                                {
                                    textBoxController.Show();
                                    textBoxController.SetTextInstant("You can't use this item");
                                    textBoxController.SetHideDelay(1.5F);
                                }
                                break;

                            case 2: //Give button
                                pokemonSelectionMode = PokemonSelectionMode.Give;
                                ChangeToPokemonSelection();
                                break;

                            default:
                                Debug.LogError("Unhandled currentActionIndex - " + currentActionIndex);
                                break;

                        }

                    }

                    lastSubmitUsage = Time.time;

                }

            }

            if (Input.GetButtonDown("Cancel"))
            {
                if (currentMode == Mode.Section)
                    CloseMenu();
                else if (currentMode == Mode.Action)
                    ChangeToSectionSelection(false);
                else if (currentMode == Mode.ChoosePokemon)
                    ChangeToActionSelection();
            }

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

        private void ChangeToSectionSelection(bool resetSelectionIndex = true)
        {

            currentMode = Mode.Section;

            sectionsBarController.Show();
            actionsBarController.Hide();
            pokemonSelectionController.Hide();

            if (resetSelectionIndex)
                currentSectionIndex = 0;

            SetCurrentSection(currentSectionIndex);

            lastSubmitUsage = Time.time;

        }

        private void ChangeToActionSelection()
        {

            currentMode = Mode.Action;

            sectionsBarController.Hide();
            actionsBarController.Show();
            pokemonSelectionController.Hide();

            currentActionIndex = 0;

            actionsBarController.SetCurrentSelectionIndex(0);

            lastSubmitUsage = Time.time;

        }

        private void ChangeToPokemonSelection()
        {

            currentMode = Mode.ChoosePokemon;

            pokemonSelectionController.Show();

            pokemonSelectionController.SetSelectableAsSelection();

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

        public void OnPokemonSelected(int index)
        {

            switch (pokemonSelectionMode)
            {

                case PokemonSelectionMode.Give:
                    OnPokemonSelected_TryGiveItem(index);
                    break;

                case PokemonSelectionMode.Use:
                    OnPokemonSelected_TryUseItem(index);
                    break;

                default:
                    Debug.LogError("Unhandled PokemonSelectionMode - " + pokemonSelectionMode);
                    break;

            }

        }

        #region Pokemon Selected

        private void OnPokemonSelected_TryGiveItem(int index)
        {

            PokemonInstance pokemon = PlayerData.singleton.partyPokemon[index];

            if (pokemon.heldItem != null)
            {
                textBoxController.Show();
                textBoxController.SetTextInstant("This pokemon is already holding an item");
                textBoxController.SetHideDelay(1.5F);
                return;
            }
            else
            {

                pokemon.heldItem = CurrentItem;
                PlayerData.singleton.inventory.RemoveItem(CurrentItem, 1);

                textBoxController.Show();
                textBoxController.SetTextInstant(pokemon.GetDisplayName() + " was given the " + CurrentItem.itemName + " to hold.");
                textBoxController.SetHideDelay(1.5F);

                ChangeToSectionSelection(false);

            }

        }

        private void OnPokemonSelected_TryUseItem(int index)
        {

            PokemonInstance pokemon = PlayerData.singleton.partyPokemon[index];

            if (CurrentItem is MedicineItem)
                OnPokemonSelected_UseItem_MedicineItem(pokemon);
            //TODO - add branches for other usable item types
            else
                Debug.LogError("Unhandled item (ID - " + CurrentItem.id + ")");

            ChangeToSectionSelection(false);

        }

        private void OnPokemonSelected_UseItem_MedicineItem(PokemonInstance pokemon)
        {

            //TODO - deal with single-move PP-restoring items (have user choose move to use item on)

            if (!CurrentItem.CheckCompatibility(pokemon))
            {
                textBoxController.Show();
                textBoxController.SetTextInstant("The " + CurrentItem.itemName + " won't have any effect on " + pokemon.GetDisplayName());
                textBoxController.SetHideDelay(1.5F);
            }
            else
            {

                Item.ItemUsageEffects itemUsageEffects = CurrentItem.GetUsageEffects(pokemon);

                if (itemUsageEffects.healthRecovered > 0)
                    pokemon.HealHealth(itemUsageEffects.healthRecovered);

                if (itemUsageEffects.nvscCured)
                    pokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;

                if (itemUsageEffects.ppIncreases.Any(x => x != 0))
                {

                    for (int i = 0; i < pokemon.movePPs.Length; i++)
                        pokemon.movePPs[i] += itemUsageEffects.ppIncreases[i];

                }

                textBoxController.Show();
                textBoxController.SetTextInstant("The " + CurrentItem.itemName + " was used on " + pokemon.GetDisplayName());
                textBoxController.SetHideDelay(1.5F);

            }

        }

        #endregion

    }
}
