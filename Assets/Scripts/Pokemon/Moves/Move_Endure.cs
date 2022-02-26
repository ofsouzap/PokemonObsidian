using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Endure : PokemonMove
    {

        public Move_Endure()
        {

            id = 203; // ID here
            name = "Endure"; // Full name here
            description = "The user endures any attack, leaving 1 HP. Its chance of failing rises if it is used in succession."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 10; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override void DoBracingUpdate(ref UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            base.DoBracingUpdate(ref usageResults, user, target, battleData);
            usageResults.setBracing = true;
        }

    }
}
