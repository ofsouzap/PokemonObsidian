using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Nightmare : PokemonMove
    {

        public Move_Nightmare()
        {

            id = 171; // ID here
            name = "Nightmare"; // Full name here
            description = "A sleeping foe is shown a nightmare that inflicts some damage every turn."; // Description here
            type = Type.Ghost; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 15; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override bool GetInflictsNightmare(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => true;

    }
}
