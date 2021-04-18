using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Menus;
using Pokemon;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu
{
    public class PokemonOptionsController : MonoBehaviour
    {

        public PokemonMenuController pokemonMenuController;

        public PokemonOptionController[] optionControllers;

        public MenuSelectableController[] GetSelectables()
            => optionControllers.Select(x => x.GetComponent<MenuSelectableController>()).ToArray();

        public void SetUp()
        {

            if (optionControllers.Length != 6)
                Debug.LogError("Incorrect number of option controllers");

            optionControllers[0].Selected.AddListener(() => pokemonMenuController.TrySetCurrentPokemonIndex(0));
            optionControllers[1].Selected.AddListener(() => pokemonMenuController.TrySetCurrentPokemonIndex(1));
            optionControllers[2].Selected.AddListener(() => pokemonMenuController.TrySetCurrentPokemonIndex(2));
            optionControllers[3].Selected.AddListener(() => pokemonMenuController.TrySetCurrentPokemonIndex(3));
            optionControllers[4].Selected.AddListener(() => pokemonMenuController.TrySetCurrentPokemonIndex(4));
            optionControllers[5].Selected.AddListener(() => pokemonMenuController.TrySetCurrentPokemonIndex(5));

            SetUpOptions();

        }

        public void SetUpOptions()
        {

            PokemonInstance[] pokemon = PlayerData.singleton.partyPokemon;

            for (int i = 0; i < pokemon.Length; i++)
            {

                if (pokemon[i] != null)
                {
                    optionControllers[i].SetInteractable(true);
                    optionControllers[i].UpdateValues(pokemon[i]);
                }
                else
                {
                    optionControllers[i].SetInteractable(false);
                }

            }

        }

    }
}