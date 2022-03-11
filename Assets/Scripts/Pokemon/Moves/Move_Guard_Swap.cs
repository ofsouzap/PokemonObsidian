using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Guard_Swap : PokemonMove
    {

        public Move_Guard_Swap()
        {

            id = 385; // ID here
            name = "Guard Swap"; // Full name here
            description = "The user employs its psychic power to switch changes to its Defense and Sp. Def with the foe."; // Description here
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
                defense = Convert.ToSByte(target.battleProperties.statModifiers.defense - user.battleProperties.statModifiers.defense),
                specialDefense = Convert.ToSByte(target.battleProperties.statModifiers.specialDefense - user.battleProperties.statModifiers.specialDefense),
            };

            Stats<sbyte> targetChanges = new Stats<sbyte>()
            {
                defense = Convert.ToSByte(-userChanges.defense),
                specialDefense = Convert.ToSByte(-userChanges.specialDefense),
            };

            usageResults.userStatChanges = userChanges;
            usageResults.targetStatChanges = targetChanges;

            return usageResults;

        }

    }
}
