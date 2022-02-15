using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;
using Pokemon.Moves;

namespace Battle.NPCBattleParticipantModes
{
    public class WildPokemon : Generic
    {

        /* This mode will be very simple: moves with type advantage will be slightly favoured
         * Formula for type advantage weighting:
         *     w = sqrt(e)
         *     'w' - weighting multiplier
         *     'e' - effectiveness multiplier
         */

        public WildPokemon(string name,
            PokemonInstance[] pokemon,
            byte basePayout,
            string[] defeatMessages)
        {

            if (pokemon.Length > 1)
                Debug.LogWarning("Multiple pokemon set for wild pokemon controller. Action-choosing may not work as expected");

            npcName = name;
            this.pokemon = pokemon;
            this.basePayout = basePayout;
            this.defeatMessages = defeatMessages;

        }

        public override void StartChoosingAction(BattleData battleData)
        {

            if (!ActivePokemon.HasUsableMove)
            {
                chosenAction = new Action(this)
                {
                    type = Action.Type.Fight,
                    fightUsingStruggle = true,
                    fightMoveTarget = battleData.participantPlayer
                };
                actionHasBeenChosen = true;
            }

            //This script is always for the opponent so battleData.participantPlayer is used to find the opponent
            PokemonInstance opposingPokemon = battleData.participantPlayer.ActivePokemon;

            int selectedMoveIndex = SelectFromWeightings(GetMoveWeightings(opposingPokemon));

            chosenAction = new Action(this)
            {
                type = Action.Type.Fight,
                fightUsingStruggle = false,
                fightMoveTarget = battleData.participantPlayer,
                fightMoveIndex = selectedMoveIndex
            };
            actionHasBeenChosen = true;

        }

        private float GetTypeAdvantageWeighting(float typeAdvantageMultiplier)
            => Mathf.Sqrt(typeAdvantageMultiplier);

        private float[] GetMoveWeightings(PokemonInstance opposingPokemon)
        {

            float[] weightings = new float[ActivePokemon.moveIds.Length];

            if (ActivePokemon.battleProperties.volatileStatusConditions.encoreTurns > 0)
                weightings = GetEncoreMoveWeightings();
            else
            {
                for (int i = 0; i < ActivePokemon.moveIds.Length; i++)
                {

                    weightings[i] = 1;

                    //Move unset
                    if (PokemonMove.MoveIdIsUnset(ActivePokemon.moveIds[i]))
                    {
                        weightings[i] = 0;
                        continue;
                    }

                    //Out of PP
                    if (ActivePokemon.movePPs[i] <= 0)
                        weightings[i] = 0;

                    PokemonMove move = PokemonMove.GetPokemonMoveById(ActivePokemon.moveIds[i]);

                    //Being taunted
                    if (ActivePokemon.battleProperties.volatileStatusConditions.tauntTurns > 0
                        && move.moveType == PokemonMove.MoveType.Status)
                        weightings[i] = 0;

                    //Being tormented
                    if (ActivePokemon.battleProperties.volatileStatusConditions.torment
                        && ActivePokemon.battleProperties.lastMoveId == move.id)
                        weightings[i] = 0;

                    //Type advantage
                    weightings[i] *= GetTypeAdvantageWeighting(TypeAdvantage.CalculateMultiplier(move.type, opposingPokemon.species));

                }

            }

            return weightings;

        }

        private int SelectFromWeightings(float[] rawWeightings)
        {

            float[] normalisedWeightings = NormaliseWeightings(rawWeightings);

            float r = battleManager.battleData.RandomValue01();

            float currentTotal = 0;
            for (int i = 0; i < normalisedWeightings.Length; i++)
            {

                currentTotal += normalisedWeightings[i];

                if (r <= currentTotal)
                    return i;

            }

            Debug.LogError("No selection able to be made from weightings");

            return 0;

        }

        private float[] NormaliseWeightings(float[] rawWeightings)
        {

            if (rawWeightings.All(x => x == 0))
                return rawWeightings.Select(x => 1F).ToArray();

            float total = rawWeightings.Sum();

            return rawWeightings.Select(x => x / total).ToArray();

        }

    }
}
