using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Charge_Beam : PokemonMove
    {

        public Move_Charge_Beam()
        {

            id = 451; // ID here
            name = "Charge Beam"; // Full name here
            description = "The user fires a concentrated bundle of electricity. It may also raise the user's Sp. Atk stat."; // Description here
            type = Type.Electric; // Type here
            moveType = MoveType.Special; // Move type here
            maxPP = 10; // PP here
            power = 50; // Power here
            accuracy = 90; // Accuracy here

            userStatChangeChances = new StatChangeChance[1]
            {
                new StatChangeChance()
                {
                    chance = 0.7F,
                    statChanges = new Stats<sbyte>()
                    {
                        attack = 0,
                        defense = 0,
                        specialAttack = 1,
                        specialDefense = 0,
                        speed = 0
                    }
                }
            };

        }

    }
}
