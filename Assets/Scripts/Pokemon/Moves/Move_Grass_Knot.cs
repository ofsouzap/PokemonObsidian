using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Grass_Knot : PokemonMove
    {

        public Move_Grass_Knot()
        {

            id = 447; // ID here
            name = "Grass Knot"; // Full name here
            description = "The user snares the foe with grass and trips it. The heavier the foe, the greater the damage."; // Description here
            type = Type.Grass; // Type here
            moveType = MoveType.Special; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
        {

            if (target.species.weight < 10)
                return 20;
            else if (target.species.weight < 25)
                return 40;
            else if (target.species.weight < 50)
                return 60;
            else if (target.species.weight < 100)
                return 80;
            else if (target.species.weight < 200)
                return 100;
            else
                return 120;

        }

    }
}
