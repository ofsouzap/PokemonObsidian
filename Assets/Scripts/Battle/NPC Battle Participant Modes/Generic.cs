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

        /// <summary>
        /// Get the weightings for choosing a move when the active pokemon is under the influence of an encore.
        /// </summary>
        protected float[] GetEncoreMoveWeightings()
        {

            float[] weightings;

            int encoreMoveIndex = -1;

            for (int i = 0; i < ActivePokemon.moveIds.Length; i++)
                if (ActivePokemon.moveIds[i] == ActivePokemon.battleProperties.volatileStatusConditions.encoreMoveId)
                {
                    encoreMoveIndex = i;
                    break;
                }

            if (encoreMoveIndex < 0)
            {
                Debug.LogError("Unable to find encore move in participant active pokemon moves");
                weightings = new float[4] { 1, 1, 1, 1 };
                return weightings;
            }

            weightings = new float[4] { 0, 0, 0, 0 };
            weightings[encoreMoveIndex] = 1;

            return weightings;

        }

    }
}
