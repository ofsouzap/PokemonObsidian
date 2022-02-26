using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Steel_Wing : PokemonMove
    {

        public Move_Steel_Wing()
        {

            id = 211; // ID here
            name = "Steel Wing"; // Full name here
            description = "The foe is hit with wings of steel. It may also raise the user's Defense stat."; // Description here
            type = Type.Steel; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 25; // PP here
            power = 70; // Power here
            accuracy = 90; // Accuracy here

            userStatChangeChances = new StatChangeChance[1]
            {
                new StatChangeChance()
                {
                    chance = 0.1F,
                    statChanges = new Stats<sbyte>()
                    {
                        attack = 0,
                        defense = 1,
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
