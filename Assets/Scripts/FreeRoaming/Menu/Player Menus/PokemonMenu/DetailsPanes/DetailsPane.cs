using System.Collections;
using UnityEngine;
using Pokemon;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu.DetailsPanes
{
    public abstract class DetailsPane : MonoBehaviour
    {

        public abstract void RefreshDetails(PokemonInstance pokemon);

    }
}
