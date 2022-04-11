using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Low_Kick : PokemonMove
    {

        public Move_Low_Kick()
        {

            id = 67; // ID here
            name = "Low Kick"; // Full name here
            description = "A powerful low kick that makes the foe fall over. It inflicts greater damage on heavier foes."; // Description here
            type = Type.Fighting; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
        {

            float weight = target.species.weight;

            if (weight < 10)
                return 20;
            else if (weight < 25)
                return 40;
            else if (weight < 50)
                return 60;
            else if (weight < 100)
                return 80;
            else if (weight < 200)
                return 100;
            else
                return 120;

        }

    }
}
