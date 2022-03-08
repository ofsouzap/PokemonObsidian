using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Lock_On : PokemonMove
    {

        public Move_Lock_On()
        {

            id = 199; // ID here
            name = "Lock-On"; // Full name here
            description = "The user takes sure aim at the foe. It ensures the next attack does not fail to hit the target."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override void DoSetTakingAimUpdate(ref UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            base.DoSetTakingAimUpdate(ref usageResults, user, target, battleData);
            usageResults.setTakingAim = true;
        }

    }
}
