using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Ingrain : PokemonMove
    {

        public Move_Ingrain()
        {

            id = 275; // ID here
            name = "Ingrain"; // Full name here
            description = "The user lays roots that restore HP on every turn. Because it is rooted, it can't switch out."; // Description here
            type = Type.Grass; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override void DoRootingUpdate(ref UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            base.DoRootingUpdate(ref usageResults, user, target, battleData);

            usageResults.setRooting = true;

        }

    }
}
