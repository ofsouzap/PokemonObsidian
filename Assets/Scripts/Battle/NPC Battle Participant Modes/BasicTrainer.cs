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

        protected static float GetStatOEWeighting(byte statusMovesUsed)
            => (float)(0.1 + (1 / (statusMovesUsed + 0.25)));

        protected static float GetAttackMoveWeighting(float effectivenessModifier)
            => Mathf.Sqrt(effectivenessModifier);

        protected static float GetHealingMoveWeighint(float healthProportion)
            => healthProportion < 0.5f
            ? (float)((-4 * healthProportion) + 5)
            : 1;

        protected const float priorityMoveOpponentHealthThreshold = 0.2F;

        protected byte statOEMovesUsed;

        public BasicTrainer(TrainersData.TrainerDetails details)
        {

            npcName = details.GetFullName();
            pokemon = details.GenerateParty();
            basePayout = details.GetBasePayout();
            defeatMessages = details.defeatMessages;

            statOEMovesUsed = 0;

        }

        public override void StartChoosingAction(BattleData battleData)
        {

            base.StartChoosingAction(battleData);

            if (actionHasBeenChosen)
                return;

            if (!ActivePokemon.HasUsableMove)
            {
                SetChosenAction(new Action(this)
                {
                    type = Action.Type.Fight,
                    fightUsingStruggle = true,
                    fightMoveTarget = battleData.participantPlayer
                });
            }
            else
                ChooseAction(battleData);

        }

        protected virtual void ChooseAction(BattleData battleData)
        {

            if (actionHasBeenChosen)
                return;

            //This script is always for the opponent so battleData.participantPlayer is used to find the opponent
            PokemonInstance opposingPokemon = battleData.participantPlayer.ActivePokemon;

            //If opposing pokemon health below threshold, try use priority move
            if (opposingPokemon.HealthProportion < priorityMoveOpponentHealthThreshold)
            {

                int? priorityMoveIndex = FindUsablePriorityMoveIndex(ActivePokemon);

                if (priorityMoveIndex != null)
                {

                    SetChosenAction(new Action(this)
                    {
                        type = Action.Type.Fight,
                        fightUsingStruggle = false,
                        fightMoveTarget = battleData.participantPlayer,
                        fightMoveIndex = (int)priorityMoveIndex
                    });

                    return;

                }

            }

            //Normal case is to choose a move based on the moves' weightings
            float[] moveWeightings = GetMoveWeightings(opposingPokemon);
            int selectedMoveIndex = SelectFromWeightings(moveWeightings);

            SetChosenAction(new Action(this)
            {
                type = Action.Type.Fight,
                fightUsingStruggle = false,
                fightMoveTarget = battleData.participantPlayer,
                fightMoveIndex = selectedMoveIndex
            });

            PokemonMove move = PokemonMove.GetPokemonMoveById(ActivePokemon.moveIds[selectedMoveIndex]);

            if (move == null)
                Debug.LogError("Move selected from weightings was null");

            if (MoveIsStatOEMove(move))
            {
                statOEMovesUsed++;
            }

        }

        protected int? FindUsablePriorityMoveIndex(PokemonInstance pokemon)
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

        protected int SelectFromWeightings(float[] rawWeightings)
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

        protected float[] GetMoveWeightings(PokemonInstance target)
        {

            float[] weightings = new float[4] { 1, 1, 1, 1 };

            if (ActivePokemon.battleProperties.volatileStatusConditions.encoreTurns > 0)
                weightings = GetEncoreMoveWeightings();
            else
            {

                for (int i = 0; i < ActivePokemon.moveIds.Length; i++)
                {

                    //Move unset
                    if (PokemonMove.MoveIdIsUnset(ActivePokemon.moveIds[i]))
                    {
                        weightings[i] = 0;
                        continue;
                    }

                    PokemonMove move = PokemonMove.GetPokemonMoveById(ActivePokemon.moveIds[i]);

                    //Being taunted
                    if (ActivePokemon.battleProperties.volatileStatusConditions.tauntTurns > 0
                        && move.moveType == PokemonMove.MoveType.Status)
                        weightings[i] = 0;

                    //Being tormented
                    if (ActivePokemon.battleProperties.volatileStatusConditions.torment
                        && ActivePokemon.battleProperties.lastMoveId == move.id)
                        weightings[i] = 0;

                    float effectivenessModifier = target.species.type2 == null
                        ? TypeAdvantage.CalculateMultiplier(move.type, target.species.type1)
                        : TypeAdvantage.CalculateMultiplier(move.type, target.species.type1, (Type)target.species.type2);

                    //Out of PP
                    if (ActivePokemon.movePPs[i] <= 0)
                        weightings[i] = 0;

                    //Status or attack move
                    if (move.moveType == PokemonMove.MoveType.Status)
                        weightings[i] *= GetStatOEWeighting(statOEMovesUsed);
                    else
                        weightings[i] *= GetAttackMoveWeighting(effectivenessModifier);

                    //Healing weight
                    weightings[i] *= GetHealingMoveWeighint(ActivePokemon.HealthProportion);

                }

            }

            return weightings;

        }

        protected bool MoveIsStatOEMove(PokemonMove move)
        {

            bool nvscCondition = false;

            if (move.nonVolatileStatusConditionChances != null)
                foreach (PokemonInstance.NonVolatileStatusCondition nvsc in move.nonVolatileStatusConditionChances.Keys)
                    if (move.nonVolatileStatusConditionChances[nvsc] >= 1)
                        nvscCondition = true;

            return move.targetStatChanges.GetEnumerator(false).Any(x => x != 0)
                || move.userStatChanges.GetEnumerator(false).Any(x => x != 0)
                || nvscCondition;

        }

        protected float[] NormaliseWeightings(float[] rawWeightings)
        {

            if (rawWeightings.All(x => x == 0))
                return rawWeightings.Select(x => 1F).ToArray();

            float total = rawWeightings.Sum();

            return rawWeightings.Select(x => x / total).ToArray();

        }

    }
}
