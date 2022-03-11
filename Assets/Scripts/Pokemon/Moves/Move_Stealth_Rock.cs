using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Stealth_Rock : PokemonMove
    {

        public Move_Stealth_Rock()
        {

            id = 446; // ID here
            name = "Stealth Rock"; // Full name here
            description = "The user lays a trap of levitating stones around the foe. The trap hurts foes that switch into battle."; // Description here
            type = Type.Rock; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override UsageResults CalculateEffect(PokemonInstance user, PokemonInstance target, BattleData battleData, bool allowMissing = true)
        {

            UsageResults usageResults = base.CalculateEffect(user, target, battleData, allowMissing);

            if (usageResults.Succeeded)
                usageResults.setTargetStagePointedStones = true;

            return usageResults;

        }

    }
}
