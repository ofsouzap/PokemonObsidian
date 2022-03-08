using System;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Water_Spout : PokemonMove
    {

        public Move_Water_Spout()
        {

            id = 323; // ID here
            name = "Water Spout"; // Full name here
            description = "The user spouts water to damage the foe. The lower the user's HP, the less powerful it becomes."; // Description here
            type = Type.Water; // Type here
            moveType = MoveType.Special; // Move type here
            maxPP = 5; // PP here
            power = 150; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => Convert.ToByte((user.health / user.GetBattleStats().health) * 150);

    }
}
