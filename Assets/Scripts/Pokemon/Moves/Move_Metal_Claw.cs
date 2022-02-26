using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Metal_Claw : PokemonMove
    {

        public Move_Metal_Claw()
        {

            id = 232; // ID here
            name = "Metal Claw"; // Full name here
            description = "The foe is raked with steel claws. It may also raise the user's Attack stat."; // Description here
            type = Type.Steel; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 35; // PP here
            power = 50; // Power here
            accuracy = 95; // Accuracy here

            userStatChangeChances = new StatChangeChance[1]
            {
                new StatChangeChance()
                {
                    chance = 0.1F,
                    statChanges = new Stats<sbyte>()
                    {
                        attack = 1,
                        defense = 0,
                        specialAttack = 0,
                        specialDefense = 0,
                        speed = 0,
                        health = 0
                    }
                }
            };

        }

    }
}
