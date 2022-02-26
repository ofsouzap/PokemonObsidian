using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Magnitude : PokemonMove
    {

        public Move_Magnitude()
        {

            id = 222; // ID here
            name = "Magnitude"; // Full name here
            description = "The user looses a ground-shaking quake affecting everyone in battle. Its power varies."; // Description here
            type = Type.Ground; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 30; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
        {

            float r = battleData.RandomValue01();

            if (r <= 5)
                return 10;
            else if (r <= 15)
                return 30;
            else if (r <= 35)
                return 50;
            else if (r <= 65)
                return 70;
            else if (r <= 85)
                return 90;
            else if (r <= 95)
                return 110;
            else
                return 150;

        }

    }
}
