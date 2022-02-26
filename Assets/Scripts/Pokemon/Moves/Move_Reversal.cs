using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Reversal : PokemonMove
    {

        public Move_Reversal()
        {

            id = 179; // ID here
            name = "Reversal"; // Full name here
            description = "An all-out attack that becomes more powerful the less HP the user has."; // Description here
            type = Type.Fighting; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 15; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override byte GetUsagePower(BattleData battleData, PokemonInstance user, PokemonInstance target)
        {

            // https://bulbapedia.bulbagarden.net/wiki/Reversal_(move)#Generation_IV

            float healthProp = user.health / (float)user.GetStats().health;

            if (healthProp >= 0.6719F)
                return 20;
            else if (healthProp >= 0.3438F)
                return 40;
            else if (healthProp >= 0.2031F)
                return 80;
            else if (healthProp >= 0.938F)
                return 100;
            else if (healthProp >= 0.313F)
                return 150;
            else
                return 200;

        }

    }
}
