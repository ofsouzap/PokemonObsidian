using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Curse : PokemonMove
    {

        public Move_Curse()
        {

            id = 174; // ID here
            name = "Curse"; // Full name here
            description = "A move that works differently for the Ghost type than for all the other types."; // Description here
            type = Type.Ghost; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 10; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

            // Only used if user isn't ghost type
            userStatChanges = new Stats<sbyte>()
            {
                attack = 1,
                defense = 1,
                specialAttack = 0,
                specialDefense = 0,
                speed = -1,
                health = 0
            };

        }

        public override bool GetInflictsCurse(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => user.HasType(Type.Ghost);

        public override UsageResults CalculateStatChanges(UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            if (user.HasType(Type.Ghost))
                return usageResults; // No stat changes if user a ghost type
            else
                return base.CalculateStatChanges(usageResults, user, target, battleData); // Use default stat changes if user not a ghost tye

        }

        public override int CalculateUserRecoilDamage(PokemonInstance user, PokemonInstance target, BattleData battleData, int targetDamageDealt = 0)
        {

            if (user.HasType(Type.Ghost))
                return user.GetStats().health / 2; // Half of user's health is removed if user is ghost type
            else
                return base.CalculateUserRecoilDamage(user, target, battleData, targetDamageDealt); // Use default recoil damage if user not a ghost type

        }

    }
}
