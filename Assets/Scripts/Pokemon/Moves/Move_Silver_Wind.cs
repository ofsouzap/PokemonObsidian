using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Silver_Wind : PokemonMove
    {

        public Move_Silver_Wind()
        {

            id = 318; // ID here
            name = "Silver Wind"; // Full name here
            description = "The foe is attacked with powdery scales blown by wind. It may also raise all the user's stats."; // Description here
            type = Type.Bug; // Type here
            moveType = MoveType.Special; // Move type here
            maxPP = 5; // PP here
            power = 60; // Power here
            accuracy = 100; // Accuracy here

            userStatChangeChances = new StatChangeChance[1]
            {
                new StatChangeChance()
                {
                    chance = 0.1F,
                    statChanges = new Stats<sbyte>()
                    {
                        attack = 1,
                        defense = 1,
                        specialAttack = 1,
                        specialDefense = 1,
                        speed = 1
                    }
                }
            };

        }

    }
}
