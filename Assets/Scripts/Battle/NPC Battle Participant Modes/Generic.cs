using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;

namespace Battle.NPCBattleParticipantModes
{
    public abstract class Generic : BattleParticipantNPC
    {

        public override void StartChoosingNextPokemon()
        {

            //Generic opponents use their pokemon in the order they have them

            int selectedIndex = -1;
            bool validIndexFound = false;

            for (int i = 0; i < pokemon.Length; i++)
            {

                if (!GetPokemon()[i].IsFainted)
                {
                    selectedIndex = i;
                    validIndexFound = true;
                    break;
                }

            }

            if (!validIndexFound)
            {
                Debug.LogError("Unable to find valid next pokemon index");
                nextPokemonHasBeenChosen = false;
                return;
            }

            chosenNextPokemonIndex = selectedIndex;
            nextPokemonHasBeenChosen = true;

        }

    }
}
