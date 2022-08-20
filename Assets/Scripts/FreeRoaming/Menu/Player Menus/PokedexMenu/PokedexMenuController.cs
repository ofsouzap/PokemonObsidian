using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Menus;
using Pokemon;

namespace FreeRoaming.Menu.PlayerMenus.PokedexMenu
{
    public class PokedexMenuController : PlayerMenuController
    {

        [SerializeField]
        private sbyte navigationSkipAmount = 5;

        public GameObject speciesBorderPrefab;

        public Button buttonBack;

        public PokedexPokemonListController speciesList;

        public GeneralDetailsArea generalDetails;

        public GenderSpritesAreaController maleSprites;
        public GenderSpritesAreaController femaleSprites;

        public StatsAreaController stats;

        private int currentListIndex;

        private KeyValuePair<PokemonSpecies, bool>[] items;

        protected override GameObject GetDefaultSelectedGameObject()
            => null;

        protected override MenuSelectableController[] GetSelectables()
            => new MenuSelectableController[0];

        protected override void SetUp()
        {

            speciesList.SetUp(speciesBorderPrefab, index => SetCurrentSpeciesListIndex(index));
            items = GenerateSpeciesListItemValues();
            speciesList.SetItems(items);

            buttonBack.onClick.AddListener(CloseMenu);

            generalDetails.SetUp();

            currentListIndex = 0;
            speciesList.SetCurrentSelectionIndex(currentListIndex);
            SetCurrentSpeciesListIndex(currentListIndex);
            speciesList.speciesIndexSelected.RemoveAllListeners();
            speciesList.speciesIndexSelected.AddListener(index =>
            {
                currentListIndex = index;
                SetCurrentSpeciesListIndex(currentListIndex);
            });

        }

        #region Control

        private float lastNavigationMove = float.MinValue;
        [SerializeField]
        private float navigationMoveDelay = 0.25f;

        protected override void Update()
        {

            base.Update();

            #region Navigation

            if (Time.time - lastNavigationMove >= navigationMoveDelay)
            {

                if (Input.GetAxis("Vertical") != 0)
                {

                    ChangeListIndex(Input.GetAxis("Vertical") > 0 ? (sbyte)-1 : (sbyte)1); //N.B. opposite directions used as items list indexing works in different direction

                    lastNavigationMove = Time.time;

                }
                else if (Input.GetAxis("Horizontal") != 0)
                {

                    ChangeListIndex(Input.GetAxis("Horizontal") > 0 ? navigationSkipAmount : (sbyte)-navigationSkipAmount);

                    lastNavigationMove = Time.time;

                }

            }

            // Allow repeated press-then-release of navigation buttons to move through the pokedex faster
            if (Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0)
            {
                lastNavigationMove = 0;
            }

            #endregion

            if (Input.GetButtonDown("Submit"))
            {
                generalDetails.TryPlayCurrentPokemonCry();
            }

        }

        private void ChangeListIndex(sbyte amount)
        {

            int maxValue = items.Length;

            currentListIndex += amount + maxValue;
            currentListIndex %= maxValue;

            speciesList.SetCurrentSelectionIndex(currentListIndex);
            SetCurrentSpeciesListIndex(currentListIndex);

        }

        #endregion

        protected KeyValuePair<PokemonSpecies, bool>[] GenerateSpeciesListItemValues(PlayerData player = null)
        {

            player ??= PlayerData.singleton;

            List<KeyValuePair<PokemonSpecies, bool>> items = new List<KeyValuePair<PokemonSpecies, bool>>();

            foreach (PokemonSpecies species in PokemonSpecies.registry.GetArray())
            {

                bool encountered = player.pokedex.GetHasBeenEncountered(species.id);
                
                items.Add(new KeyValuePair<PokemonSpecies, bool>(species, encountered));

            }

            return items.ToArray();

        }

        protected void SetCurrentSpeciesListIndex(int index)
        {
            
            PokemonSpecies species = items[index].Key;
            bool seen = items[index].Value;

            if (seen)
            {

                maleSprites.SetSpecies(species);
                femaleSprites.SetSpecies(species);

                generalDetails.SetSpecies(species);

                stats.SetSpecies(species);

            }
            else
            {

                maleSprites.SetUnseenSpecies();
                femaleSprites.SetUnseenSpecies();
                generalDetails.SetUnseenSpecies();
                stats.SetUnseenSpecies();

            }

        }

    }
}
