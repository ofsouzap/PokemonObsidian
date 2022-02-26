using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Outrage : PokemonMove
    {

        public Move_Outrage()
        {

            id = 200; // ID here
            name = "Outrage"; // Full name here
            description = "The user rampages and attacks for a few turns. However, it then becomes confused."; // Description here
            type = Type.Dragon; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 10; // PP here
            power = 120; // Power here
            accuracy = 100; // Accuracy here

        }

        public override void DoThrashingUpdate(ref UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            base.DoThrashingUpdate(ref usageResults, user, target, battleData);
            usageResults.thrashingTurns = PokemonInstance.BattleProperties.VolatileBattleStatus.GetRandomThrashingDuration(battleData);
        }

    }
}
