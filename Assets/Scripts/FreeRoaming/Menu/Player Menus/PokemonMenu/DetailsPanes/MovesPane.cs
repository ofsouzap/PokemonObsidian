using UnityEngine;
using UnityEngine.UI;
using Pokemon;

namespace FreeRoaming.Menu.PlayerMenus.PokemonMenu.DetailsPanes
{
    public class MovesPane : DetailsPane
    {

        public MovesPane_MovePane[] movePaneControllers;

        public override void SetUp()
        {

            base.SetUp();

            if (movePaneControllers.Length != 4)
            {
                Debug.LogError("Non-4 number of move pane controllers provided");
            }

        }

        public override void RefreshDetails(PokemonInstance pokemon)
        {

            for (byte i = 0; i < pokemon.moveIds.Length; i++)
                movePaneControllers[i].SetMoveById(pokemon.moveIds[i], pokemon.movePPs[i]);

        }

    }
}
