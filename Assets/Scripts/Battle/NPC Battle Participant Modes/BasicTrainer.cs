using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;
using Pokemon.Moves;

namespace Battle.NPCBattleParticipantModes
{
    public class BasicTrainer : Generic
    {

        /* This mode will use a random move where the moves are weighted by their effectiveness against the target
         * If a move has no PP, it won't be used (its weighting will be set to 0)
         * If the pokemon hasn't used any "stat (oe)" moves yet, the weightings for "stat (oe)" moves will be much higher
         *     "stat (oe)" - moves that affect a pokemon's stat modifiers or the target's non-volatile status condition
         * Weightings are relative to each other (they don't necessarily sum to 1)
         * Weighting for "stat (oe)" moves:
         *     w = 0.1 + (1 / (s + 0.25) )
         *     'w' - weighting multiplier
         *     's' - status moves used
         * Weighting for attack moves:
         *     w = sqrt(e) + 0.1
         *     'w' - weighting multiplier
         *     'e' - effectiveness multiplier of move (eg. 2, 1, 1/2)
         * If opposing pokemon's health is under 20% and this participant's active pokemon has a priority move, this will use that
         * If this participant's pokemon's health is low, They will have a higher chance of using a healing move
         * Weighting for healing moves:
         *     w = -4h + 5 {0 <= h <= 0.5}
         *     w = 1 {0.5 <= h <= 1}
         *     'w' - weighting
         *     'h' - proportion of pokemon's max. health that they are at
        */

        private static float GetStatOEWeighting(byte statusMovesUsed)
            => (float)(0.1 + (1 / (statusMovesUsed + 0.25)));

        private static float GetAttackMoveWeighting(float effectivenessModifier)
            => Mathf.Sqrt(effectivenessModifier);

        private static float GetHealingMoveWeighint(float healthProportion)
            => healthProportion < 0.5f
            ? (float)((-4 * healthProportion) + 5)
            : 1;

        private const float priorityMoveOpponentHealthThreshold = 0.2F;

        private byte statOEMovesUsed;

        public BasicTrainer(string name,
            PokemonInstance[] pokemon,
            byte basePayout,
            string[] defeatMessages)
        {

            npcName = name;
            this.pokemon = pokemon;
            this.basePayout = basePayout;
            this.defeatMessages = defeatMessages;

            statOEMovesUsed = 0;

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

            //If opposing pokemon health below threshold, try use priority move
            if (((float)opposingPokemon.health / opposingPokemon.GetStats().health) < priorityMoveOpponentHealthThreshold)
            {

                int? priorityMoveIndex = FindUsablePriorityMoveIndex(ActivePokemon);

                if (priorityMoveIndex != null)
                {

                    chosenAction = new Action(this)
                    {
                        type = Action.Type.Fight,
                        fightUsingStruggle = false,
                        fightMoveTarget = battleData.participantPlayer,
                        fightMoveIndex = (int)priorityMoveIndex
                    };
                    actionHasBeenChosen = true;

                    return;

                }

            }

            float[] moveWeightings = GetMoveWeightings(opposingPokemon);
            int selectedMoveIndex = SelectFromWeightings(moveWeightings);

            chosenAction = new Action(this)
            {
                type = Action.Type.Fight,
                fightUsingStruggle = false,
                fightMoveTarget = battleData.participantPlayer,
                fightMoveIndex = selectedMoveIndex
            };
            actionHasBeenChosen = true;

            if (MoveIsStatOEMove(PokemonMove.GetPokemonMoveById(ActivePokemon.moveIds[selectedMoveIndex])))
            {
                statOEMovesUsed++;
            }

        }

        private int? FindUsablePriorityMoveIndex(PokemonInstance pokemon)
        {

            for (int i = 0; i < pokemon.moveIds.Length; i++)
            {

                if (PokemonMove.MoveIdIsUnset(pokemon.moveIds[i]))
                    continue;

                if (PokemonMove.GetPokemonMoveById(pokemon.moveIds[i]).movePriority == true
                    && pokemon.movePPs[i] > 0)
                    return i;

            }

            return null;

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

        private float[] GetMoveWeightings(PokemonInstance target)
        {

            float[] weightings = new float[] { 1, 1, 1, 1 };

            for (int i = 0; i < ActivePokemon.moveIds.Length; i++)
            {

                if (PokemonMove.MoveIdIsUnset(ActivePokemon.moveIds[i]))
                {
                    weightings[i] = 0;
                    continue;
                }

                PokemonMove move = PokemonMove.GetPokemonMoveById(ActivePokemon.moveIds[i]);

                float effectivenessModifier = target.species.type2 == null
                    ? TypeAdvantage.CalculateMultiplier(move.type, target.species.type1)
                    : TypeAdvantage.CalculateMultiplier(move.type, target.species.type1, (Type)target.species.type2);

                if (ActivePokemon.movePPs[i] <= 0)
                    weightings[i] = 0;

                if (move.moveType == PokemonMove.MoveType.Status)
                    weightings[i] *= GetStatOEWeighting(statOEMovesUsed);
                else
                    weightings[i] *= GetAttackMoveWeighting(effectivenessModifier);

                weightings[i] *= GetHealingMoveWeighint((float)ActivePokemon.health / ActivePokemon.GetStats().health);

            }

            return weightings;

        }

        private bool MoveIsStatOEMove(PokemonMove move)
        {

            bool nvscCondition = false;

            foreach (PokemonInstance.NonVolatileStatusCondition nvsc in move.nonVolatileStatusConditionChances.Keys)
                if (move.nonVolatileStatusConditionChances[nvsc] >= 1)
                    nvscCondition = true;

            return move.targetStatChanges.GetEnumerator(false).Any(x => x != 0)
                || move.userStatChanges.GetEnumerator(false).Any(x => x != 0)
                || nvscCondition;

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
