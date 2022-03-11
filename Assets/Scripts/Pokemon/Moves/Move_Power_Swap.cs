using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Power_Swap : PokemonMove
    {

        public Move_Power_Swap()
        {

            id = 384; // ID here
            name = "Power Swap"; // Full name here
            description = "The user employs its psychic power to switch changes to its Attack and Sp. Atk with the foe."; // Description here
            type = Type.Psychic; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 10; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override UsageResults CalculateStatChanges(UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            Stats<sbyte> userChanges = new Stats<sbyte>()
            {
                attack = Convert.ToSByte(target.battleProperties.statModifiers.attack - user.battleProperties.statModifiers.attack),
                specialAttack = Convert.ToSByte(target.battleProperties.statModifiers.specialAttack - user.battleProperties.statModifiers.specialAttack),
            };

            Stats<sbyte> targetChanges = new Stats<sbyte>()
            {
                attack = Convert.ToSByte(-userChanges.attack),
                specialAttack = Convert.ToSByte(-userChanges.specialAttack),
            };

            usageResults.userStatChanges = userChanges;
            usageResults.targetStatChanges = targetChanges;

            return usageResults;

        }

    }
}
