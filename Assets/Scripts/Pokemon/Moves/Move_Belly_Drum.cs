using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Belly_Drum : PokemonMove
    {

        public Move_Belly_Drum()
        {

            id = 187; // ID here
            name = "Belly Drum"; // Full name here
            description = "The user maximizes its Attack stat in exchange for HP equal to half its max HP."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 10; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

            maxHealthRelativeRecoilDamage = 0.5F; // Move takess half of user's maximum health

        }

        public override UsageResults CalculateStatChanges(UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            usageResults = base.CalculateStatChanges(usageResults, user, target, battleData);

            usageResults.userStatChanges = new Stats<sbyte>()
            {

                // This move sets the user's attack stat to the maximum value (6)
                attack = (sbyte)(PokemonInstance.BattleProperties.maximumStatModifier - user.battleProperties.statModifiers.attack),

                defense = usageResults.userStatChanges.defense,
                specialAttack = usageResults.userStatChanges.specialAttack,
                specialDefense = usageResults.userStatChanges.specialDefense,
                speed = usageResults.userStatChanges.speed,
                health = usageResults.userStatChanges.health

            };

            return usageResults;

        }

    }
}
