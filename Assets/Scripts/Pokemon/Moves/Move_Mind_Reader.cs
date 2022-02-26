using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Mind_Reader : PokemonMove
    {

        public Move_Mind_Reader()
        {

            id = 170; // ID here
            name = "Mind Reader"; // Full name here
            description = "The user senses the foe's movements with its mind to ensure its next attack does not miss."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override void DoSetTakingAimUpdate(ref UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            base.DoSetTakingAimUpdate(ref usageResults, user, target, battleData);
            usageResults.setTakingAim = true;
        }

    }
}
