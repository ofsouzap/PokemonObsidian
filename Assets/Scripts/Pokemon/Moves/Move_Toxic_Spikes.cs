using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Toxic_Spikes : PokemonMove
    {

        public Move_Toxic_Spikes()
        {

            id = 390; // ID here
            name = "Toxic Spikes"; // Full name here
            description = "The user lays a trap of poison spikes at the foe's feet. They poison foes that switch into battle."; // Description here
            type = Type.Poison; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override UsageResults CalculateEffect(PokemonInstance user, PokemonInstance target, BattleData battleData, bool allowMissing = true)
        {

            UsageResults usageResults = base.CalculateEffect(user, target, battleData, allowMissing);

            if (usageResults.Succeeded)
                usageResults.increaseTargetToxicStageSpikes = true;

            return usageResults;

        }

    }
}
