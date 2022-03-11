using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Ominous_Wind : PokemonMove
    {

        public Move_Ominous_Wind()
        {

            id = 466; // ID here
            name = "Ominous Wind"; // Full name here
            description = "The user creates a gust of repulsive wind. It may also raise all the user's stats at once."; // Description here
            type = Type.Ghost; // Type here
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
