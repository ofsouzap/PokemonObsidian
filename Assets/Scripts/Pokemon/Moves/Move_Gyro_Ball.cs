using System;
using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Gyro_Ball : PokemonMove
    {

        public Move_Gyro_Ball()
        {

            id = 360; // ID here
            name = "Gyro Ball"; // Full name here
            description = "The user tackles the foe with a high-speed spin. The slower the user, the greater the damage"; // Description here
            type = Type.Steel; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
        {

            int targetSpeed = target.GetStats().speed;
            int userSpeed = user.GetStats().speed;

            if (userSpeed == 0)
                userSpeed = 1;

            int calcPower = ((25 * targetSpeed) / userSpeed) + 1;

            if (calcPower > 150)
                return 150;
            else
                return Convert.ToByte(calcPower);

        }

    }
}
