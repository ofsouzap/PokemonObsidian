using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Present : PokemonMove
    {

        public Move_Present()
        {

            id = 217; // ID here
            name = "Present"; // Full name here
            description = "The user attacks by giving the foe a surprise booby-trapped gift. Sometimes this move is weaker, sometimes it is stronger"; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 15; // PP here
            power = 0; // Power here
            accuracy = 90; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
        {

            float r = battleData.RandomValue01();

            if (r <= 0.2F)
                return 5;
            else if (r <= 0.6F)
                return 40;
            else if (r <= 0.9F)
                return 80;
            else
                return 120;

        }

    }
}
