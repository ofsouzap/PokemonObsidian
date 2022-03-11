using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Embargo : PokemonMove
    {

        public Move_Embargo()
        {

            id = 373; // ID here
            name = "Embargo"; // Full name here
            description = "It prevents the foe from using its held item. Its Trainer is also prevented from using items on it."; // Description here
            type = Type.Dark; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 10; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override bool GetInflictsEmbargo(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => true;

    }
}
