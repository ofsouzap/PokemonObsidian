using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Items;
using Items.MedicineItems;
using Pokemon.Moves;

namespace Battle.PlayerUI
{
    public class PlayerBattleUIController : MonoBehaviour
    {

        public static PlayerBattleUIController singleton;

        public BattleParticipantPlayer playerBattleParticipant;

        public MenuRootController menuRootController;

        public MenuFightController menuFightController;

        public MenuBagController menuBagController;

        public MenuBagCategoryController menuBagCategoryController;

        public MenuPartyController menuPartyController;

        public MenuPartyPokemonController menuPartyPokemonController;

        public MenuPartyPokemonMovesController menuPartyPokemonMovesController;

        public GameObject selectableSelectionPrefab;

        private PlayerData player;
        private BattleManager battleManager;

        public bool GetPlayerAllowedToFlee() => playerBattleParticipant.GetAllowedToFlee();

        public byte currentSelectedPartyPokemonIndex;

        public void SetUp(BattleManager battleManager)
        {

            singleton = this;
            player = PlayerData.singleton;
            this.battleManager = battleManager;
            currentSelectedPartyPokemonIndex = (byte)battleManager.battleData.participantPlayer.activePokemonIndex;

            #region Run Set-Up Functions

            menuFightController.SetUp();
            menuPartyController.SetUp();
            menuBagCategoryController.SetUp();

            #endregion

            #region Back Buttons

            menuFightController.buttonBack.onClick.AddListener(() => OpenRootMenu());

            menuBagController.buttonBack.onClick.AddListener(() => OpenRootMenu());

            menuBagCategoryController.buttonBack.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuBagController.Show();
                EventSystem.current.SetSelectedGameObject(menuBagController.buttonBack.gameObject);
            });

            menuPartyController.buttonBack.onClick.AddListener(() => OpenRootMenu());

            menuPartyPokemonController.buttonBack.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuPartyController.Show();
                EventSystem.current.SetSelectedGameObject(menuPartyController.pokemonButtons[0].gameObject);
            });

            menuPartyPokemonMovesController.buttonBack.onClick.AddListener(() =>
            {
                menuPartyPokemonMovesController.CloseMenu();
                DisableAllMenus();
                menuPartyPokemonController.Show();
                EventSystem.current.SetSelectedGameObject(menuPartyPokemonController.buttonBack.gameObject);
            });

            #endregion

            #region Root Menu Buttons

            if (playerBattleParticipant != null)
            {
                menuRootController.buttonRun.onClick.AddListener(() =>
                {

                    if (GetPlayerAllowedToFlee())
                    {
                        playerBattleParticipant.ChooseActionFlee();
                    }
                    else
                    {
                        battleManager.DisplayPlayerInvalidSelectionMessage("You can't flee!");
                    }

                });
            }

            menuRootController.buttonBag.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuBagController.Show();
                EventSystem.current.SetSelectedGameObject(menuBagController.buttonBack.gameObject);
            });

            menuRootController.buttonParty.onClick.AddListener(() =>
            {
                menuPartyController.RefreshButtons();
                DisableAllMenus();
                menuPartyController.Show();
                EventSystem.current.SetSelectedGameObject(menuPartyController.pokemonButtons[0].gameObject);
            });

            menuRootController.buttonFight.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuFightController.Show();
                EventSystem.current.SetSelectedGameObject(menuFightController.buttonBack.gameObject);
            });

            #endregion

            #region Fight Menu Buttons

            if (playerBattleParticipant != null)
            {

                menuFightController.SetCurrentPokemonIndex(playerBattleParticipant.activePokemonIndex);

                menuFightController.moveButtons[0].onClick.AddListener(() => SuggestChooseMoveIndex(0));

                menuFightController.moveButtons[1].onClick.AddListener(() => SuggestChooseMoveIndex(1));

                menuFightController.moveButtons[2].onClick.AddListener(() => SuggestChooseMoveIndex(2));

                menuFightController.moveButtons[3].onClick.AddListener(() => SuggestChooseMoveIndex(3));

                menuFightController.buttonMoveStruggle.onClick.AddListener(() =>
                {
                    playerBattleParticipant.ChooseActionFightStruggle();
                });

            }

            menuFightController.RefreshMoveButtons();

            #endregion

            #region Bag Menu Buttons

            menuBagController.buttonHPPPRestore.onClick.AddListener(() => OpenBagCategoryMenu(MenuBagCategoryController.BagCategory.HPPPRestore));
            menuBagController.buttonStatusItems.onClick.AddListener(() => OpenBagCategoryMenu(MenuBagCategoryController.BagCategory.StatusRestore));
            menuBagController.buttonPokeBalls.onClick.AddListener(() => OpenBagCategoryMenu(MenuBagCategoryController.BagCategory.PokeBalls));
            menuBagController.buttonBattleItems.onClick.AddListener(() => OpenBagCategoryMenu(MenuBagCategoryController.BagCategory.BattleItems));

            #endregion

            #region Bag Category Menu Buttons

            menuBagCategoryController.buttonNext.onClick.AddListener(() => menuBagCategoryController.NextPage());
            menuBagCategoryController.buttonPrevious.onClick.AddListener(() => menuBagCategoryController.PreviousPage());

            menuBagCategoryController.itemButtons[0].Button.onClick.AddListener(() => SuggestUseItemByPageIndex(0));
            menuBagCategoryController.itemButtons[1].Button.onClick.AddListener(() => SuggestUseItemByPageIndex(1));
            menuBagCategoryController.itemButtons[2].Button.onClick.AddListener(() => SuggestUseItemByPageIndex(2));
            menuBagCategoryController.itemButtons[3].Button.onClick.AddListener(() => SuggestUseItemByPageIndex(3));
            menuBagCategoryController.itemButtons[4].Button.onClick.AddListener(() => SuggestUseItemByPageIndex(4));
            menuBagCategoryController.itemButtons[5].Button.onClick.AddListener(() => SuggestUseItemByPageIndex(5));
            menuBagCategoryController.itemButtons[6].Button.onClick.AddListener(() => SuggestUseItemByPageIndex(6));
            menuBagCategoryController.itemButtons[7].Button.onClick.AddListener(() => SuggestUseItemByPageIndex(7));

            #endregion

            #region Party Menu Buttons

            menuPartyController.pokemonButtons[0].Button.onClick.AddListener(() => OpenPartyPokemonMenu(0));
            menuPartyController.pokemonButtons[1].Button.onClick.AddListener(() => OpenPartyPokemonMenu(1));
            menuPartyController.pokemonButtons[2].Button.onClick.AddListener(() => OpenPartyPokemonMenu(2));
            menuPartyController.pokemonButtons[3].Button.onClick.AddListener(() => OpenPartyPokemonMenu(3));
            menuPartyController.pokemonButtons[4].Button.onClick.AddListener(() => OpenPartyPokemonMenu(4));
            menuPartyController.pokemonButtons[5].Button.onClick.AddListener(() => OpenPartyPokemonMenu(5));

            menuPartyController.RefreshButtons();

            #endregion

            #region Party Pokemon Buttons

            menuPartyPokemonController.buttonCheckMoves.onClick.AddListener(() =>
            {
                OpenPartyPokemonMovesMenu();
            });

            if (playerBattleParticipant != null)
            {
                menuPartyPokemonController.buttonSendOut.onClick.AddListener(() =>
                {

                    bool isNull, healthPositive;

                    if (currentSelectedPartyPokemonIndex == playerBattleParticipant.activePokemonIndex)
                    {
                        battleManager.DisplayPlayerInvalidSelectionMessage("That is already your active pokemon");
                        return;
                    }

                    if (!playerBattleParticipant.GetAllowedToSwitchPokemon())
                    {
                        battleManager.DisplayPlayerInvalidSelectionMessage("You can't switch pokemon now!");
                        return;
                    }

                    Pokemon.PokemonInstance pokemon = PlayerData.singleton.partyPokemon[currentSelectedPartyPokemonIndex];
                    isNull = pokemon == null;

                    if (!isNull)
                        healthPositive = pokemon.health > 0;
                    else
                        healthPositive = false; //Health Postive is unecessary if pokemon is null

                    if (!isNull && healthPositive)
                    {
                        playerBattleParticipant.ChooseActionSwitchPokemon(currentSelectedPartyPokemonIndex);
                    }
                    else if (isNull)
                    {
                        Debug.LogError("Current selected pokemon is null when trying to choose switch pokemon action");
                    }
                    else
                    {
                        battleManager.DisplayPlayerInvalidSelectionMessage("You can't switch in a fainted pokemon");
                    }

                });
            }

            menuPartyPokemonController.buttonNext.onClick.AddListener(() =>
            {
                currentSelectedPartyPokemonIndex = (byte)((currentSelectedPartyPokemonIndex + 1) % player.GetNumberOfPartyPokemon());
                OpenPartyPokemonMenu(currentSelectedPartyPokemonIndex);
            });

            menuPartyPokemonController.buttonPrevious.onClick.AddListener(() =>
            {
                currentSelectedPartyPokemonIndex = (byte)((currentSelectedPartyPokemonIndex - 1 + player.GetNumberOfPartyPokemon()) % player.GetNumberOfPartyPokemon());
                OpenPartyPokemonMenu(currentSelectedPartyPokemonIndex);
            });

            #endregion

            #region Party Pokemon Move Button Names

            menuPartyPokemonMovesController.SetUp();
            menuPartyPokemonMovesController.RefreshMoveButtons();

            #endregion

        }

        public void RefreshMenus()
        {

            menuFightController.SetCurrentPokemonIndex(playerBattleParticipant.activePokemonIndex);
            menuFightController.RefreshMoveButtons();
            menuPartyController.RefreshButtons();
            menuPartyPokemonMovesController.RefreshMoveButtons();

        }

        public void SuggestChooseMoveIndex(int index)
        {

            int moveId = playerBattleParticipant.ActivePokemon.moveIds[index];
            PokemonMove move = PokemonMove.GetPokemonMoveById(moveId);

            if (playerBattleParticipant.ActivePokemon.movePPs[index] <= 0)
            {

                battleManager.DisplayPlayerInvalidSelectionMessage("That move has no PP");
                
            }
            else if (playerBattleParticipant.ActivePokemon.battleProperties.volatileStatusConditions.encoreTurns > 0
                && moveId != playerBattleParticipant.ActivePokemon.battleProperties.volatileStatusConditions.encoreMoveId)
            {

                //If under influence of encore and trying to use a non-encored move

                battleManager.DisplayPlayerInvalidSelectionMessage(playerBattleParticipant.ActivePokemon.GetDisplayName() + " must provide an encore!");

            }
            else if (playerBattleParticipant.ActivePokemon.battleProperties.volatileStatusConditions.tauntTurns > 0
                && move.moveType == PokemonMove.MoveType.Status)
            {

                //If under influence of taunt and trying to use a status move

                battleManager.DisplayPlayerInvalidSelectionMessage(playerBattleParticipant.ActivePokemon.GetDisplayName() + " can't use status moves while being taunted!");

            }
            else if (playerBattleParticipant.ActivePokemon.battleProperties.volatileStatusConditions.torment
                && playerBattleParticipant.ActivePokemon.battleProperties.lastMoveId == moveId)
            {

                //If under influence of torment and trying to use same move as last turn

                battleManager.DisplayPlayerInvalidSelectionMessage(playerBattleParticipant.ActivePokemon.GetDisplayName() + " can't use the same move as last turn while being tormented!");

            }
            else
            {

                menuFightController.CloseMenu();
                playerBattleParticipant.ChooseActionFight(index);

            }

        }

        public void DisableAllMenus()
        {

            menuRootController.Hide();
            menuFightController.Hide();
            menuBagController.Hide();
            menuBagCategoryController.Hide();
            menuPartyController.Hide();
            menuPartyPokemonController.Hide();
            menuPartyPokemonMovesController.Hide();

        }

        public void OpenRootMenu()
        {
            DisableAllMenus();
            menuRootController.Show();
            EventSystem.current.SetSelectedGameObject(menuRootController.buttonFight.gameObject);
        }

        private void OpenPartyPokemonMenu(byte partyIndex)
        {

            if (partyIndex > 5)
            {
                Debug.LogError("Party index out of greater than 5");
                return;
            }

            currentSelectedPartyPokemonIndex = partyIndex;

            DisableAllMenus();
            menuPartyPokemonController.Show();
            EventSystem.current.SetSelectedGameObject(menuPartyPokemonController.buttonBack.gameObject);

            menuPartyPokemonController.SetPokemonDetails(player.partyPokemon[partyIndex]);

        }

        private void OpenPartyPokemonMovesMenu()
        {

            DisableAllMenus();
            menuPartyPokemonMovesController.Show();
            menuPartyPokemonMovesController.RefreshMoveButtons();
            EventSystem.current.SetSelectedGameObject(menuPartyPokemonMovesController.buttonBack.gameObject);

        }

        private void OpenBagCategoryMenu(MenuBagCategoryController.BagCategory category)
        {

            PlayerData.Inventory playerInventory = PlayerData.singleton.inventory;

            Item[] itemsToDisplay;

            switch (category)
            {

                case MenuBagCategoryController.BagCategory.HPPPRestore:
                    itemsToDisplay = playerInventory.medicineItems.GetItems().Where(x => x is HealthMedicineItem || x is PPRestoreMedicineItem).ToArray();
                    break;

                case MenuBagCategoryController.BagCategory.StatusRestore:
                    itemsToDisplay = playerInventory.medicineItems.GetItems().Where(x => x is NVSCCureMedicineItem).ToArray();
                    break;

                case MenuBagCategoryController.BagCategory.PokeBalls:
                    itemsToDisplay = playerInventory.pokeBalls.GetItems();
                    break;

                case MenuBagCategoryController.BagCategory.BattleItems:
                    itemsToDisplay = playerInventory.battleItems.GetItems();
                    break;

                default:
                    Debug.LogError("Unknown bag category provided (" + category + ")");
                    return;

            }

            menuBagCategoryController.SetItems(itemsToDisplay);

            ShowBagCategoryMenu();

        }

        public void ShowBagCategoryMenu()
        {

            DisableAllMenus();
            menuBagCategoryController.Show();
            EventSystem.current.SetSelectedGameObject(menuBagCategoryController.buttonBack.gameObject);

        }

        public void SuggestUseItemByPageIndex(int index)
            => SuggestUseItem(menuBagCategoryController.GetPageItem(index));

        public void SuggestUseItem(Item item)
        {

            if (playerBattleParticipant.CheckIfItemUsageAllowed(item, out string invalidSelectionMessage))
            {
                playerBattleParticipant.ChooseActionUseItem(item);
            }
            else
            {
                battleManager.DisplayPlayerInvalidSelectionMessage(invalidSelectionMessage);
            }

        }

    }
}
