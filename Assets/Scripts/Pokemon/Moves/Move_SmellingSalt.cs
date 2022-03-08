using System;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_SmellingSalt : PokemonMove
    {

        public Move_SmellingSalt()
        {

            id = 265; // ID here
            name = "SmellingSalt"; // Full name here
            description = "This attack inflicts double damage on a paralyzed foe. It also cures the foe's paralysis, however."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 10; // PP here
            power = 60; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
        {

            byte basePower = base.GetUsagePower(battleData, user, target);

            if (target.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Paralysed)
                return Convert.ToByte(basePower * 2);
            else
                return basePower;

        }

        public override UsageResults CalculateEffect(PokemonInstance user, PokemonInstance target, BattleData battleData, bool allowMissing = true)
        {

            UsageResults usageResults = base.CalculateEffect(user, target, battleData, allowMissing);

            if (target.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Paralysed)
                usageResults.clearTargetNonVolatileStatusCondition = true;

            return usageResults;

        }

    }
}
