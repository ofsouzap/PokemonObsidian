using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Menus;
using Pokemon;

namespace FreeRoaming.Menu.PlayerMenus.PokedexMenu
{
    public class PokedexPokemonListController : ScrollListController<KeyValuePair<PokemonSpecies, bool>>
    {

        public UnityEvent<int> speciesIndexSelected;

        protected override GameObject GenerateListItem(KeyValuePair<PokemonSpecies, bool> item, int index)
        {

            GameObject newItem = Instantiate(scrollListItemPrefab, itemsListContentTransform);
            PokedexPokemonListItemController controller = newItem.GetComponent<PokedexPokemonListItemController>();

            newItem.GetComponent<Button>().onClick.AddListener(() =>
            {
                speciesIndexSelected.Invoke(index);
                SetCurrentSelectionIndex(index);
                itemSelectedAction(index);
            });

            controller.SetPositionIndex(index, itemsPadding);
            controller.SetValues(item);

            return newItem;

        }

    }
}