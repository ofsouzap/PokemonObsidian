using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Battle;
using Pokemon;
using Battle.PlayerUI;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

namespace Battle
{
    public class BattleParticipantPlayer : BattleParticipant
    {

        public PlayerBattleUIController playerBattleUIController;
        public PlayerPokemonSelectUIController playerPokemonSelectUIController;
        public PlayerMoveSelectUIController playerMoveSelectUIController;

        public override string GetName() => PlayerData.singleton.profile.name;

        private BattleData recentBattleData;

        public override PokemonInstance[] GetPokemon()
        {
            return PlayerData.singleton.partyPokemon;
        }

        public override void StartChoosingAction(BattleData battleData)
        {
            recentBattleData = battleData;
            actionHasBeenChosen = false;
            playerBattleUIController.RefreshMenus();
            OpenBattleUIRoot();
        }

        public override void StartChoosingNextPokemon()
        {
            nextPokemonHasBeenChosen = false;
            StartPokemonSelectUI(PokemonSelectUIPurpose.ReplacingPokemon,
                (x) => !x.IsFainted,
                false);
        }

        public override bool CheckIfDefeated()
        {
            return GetPokemon().Where(x => x != null).All((x) => x.IsFainted);
        }

        public void SetUp()
        {

            HideBattleUI();
            HidePokemonSelectUI();

            playerBattleUIController.playerBattleParticipant = this;
            playerPokemonSelectUIController.playerBattleParticipant = this;
            playerMoveSelectUIController.playerBattleParticipant = this;

            playerBattleUIController.SetUp(battleManager);
            playerPokemonSelectUIController.SetUp();

            playerPokemonSelectUIController.buttonBack.onClick.AddListener(OnPokemonSelectUIButtonBackClick);
            playerMoveSelectUIController.buttonBack.onClick.AddListener(OnMoveSelectUIButtonBackClick);

        }

        public void SetPlayerCanFlee(bool state)
        {
            if (playerBattleUIController != null)
            {
                playerBattleUIController.SetPlayerCanFlee(state);
            }
            else
            {
                Debug.LogWarning("Setting player can flee before player battle UI controller set");
            }
        }

        #region Player Battle UI

        private void OpenBattleUIRoot()
        {
            playerBattleUIController.OpenRootMenu();
        }

        private void HideBattleUI()
        {
            playerBattleUIController.DisableAllMenus();
        }

        /// <summary>
        /// Choose to attempt to flee the battle
        /// </summary>
        public void ChooseActionFlee()
        {

            chosenAction = new Action(this)
            {
                type = Action.Type.Flee
            };

            actionHasBeenChosen = true;

            HideBattleUI();

        }

        #region Item Usage

        /// <summary>
        /// Checks if the player is able and allowed to use a specified item at the moment
        /// </summary>
        /// <param name="item">The item to consider</param>
        /// <param name="message">The message to display if the user tries to use this item</param>
        /// <returns>Whether the player can use the item</returns>
        public bool CheckIfItemUsageAllowed(Item item,
            out string message)
        {

            BattleData.ItemUsagePermissions itemPermissions = recentBattleData.itemUsagePermissions;

            if (!itemPermissions.pokeBalls && item is PokeBall)
            {
                message = "You aren't allowed to use poke balls in this battle";
                return false;
            }
            else if (!itemPermissions.hpRestorationItems && item is HealthMedicineItem)
            {
                message = "You aren't allowed to use health-restoration items in this battle";
                return false;
            }
            else if (!itemPermissions.ppRestorationItems && item is PPRestoreMedicineItem)
            {
                message = "You aren't allowed to use PP-restoration items in this battle";
                return false;
            }
            else if (!itemPermissions.revivalItems && item is RevivalMedicineItem)
            {
                message = "You aren't allowed to use revival items in this battle";
                return false;
            }
            else if (!itemPermissions.statusItems && item is NVSCCureMedicineItem)
            {
                message = "You aren't allowed to use status items in this battle";
                return false;
            }
            else if (!itemPermissions.battleItems && item is BattleItem)
            {
                message = "You aren't allowed to use battle items in this battle";
                return false;
            }
            else if (item is BattleItem && !item.CheckCompatibility(ActivePokemon))
            {
                message = ActivePokemon.GetDisplayName() + " can't use this item";
                return false;
            }
            else if (item is PokeBall && PlayerData.singleton.PartyIsFull && PlayerData.singleton.boxPokemon.IsFull)
            {
                message = "You have no space for another pokemon!";
                return false;
            }

            message = "";
            return true;

        }

        /// <summary>
        /// Choose to use an item
        /// </summary>
        /// <param name="item">The item to use</param>
        public void ChooseActionUseItem(Item item)
        {

            if (!CheckIfItemUsageAllowed(item, out _))
            {
                Debug.LogError("Item chosen for usage that isn't allowed to be used");
                return;
            }

            HideBattleUI();

            //Choosing how to prceed based on whether user needs to select a pokemon for the item or not
            if (item is MedicineItem)
                ChooseActionUseItem_TargetRequired(item);
            else
                ChooseActionUseItem_TargetNotRequired(item);

        }

        private Item itemToUseForSelectedPokemonFromPokemonSelectUI = null;

        private void ChooseActionUseItem_TargetRequired(Item item)
        {

            itemToUseForSelectedPokemonFromPokemonSelectUI = item;

            StartPokemonSelectUI(
                PokemonSelectUIPurpose.ItemTarget,
                item.CheckCompatibility,
                true,
                () =>
                {
                    HidePokemonSelectUI();
                    playerBattleUIController.ShowBagCategoryMenu();
                }
            );

        }

        private int partyIndexForSelectedMoveIndexFromMoveSelectUI = -1;

        private void SelectItemUsageTargetPokemon(int partyIndex)
        {

            if (itemToUseForSelectedPokemonFromPokemonSelectUI is PPRestoreMedicineItem && ((PPRestoreMedicineItem)itemToUseForSelectedPokemonFromPokemonSelectUI).isForSingleMove)
            {

                partyIndexForSelectedMoveIndexFromMoveSelectUI = partyIndex;

                StartMoveSelectUI(MoveSelectUIPurpose.PPRecoveryTargetSelection,
                    GetPokemon()[partyIndexForSelectedMoveIndexFromMoveSelectUI],
                    (moveIndex) => moveSelectUISelectedPokemon.movePPs[moveIndex]
                        < Pokemon.Moves.PokemonMove.GetPokemonMoveById(
                            moveSelectUISelectedPokemon.moveIds[moveIndex]
                            ).maxPP,
                    () =>
                    {
                        HideMoveSelectUI();
                        playerBattleUIController.ShowBagCategoryMenu();
                    });

            }
            else
            {

                chosenAction = new Action(this)
                {
                    type = Action.Type.UseItem,
                    useItemItemToUse = itemToUseForSelectedPokemonFromPokemonSelectUI,
                    useItemTargetPartyIndex = partyIndex
                };
                actionHasBeenChosen = true;

            }

        }

        public void SelectMoveItemUsageTargetMoveIndex(int moveIndex)
        {

            chosenAction = new Action(this)
            {
                type = Action.Type.UseItem,
                useItemItemToUse = itemToUseForSelectedPokemonFromPokemonSelectUI,
                useItemTargetPartyIndex = partyIndexForSelectedMoveIndexFromMoveSelectUI,
                useItemTargetMoveIndex = moveIndex
            };
            actionHasBeenChosen = true;

        }

        private void ChooseActionUseItem_TargetNotRequired(Item item)
        {

            chosenAction = new Action(this)
            {
                type = Action.Type.UseItem,
                useItemItemToUse = item,
                useItemPokeBallTarget = (item is PokeBall) ? recentBattleData.participantOpponent : null
            };
            actionHasBeenChosen = true;

        }

        #endregion

        /// <summary>
        /// Choose to fight using the move with index moveIndex in the active pokemon's moves
        /// </summary>
        /// <param name="partyIndex">The indexx of the move in the active pokemon's moves</param>
        public void ChooseActionFight(int moveIndex)
        {

            if (moveIndex < 0 || moveIndex > 3)
            {
                Debug.LogError("Invalid move index sent for choosing to fight - " + moveIndex);
                return;
            }

            chosenAction = new Action(this)
            {
                type = Action.Type.Fight,
                fightMoveIndex = moveIndex,
                fightMoveTarget = recentBattleData.participantOpponent
            };

            actionHasBeenChosen = true;

            HideBattleUI();

        }

        /// <summary>
        /// Choose that the pokemon should use the struggle move
        /// </summary>
        public void ChooseActionFightStruggle()
        {

            chosenAction = new Action(this)
            {
                type = Action.Type.Fight,
                fightUsingStruggle = true,
                fightMoveTarget = recentBattleData.participantOpponent
            };

            actionHasBeenChosen = true;

            HideBattleUI();

        }

        /// <summary>
        /// Choose to switch pokemon to the pokemon with index partyIndex in the player's party
        /// </summary>
        /// <param name="partyIndex">The index of the pokemon to switch to in the player's party</param>
        public void ChooseActionSwitchPokemon(int partyIndex)
        {

            if (partyIndex < 0 || partyIndex > 5)
            {
                Debug.LogError("Invalid party index sent for choosing to switch pokemon - " + partyIndex);
                return;
            }

            chosenAction = new Action(this)
            {
                type = Action.Type.SwitchPokemon,
                switchPokemonIndex = partyIndex
            };

            actionHasBeenChosen = true;

            HideBattleUI();

        }

        #endregion

        #region Player Pokemon Selection UI

        private enum PokemonSelectUIPurpose
        {
            ReplacingPokemon,
            ItemTarget
        }

        private PokemonSelectUIPurpose pokemonSelectUIPurpose;
        private Predicate<PokemonInstance> pokemonSelectUIValidityPredicate;
        private System.Action pokemonSelectUIBackButtonAction;

        /// <summary>
        /// Start the pokemon selection UI and specify settings for starting it
        /// </summary>
        /// <param name="purpose">The purpose for which the UI is being opened</param>
        /// <param name="validityCheck">A predicate to check if a chosen pokemon is a valid target for selection (eg. a pokemon can't be selected as the next pokemon if it has no health)</param>
        /// <param name="showBackButton">Whether to show the back button (eg. when selecting a next pokemon, the player shouldn't be allowed to go back)</param>
        private void StartPokemonSelectUI(PokemonSelectUIPurpose purpose,
            Predicate<PokemonInstance> validityCheck,
            bool showBackButton = true,
            System.Action backButtonAction = null)
        {

            pokemonSelectUIPurpose = purpose;
            pokemonSelectUIValidityPredicate = validityCheck;
            pokemonSelectUIBackButtonAction = backButtonAction;

            playerPokemonSelectUIController.RefreshButtons();
            ShowPokemonSelectUI(showBackButton);

        }

        private void ShowPokemonSelectUI(bool showBackButton = true)
        {

            playerPokemonSelectUIController.Show();

            if (showBackButton)
                playerPokemonSelectUIController.ShowBackButton();
            else
                playerPokemonSelectUIController.HideBackButton();

            EventSystem.current.SetSelectedGameObject(playerPokemonSelectUIController.pokemonButtons[0].gameObject);

        }

        private void HidePokemonSelectUI()
        {
            playerPokemonSelectUIController.Hide();
        }

        private bool PokemonSelectUICheckValidity(PokemonInstance pokemon)
        {

            if (pokemonSelectUIValidityPredicate != null)
                return pokemonSelectUIValidityPredicate(pokemon);
            else
            {
                Debug.LogError("Pokemon select UI validity predicate unset");
                return true;
            }

        }

        public void PokemonSelectUISelectPokemon(int partyIndex)
        {

            if (partyIndex < 0 || partyIndex > 5)
            {
                Debug.LogError("Party index out of range - " + partyIndex);
                return;
            }

            PokemonInstance selectedPokemon = PlayerData.singleton.partyPokemon[partyIndex];

            if (!PokemonSelectUICheckValidity(selectedPokemon))
            {
                PokemonSelectDisplayInvalidMessage();
                return;
            }

            if (pokemonSelectUIPurpose == PokemonSelectUIPurpose.ReplacingPokemon)
            {

                nextPokemonHasBeenChosen = true;
                chosenNextPokemonIndex = partyIndex;

                HidePokemonSelectUI();

            }
            else if (pokemonSelectUIPurpose == PokemonSelectUIPurpose.ItemTarget)
            {

                HidePokemonSelectUI();
                SelectItemUsageTargetPokemon(partyIndex);

            }
            else
            {
                Debug.LogError("Unknown pokemon select UI purpose - " + pokemonSelectUIPurpose);
            }

        }

        private void PokemonSelectDisplayInvalidMessage()
        {
            battleManager.DisplayPlayerInvalidSelectionMessage("You can't select this pokemon");
        }

        #region Back Button

        private void OnPokemonSelectUIButtonBackClick()
        {

            if (pokemonSelectUIPurpose == PokemonSelectUIPurpose.ReplacingPokemon)
            {
                Debug.LogError("Player shouldn't be able to use pokemon select UI back button when replacing pokemon");
                return;
            }

            if (pokemonSelectUIBackButtonAction != null)
                pokemonSelectUIBackButtonAction();
            else
                Debug.LogError("pokemonSelectUIBackButtonAction is unset");

        }

        #endregion

        #endregion

        #region Player Move Selection UI

        private enum MoveSelectUIPurpose
        {
            PPRecoveryTargetSelection
        }

        private MoveSelectUIPurpose moveSelectUIPurpose;
        private PokemonInstance moveSelectUISelectedPokemon;
        private Predicate<int> moveSelectUIValidityPredicate; //Integer parameter is index of move in pokemon's attributes
        private System.Action moveSelectUIBackButtonAction;

        private void StartMoveSelectUI(MoveSelectUIPurpose purpose,
            PokemonInstance selectedPokemon,
            Predicate<int> validityCheck,
            System.Action backButtonAction)
        {

            moveSelectUIPurpose = purpose;
            moveSelectUISelectedPokemon = selectedPokemon;
            moveSelectUIValidityPredicate = validityCheck;
            moveSelectUIBackButtonAction = backButtonAction;

            playerMoveSelectUIController.RefreshButtons();
            ShowMoveSelectUI();

        }

        private void ShowMoveSelectUI()
        {

            playerMoveSelectUIController.Show();
            playerMoveSelectUIController.ShowBackButton();

            playerMoveSelectUIController.HideMovePane();
            EventSystem.current.SetSelectedGameObject(playerMoveSelectUIController.buttonBack.gameObject);

        }

        private void HideMoveSelectUI()
        {
            playerMoveSelectUIController.Hide();
        }

        private bool MoveSelectUICheckValidity(int moveIndex)
        {

            if (moveSelectUIValidityPredicate != null)
                return moveSelectUIValidityPredicate(moveIndex);
            else
            {
                Debug.LogError("Move select UI validity predicate unset");
                return true;
            }

        }

        public void MoveSelectUISelectMove(int moveIndex)
        {

            if (moveIndex < 0 || moveIndex > 3)
            {
                Debug.LogError("Move index out of range - " + moveIndex);
                return;
            }

            if (!MoveSelectUICheckValidity(moveIndex))
            {
                MoveSelectDisplayInvalidMessage();
                return;
            }

            if (moveSelectUIPurpose == MoveSelectUIPurpose.PPRecoveryTargetSelection)
            {

                HideMoveSelectUI();
                SelectMoveItemUsageTargetMoveIndex(moveIndex);

            }
            else
            {
                Debug.LogError("Unknown move select UI purpose - " + moveSelectUIPurpose);
            }

        }

        private void MoveSelectDisplayInvalidMessage()
        {
            battleManager.DisplayPlayerInvalidSelectionMessage("You can't select this move");
        }

        #region Back Button

        private void OnMoveSelectUIButtonBackClick()
        {

            if (moveSelectUIBackButtonAction != null)
                moveSelectUIBackButtonAction();
            else
                Debug.LogError("moveSelectUIBackButtonAction is unset");

        }

        #endregion

        #endregion

    }
}