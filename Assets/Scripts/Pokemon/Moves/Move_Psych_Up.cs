using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Psych_Up : PokemonMove
    {

        public Move_Psych_Up()
        {

            id = 244; // ID here
            name = "Psych Up"; // Full name here
            description = "The user hypnotizes itself into copying any stat change made by the foe."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 10; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override UsageResults CalculateStatChanges(UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            usageResults = base.CalculateStatChanges(usageResults, user, target, battleData);

            Stats<sbyte> userSMs = user.battleProperties.statModifiers;
            sbyte userEvasionM = user.battleProperties.evasionModifier;
            sbyte userAccuracyM = user.battleProperties.accuracyModifier;

            Stats<sbyte> targetSMs = target.battleProperties.statModifiers;
            sbyte targetEvasionM = target.battleProperties.evasionModifier;
            sbyte targetAccuracyM = target.battleProperties.accuracyModifier;

            usageResults.userStatChanges = new Stats<sbyte>()
            {
                attack = (sbyte)(targetSMs.attack - userSMs.attack),
                defense = (sbyte)(targetSMs.defense - userSMs.defense),
                specialAttack = (sbyte)(targetSMs.specialAttack - userSMs.specialAttack),
                specialDefense = (sbyte)(targetSMs.specialDefense - userSMs.specialDefense),
                speed = (sbyte)(targetSMs.speed - userSMs.speed),
                health = (sbyte)(targetSMs.health - userSMs.health)
            };

            usageResults.userEvasionChange = (sbyte)(targetEvasionModifier - userEvasionM);
            usageResults.userAccuracyChange = (sbyte)(targetAccuracyM - userAccuracyM);

            return usageResults;

        }

    }
}
