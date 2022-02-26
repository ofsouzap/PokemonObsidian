using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Feint_Attack : PokemonMove
    {

        public Move_Feint_Attack()
        {

            id = 185; // ID here
            name = "Feint Attack"; // Full name here
            description = "The user draws up to the foe disarmingly, then throws a sucker punch. It hits without fail."; // Description here
            type = Type.Dark; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 20; // PP here
            power = 60; // Power here
            accuracy = 0; // Accuracy here

        }

    }
}
