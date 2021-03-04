using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battle.PlayerUI;

namespace Battle.PlayerUI
{
    public class PlayerBattleUIController : MonoBehaviour
    {

        public MenuRootController menuRootController;

        public MenuFightController menuFightController;

        public MenuBagController menuBagController;

        public MenuBagCategoryController menuBagCategoryController;

        public MenuPartyController menuPartyController;

        public MenuPartyPokemonController menuPartyPokemonController;

        public MenuPartyPokemonMovesController menuPartyPokemonMovesController;

        private PlayerData player;

        private byte currentSelectedPartyPokemonIndex;

        private void Start()
        {
            player = PlayerData.singleton;
            SetUp();
            OpenRootMenu();
        }

        private void SetUp()
        {

            //TODO - for each opened menu, set currently-selected button in event system

            #region Run Set-Up Functions

            menuFightController.SetUp();
            menuPartyController.SetUp();
            menuBagCategoryController.SetUp();

            #endregion

            #region Back Buttons

            menuFightController.buttonBack.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuRootController.gameObject.SetActive(true);
            });

            menuBagController.buttonBack.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuRootController.gameObject.SetActive(true);
            });

            menuBagCategoryController.buttonBack.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuBagController.gameObject.SetActive(true);
            });

            menuPartyController.buttonBack.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuRootController.gameObject.SetActive(true);
            });

            menuPartyPokemonController.buttonBack.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuPartyController.gameObject.SetActive(true);
            });

            menuPartyPokemonMovesController.buttonBack.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuPartyPokemonController.gameObject.SetActive(true);
            });

            #endregion

            #region Root Menu Buttons

            menuRootController.buttonRun.onClick.AddListener(() =>
            {
                //TODO - do once player participant controller ready
            });

            menuRootController.buttonBag.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuBagController.gameObject.SetActive(true);
            });

            menuRootController.buttonParty.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuPartyController.gameObject.SetActive(true);
            });

            menuRootController.buttonFight.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuFightController.gameObject.SetActive(true);
            });

            #endregion

            #region Fight Menu Buttons

            //TODO - do once controller ready (as mentioned below)
            //menuFightController.SetCurrentPokemonIndex();

            //TODO - do once player participant controller ready

            #endregion

            #region Bag Menu Buttons

            //TODO - do once items ready

            #endregion

            #region Bag Category Menu Buttons

            //TODO - do once items ready

            #endregion

            #region Party Menu Buttons

            menuPartyController.pokemonButtons[0].onClick.AddListener(() => OpenPartyPokemonMenu(0));
            menuPartyController.pokemonButtons[1].onClick.AddListener(() => OpenPartyPokemonMenu(1));
            menuPartyController.pokemonButtons[2].onClick.AddListener(() => OpenPartyPokemonMenu(2));
            menuPartyController.pokemonButtons[3].onClick.AddListener(() => OpenPartyPokemonMenu(3));
            menuPartyController.pokemonButtons[4].onClick.AddListener(() => OpenPartyPokemonMenu(4));
            menuPartyController.pokemonButtons[5].onClick.AddListener(() => OpenPartyPokemonMenu(5));

            menuPartyController.RefreshButtons();

            #endregion

            #region Party Pokemon Buttons

            menuPartyPokemonController.buttonCheckMoves.onClick.AddListener(() =>
            {
                OpenPartyPokemonMovesMenu();
            });

            menuPartyPokemonController.buttonSendOut.onClick.AddListener(() =>
            {
                //TODO - do once player participant controller ready
            });

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

            //TODO buttons for party pokemon menu. Only show if player has that many pokemon

        }

        private void DisableAllMenus()
        {

            menuRootController.gameObject.SetActive(false);
            menuFightController.gameObject.SetActive(false);
            menuBagController.gameObject.SetActive(false);
            menuBagCategoryController.gameObject.SetActive(false);
            menuPartyController.gameObject.SetActive(false);
            menuPartyPokemonController.gameObject.SetActive(false);
            menuPartyPokemonMovesController.gameObject.SetActive(false);

        }

        public void OpenRootMenu()
        {
            DisableAllMenus();
            menuRootController.gameObject.SetActive(true);
        }

        private void OpenPartyPokemonMenu(byte partyIndex)
        {

            if (partyIndex > 5)
            {
                Debug.LogError("Party index out of greater than 5");
                return;
            }
            if (partyIndex >= player.GetNumberOfPartyPokemon())
            {
                Debug.LogError("Party index too great for player pokemon count");
                return;
            }

            currentSelectedPartyPokemonIndex = partyIndex;

            DisableAllMenus();
            menuPartyPokemonController.gameObject.SetActive(true);

            menuPartyPokemonController.SetPokemonDetails(player.partyPokemon[partyIndex]);

        }

        private void OpenPartyPokemonMovesMenu()
        {

            DisableAllMenus();
            menuPartyPokemonMovesController.gameObject.SetActive(true);

            menuPartyPokemonMovesController.SetCurrentPokemonIndex(currentSelectedPartyPokemonIndex);

        }

    }
}
