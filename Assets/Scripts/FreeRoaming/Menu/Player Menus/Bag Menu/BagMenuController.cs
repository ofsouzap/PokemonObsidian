using Menus;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Pokemon.Moves;
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

            actionsBarController.SetUp(this, fullBorderPrefab);
            sectionsBarController.SetUp(this, fullBorderPrefab);
            itemsListController.SetUp(fullBorderPrefab, (index) => OnSelectItem(index));
            pokemonSelectionController.SetUp(this);

            ChangeToSectionSelection();

        }

        #region Control

        private bool controlAllowed = true;

        private float lastNavigationMove = float.MinValue;
        [SerializeField]
        private float navigationMoveDelay = 0.25f;

        private float lastSubmitUsage = float.MinValue;
        [SerializeField]
        private float submitUsageDelay = 0.1F;

        protected override void Update()
        {

            base.Update();
            
            if (controlAllowed)
            {

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

                            OnSelectItem(currentItemIndex);

                        }
                        else if (currentMode == Mode.Action)
                        {

                            OnActionChosen(currentActionIndex);

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

        public void OnActionChosen(int chosenActionIndex)
        {

            currentActionIndex = chosenActionIndex;

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

        public void OnSelectItem(int index)
        {
            currentItemIndex = index;
            RefreshCurrentItem();
            ChangeToActionSelection();
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

        public void SetCurrentSection(int index)
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
                0 => inventory.generalItems,
                1 => inventory.medicineItems,
                2 => inventory.pokeBalls,
                3 => inventory.tmItems,
                4 => inventory.battleItems,
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

        #region Pokemon Selected

        public void OnPokemonSelected(int index)
        {

            pokemonSelectionController.Hide();

            switch (pokemonSelectionMode)
            {

                case PokemonSelectionMode.Give:
                    OnPokemonSelected_TryGiveItem(index);
                    break;

                case PokemonSelectionMode.Use:
                    StartCoroutine(OnPokemonSelected_TryUseItem(index));
                    break;

                default:
                    Debug.LogError("Unhandled PokemonSelectionMode - " + pokemonSelectionMode);
                    break;

            }

        }

        private IEnumerator AwaitUserSelectMove(PokemonInstance pokemon,
            bool autoChangeControlAllowed = true,
            bool autoPromptUser = true,
            bool autoHideTextBox = false)
        {

            if (autoChangeControlAllowed)
                controlAllowed = false;

            string[] options = new string[pokemon.moveIds.Count(x => !PokemonMove.MoveIdIsUnset(x)) + 1];

            options[0] = "Cancel";

            int optionsIndex = 1;
            for (int i = 0; i < pokemon.moveIds.Length; i++)
                if (!PokemonMove.MoveIdIsUnset(pokemon.moveIds[i]))
                {
                    options[optionsIndex] = PokemonMove.GetPokemonMoveById(pokemon.moveIds[i]).name;
                    optionsIndex++;
                }

            if (autoPromptUser)
                textBoxController.SetTextInstant("Choose a move");

            textBoxController.Show();
            yield return StartCoroutine(textBoxController.GetUserChoice(options));
            if (autoHideTextBox)
                textBoxController.Hide();

            if (autoChangeControlAllowed)
                controlAllowed = true;

        }

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

        private IEnumerator OnPokemonSelected_TryUseItem(int index)
        {

            //Must be coroutine so that currentItemIndex isn't reset until item usage fully dealt with

            PokemonInstance pokemon = PlayerData.singleton.partyPokemon[index];

            if (CurrentItem is GeneralItem)
            {

                PokemonSpecies.Evolution evolutionFound = pokemon.TryFindEvolution(false, CurrentItem.id);

                if (evolutionFound != null)
                    yield return StartCoroutine(OnPokemonSelected_UseItem_EvolvePokemon(pokemon, evolutionFound));
                else
                {

                    //TODO - deal with special usages for some general items

                    //If the selected item can't be used on the selected pokemon
                    textBoxController.Show();
                    textBoxController.RevealText("The " + CurrentItem.itemName + " can't be used on " + pokemon.GetDisplayName());
                    yield return new WaitForFixedUpdate();
                    yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());
                    textBoxController.Hide();

                }

            }
            else if (CurrentItem is MedicineItem)
                yield return StartCoroutine(OnPokemonSelected_UseItem_MedicineItem(pokemon));
            else if (CurrentItem is TMItem)
                yield return StartCoroutine(OnPokemonSelected_UseItem_TMItem(pokemon));
            else
                Debug.LogError("Unhandled item (ID - " + CurrentItem.id + ")");

            ChangeToSectionSelection(false);

        }

        private IEnumerator OnPokemonSelected_UseItem_EvolvePokemon(PokemonInstance pokemon, PokemonSpecies.Evolution evolution)
        {

            EvolutionScene.EvolutionSceneController.entranceArguments = new EvolutionScene.EvolutionSceneController.EntranceArguments()
            {
                displayName = pokemon.GetDisplayName(),
                startSpeciesId = pokemon.speciesId,
                endSpeciesId = evolution.targetId,
                useFemaleSprite = pokemon.gender == false
            };

            pokemon.Evolve(evolution);

            controlAllowed = false;
            HideMenu();

            GameSceneManager.LaunchEvolutionScene();

            bool canContinueAfterEvolution = false;
            GameSceneManager.EvolutionSceneClosed += () =>
            {
                canContinueAfterEvolution = true;
            };

            yield return new WaitUntil(() => canContinueAfterEvolution);

            ShowMenu();
            controlAllowed = true;

        }

        private IEnumerator OnPokemonSelected_UseItem_MedicineItem(PokemonInstance pokemon)
        {

            textBoxController.Show();

            if (!CurrentItem.CheckCompatibility(pokemon))
            {
                textBoxController.RevealText("The " + CurrentItem.itemName + " won't have any effect on " + pokemon.GetDisplayName());
                yield return new WaitForFixedUpdate();
                yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());
            }
            else
            {

                if (CurrentItem is PPRestoreMedicineItem CurrentPPRestoreItem && CurrentPPRestoreItem.isForSingleMove)
                {

                    yield return StartCoroutine(AwaitUserSelectMove(pokemon));

                    if (textBoxController.userChoiceIndexSelected == 0) //Index 0 is always cancel
                    {
                        textBoxController.Hide();
                        yield break;
                    }

                    int moveIndexSelected = textBoxController.userChoiceIndexSelected - 1;

                    int trueMoveIndexSelected = -1;
                    int unsetMovesFound = 0;

                    //Explained in TM-using
                    for (int i = moveIndexSelected; i < pokemon.moveIds.Length; i++)
                    {

                        if (PokemonMove.MoveIdIsUnset(pokemon.moveIds[i]))
                            unsetMovesFound++;

                        if (i - unsetMovesFound == moveIndexSelected)
                        {
                            trueMoveIndexSelected = i;
                            break;
                        }

                    }

                    if (trueMoveIndexSelected < 0)
                    {
                        Debug.LogError("Unable to find trueMoveIndexSelected");
                        textBoxController.Hide();
                        yield break;
                    }

                    if (pokemon.movePPs[trueMoveIndexSelected] >= PokemonMove.GetPokemonMoveById(pokemon.moveIds[trueMoveIndexSelected]).maxPP)
                    {
                        textBoxController.RevealText("This move can't have any more PP");
                        yield return new WaitForFixedUpdate();
                        yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());
                        textBoxController.Hide();
                        yield break;
                    }
                    else
                    {
                        PPRestoreMedicineItem.singleMoveIndexToRecoverPP = trueMoveIndexSelected;
                    }

                }

                PlayerData.singleton.inventory.RemoveItem(CurrentItem, 1);

                Item.ItemUsageEffects itemUsageEffects = CurrentItem.GetUsageEffects(pokemon);

                pokemon.AddFriendship(itemUsageEffects.friendshipGained);

                if (itemUsageEffects.healthRecovered > 0)
                    pokemon.HealHealth(itemUsageEffects.healthRecovered);

                if (itemUsageEffects.nvscCured)
                    pokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;

                if (itemUsageEffects.ppIncreases.Any(x => x != 0))
                {

                    for (int i = 0; i < pokemon.movePPs.Length; i++)
                        pokemon.movePPs[i] += itemUsageEffects.ppIncreases[i];

                }

                textBoxController.RevealText("The " + CurrentItem.itemName + " was used on " + pokemon.GetDisplayName());
                yield return new WaitForFixedUpdate();
                yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            }

            textBoxController.Hide();

        }

        private IEnumerator OnPokemonSelected_UseItem_TMItem(PokemonInstance pokemon)
        {

            controlAllowed = false;

            textBoxController.Show();

            if (!CurrentItem.CheckCompatibility(pokemon))
            {

                textBoxController.RevealText(pokemon.GetDisplayName() + " can't learn this move");
                yield return new WaitForFixedUpdate(); //Explanation somewhere below
                yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            }
            else if (pokemon.moveIds.Any(x => PokemonMove.MoveIdIsUnset(x)))
            {

                PokemonMove newMove = ((TMItem)CurrentItem).Move;

                int newMoveIndex = -1;
                for (int i = 0; i < pokemon.moveIds.Length; i++)
                    if (PokemonMove.MoveIdIsUnset(pokemon.moveIds[i]))
                    {
                        newMoveIndex = i;
                        break;
                    }

                if (newMoveIndex < 0)
                {
                    Debug.LogError("Unset move couldn't be re-found");
                    yield break;
                }

                pokemon.moveIds[newMoveIndex] = newMove.id;
                pokemon.movePPs[newMoveIndex] = newMove.maxPP;

                pokemon.AddFriendshipGainForTMUsage();

                textBoxController.RevealText(pokemon.GetDisplayName() + " learnt " + newMove.name + "!");
                yield return new WaitForFixedUpdate(); //Explanation of correspoding line in below branch
                yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            }
            else
            {

                textBoxController.SetTextInstant("Which move should " + pokemon.GetDisplayName() + " forget?");
                yield return StartCoroutine(AwaitUserSelectMove(pokemon, false, false, false));

                if (textBoxController.userChoiceIndexSelected == 0) //First choice (indexed 0) is always cancel option
                {
                    textBoxController.Hide();
                    controlAllowed = true;
                    yield break;
                }

                PlayerData.singleton.inventory.RemoveItem(CurrentItem, 1);

                int moveIndexSelected = textBoxController.userChoiceIndexSelected - 1;

                int changedMoveIndex = -1;
                int unsetMovesFound = 0;

                //Offsetting selection based on unset moves since they wouldn't have been displayed in choices
                //This shouldn't ever be a problem since, if a pokemon has a free move space, they don't need to forget a move, but is just in case
                for (int i = moveIndexSelected; i < pokemon.moveIds.Length; i++)
                {

                    if (PokemonMove.MoveIdIsUnset(pokemon.moveIds[i]))
                        unsetMovesFound++;

                    if (i - unsetMovesFound == moveIndexSelected)
                    {
                        changedMoveIndex = i;
                        break;
                    }

                }

                if (changedMoveIndex < 0)
                {
                    Debug.LogError("Unable to find changedMoveIndex");
                    textBoxController.Hide();
                    yield break;
                }

                PokemonMove forgottenMove = PokemonMove.GetPokemonMoveById(pokemon.moveIds[changedMoveIndex]); //N.B. '-1' since first option is always cancel option
                PokemonMove learntMove = ((TMItem)CurrentItem).Move;

                pokemon.moveIds[changedMoveIndex] = learntMove.id;
                pokemon.movePPs[changedMoveIndex] = learntMove.maxPP;

                pokemon.AddFriendshipGainForTMUsage();

                textBoxController.RevealText(pokemon.GetDisplayName() + " forgot " + forgottenMove.name + "...");
                yield return new WaitForFixedUpdate(); //Need to wait an extra frame since current frame is the frame that user used a submit button
                yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                textBoxController.RevealText(pokemon.GetDisplayName() + " learnt " + learntMove.name + "!");
                yield return new WaitForFixedUpdate(); //As before
                yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            }

            textBoxController.Hide();

            controlAllowed = true;

        }

        #endregion

    }
}
