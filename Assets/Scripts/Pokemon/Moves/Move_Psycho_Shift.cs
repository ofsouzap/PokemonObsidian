using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Psycho_Shift : PokemonMove
    {

        public Move_Psycho_Shift()
        {

            id = 375; // ID here
            name = "Psycho Shift"; // Full name here
            description = "Using its psychic power of suggestion, the user transfers its status problems to the target."; // Description here
            type = Type.Psychic; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 10; // PP here
            power = 0; // Power here
            accuracy = 90; // Accuracy here

        }

        public override UsageResults CalculateNonVolatileStatusConditionChanges(UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            if (user.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.None)
            {
                usageResults.failed = true;
                return usageResults;
            }
            else
            {

                usageResults.clearUserNonVolatileStatusCondition = true;
                usageResults.targetNonVolatileStatusCondition = user.nonVolatileStatusCondition;

                return usageResults;

            }

        }

    }
}
