using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Crush_Grip : PokemonMove
    {

        public Move_Crush_Grip()
        {

            id = 462; // ID here
            name = "Crush Grip"; // Full name here
            description = "The foe is crushed with great force. The attack is more powerful the more HP the foe has left."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => Convert.ToByte(1 + (120 * (target.HealthProportion)));

    }
}
