using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Meteor_Mash : PokemonMove
    {

        public Move_Meteor_Mash()
        {

            id = 309; // ID here
            name = "Meteor Mash"; // Full name here
            description = "The foe is hit with a hard punch fired like a meteor. It may also raise the user's Attack."; // Description here
            type = Type.Steel; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 10; // PP here
            power = 100; // Power here
            accuracy = 85; // Accuracy here

            // 20% chance of increasing user attack by one
            userStatChangeChances = new StatChangeChance[1]
            {
                new StatChangeChance()
                {
                    chance = 0.2F,
                    statChanges = new Stats<sbyte>()
                    {
                        attack = 1,
                        defense = 0,
                        specialAttack = 0,
                        specialDefense = 0,
                        speed = 0,
                        health = 0,
                    }
                }
            };

        }

    }
}
