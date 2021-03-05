using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Battle.PlayerUI;

namespace Battle.PlayerUI
{
    public class PlayerBattleUIController : MonoBehaviour
    {

        public static PlayerBattleUIController singleton;

        public MenuRootController menuRootController;

        public MenuFightController menuFightController;

        public MenuBagController menuBagController;

        public MenuBagCategoryController menuBagCategoryController;

        public MenuPartyController menuPartyController;

        public MenuPartyPokemonController menuPartyPokemonController;

        public MenuPartyPokemonMovesController menuPartyPokemonMovesController;

        public GameObject selectableSelectionPrefab;

        private PlayerData player;

        private byte currentSelectedPartyPokemonIndex;

        private void Start()
        {
            singleton = this;
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

            menuFightController.buttonBack.onClick.AddListener(() => OpenRootMenu());

            menuBagController.buttonBack.onClick.AddListener(() => OpenRootMenu());

            menuBagCategoryController.buttonBack.onClick.AddListener(() =>
            {
                DisableAllMenus();
                menuBagController.Show();
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
                DisableAllMenus();
                menuPartyPokemonController.Show();
                EventSystem.current.SetSelectedGameObject(menuPartyPokemonController.buttonBack.gameObject);
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
                menuBagController.Show();
                EventSystem.current.SetSelectedGameObject(menuBagController.buttonBack.gameObject);
            });

            menuRootController.buttonParty.onClick.AddListener(() =>
            {
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

            //TODO - do once controller ready (as mentioned below)
            //menuFightController.SetCurrentPokemonIndex();

            //TODO - do once player participant controller ready

            menuFightController.RefreshMoveButtons();

            #endregion

            #region Bag Menu Buttons

            //TODO - do once items ready

            #endregion

            #region Bag Category Menu Buttons

            //TODO - do once items ready

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

            #region Party Pokemon Move Button Names

            menuPartyPokemonMovesController.SetUp();
            menuPartyPokemonMovesController.RefreshMoveButtons();

            #endregion

            //TODO buttons for party pokemon menu. Only show if player has that many pokemon

        }

        private void DisableAllMenus()
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
            if (partyIndex >= player.GetNumberOfPartyPokemon())
            {
                Debug.LogError("Party index too great for player pokemon count");
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
            EventSystem.current.SetSelectedGameObject(menuPartyPokemonMovesController.moveButtons[0].gameObject);

            menuPartyPokemonMovesController.SetCurrentPokemonIndex(currentSelectedPartyPokemonIndex);

        }

    }
}
