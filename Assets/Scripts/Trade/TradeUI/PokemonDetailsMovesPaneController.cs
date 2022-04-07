using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Pokemon;
using Pokemon.Moves;

namespace Trade.TradeUI
{
    public class PokemonDetailsMovesPaneController : PokemonDetailsPaneController
    {

        public PokemonDetailsMovesPaneMoveContainerController[] moveDetailsContainers;

        public override void SetPokemon(PokemonInstance pokemon)
        {

            if (pokemon != null)
            {

                for (byte i = 0; i < pokemon.moveIds.Length; i++)
                {

                    PokemonMove move = PokemonMove.GetPokemonMoveById(pokemon.moveIds[i]);

                    moveDetailsContainers[i].SetMove(move);

                }

            }
            else
            {
                foreach (PokemonDetailsMovesPaneMoveContainerController moveContainer in moveDetailsContainers)
                    moveContainer.SetMove(null);
            }

        }

    }
}