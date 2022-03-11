using System;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Wake_Up_Slap : PokemonMove
    {

        public Move_Wake_Up_Slap()
        {

            id = 358; // ID here
            name = "Wake-Up Slap"; // Full name here
            description = "This attack inflicts high damage on a sleeping foe. It also wakes the foe up, however."; // Description here
            type = Type.Fighting; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 10; // PP here
            power = 60; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => Convert.ToByte(base.GetUsagePower(battleData, user, target) * (user.nonVolatileStatusCondition switch
            {
                PokemonInstance.NonVolatileStatusCondition.Asleep => 2, // Double power if target asleep
                _ => 1
            }));

        public override UsageResults CalculateNonVolatileStatusConditionChanges(UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            usageResults = base.CalculateNonVolatileStatusConditionChanges(usageResults, user, target, battleData);

            // Wake target if asleep
            if (target.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Asleep)
                usageResults.clearTargetNonVolatileStatusCondition = true;

            return usageResults;

        }

    }
}
