using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Heart_Swap : PokemonMove
    {

        public Move_Heart_Swap()
        {

            id = 391; // ID here
            name = "Heart Swap"; // Full name here
            description = "The user employs its psychic power to switch stat changes with the foe."; // Description here
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
                defense = Convert.ToSByte(target.battleProperties.statModifiers.defense - user.battleProperties.statModifiers.defense),
                specialAttack = Convert.ToSByte(target.battleProperties.statModifiers.specialAttack - user.battleProperties.statModifiers.specialAttack),
                specialDefense = Convert.ToSByte(target.battleProperties.statModifiers.specialDefense - user.battleProperties.statModifiers.specialDefense),
                speed = Convert.ToSByte(target.battleProperties.statModifiers.speed - user.battleProperties.statModifiers.speed),
            };

            sbyte userEvasionChange = Convert.ToSByte(target.battleProperties.evasionModifier - user.battleProperties.evasionModifier);
            sbyte userAccuracyChange = Convert.ToSByte(target.battleProperties.accuracyModifier - user.battleProperties.accuracyModifier);

            Stats<sbyte> targetChanges = new Stats<sbyte>()
            {
                attack = Convert.ToSByte(-userChanges.attack),
                defense = Convert.ToSByte(-userChanges.defense),
                specialAttack = Convert.ToSByte(-userChanges.specialAttack),
                specialDefense = Convert.ToSByte(-userChanges.specialDefense),
                speed = Convert.ToSByte(-userChanges.speed),
            };

            sbyte targetEvasionChange = Convert.ToSByte(-userEvasionChange);
            sbyte targetAccuracyChange = Convert.ToSByte(-userAccuracyChange);

            usageResults.userStatChanges = userChanges;
            usageResults.userEvasionChange = userEvasionChange;
            usageResults.userAccuracyChange = userAccuracyChange;

            usageResults.targetStatChanges = targetChanges;
            usageResults.targetEvasionChange = targetEvasionChange;
            usageResults.targetAccuracyChange = targetAccuracyChange;

            return usageResults;

        }

    }
}
