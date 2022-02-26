using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_AncientPower : PokemonMove
    {

        public Move_AncientPower()
        {

            id = 246; // ID here
            name = "AncientPower"; // Full name here
            description = "The user attacks with a prehistoric power. It may also raise all the user's stats at once."; // Description here
            type = Type.Rock; // Type here
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
                        speed = 1,
                        health = 1
                    }
                }
            };

        }

    }
}
