using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Defog : PokemonMove
    {

        public Move_Defog()
        {

            id = 432; // ID here
            name = "Defog"; // Full name here
            description = "Obstacles are moved, reducing the foe's evasion stat. It can also be used to clear deep fog, etc."; // Description here
            type = Type.Flying; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 15; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

            targetEvasionModifier = -1;

        }

        public override UsageResults CalculateEffect(PokemonInstance user, PokemonInstance target, BattleData battleData, bool allowMissing = true)
        {

            UsageResults usageResults = base.CalculateEffect(user, target, battleData, allowMissing);

            if (usageResults.Succeeded)
            {
                usageResults.clearUserStageSpikes = true;
                usageResults.clearUserStageToxicSpikes = true;
                usageResults.clearUserStagePointedStones = true;
            }

            return usageResults;

        }

    }
}
