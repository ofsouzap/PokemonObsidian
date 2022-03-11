using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Spikes : PokemonMove
    {

        public Move_Spikes()
        {

            id = 191; // ID here
            name = "Spikes"; // Full name here
            description = "The user lays a trap of spikes at the foe's feet. The trap hurts foes that switch into battle."; // Description here
            type = Type.Ground; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override UsageResults CalculateEffect(PokemonInstance user, PokemonInstance target, BattleData battleData, bool allowMissing = true)
        {

            UsageResults usageResults = base.CalculateEffect(user, target, battleData, allowMissing);

            if (usageResults.Succeeded)
                usageResults.increaseTargetStageSpikes = true;

            return usageResults;

        }

    }
}
