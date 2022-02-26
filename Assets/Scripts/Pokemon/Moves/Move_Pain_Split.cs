using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Pain_Split : PokemonMove
    {

        public Move_Pain_Split()
        {

            id = 220; // ID here
            name = "Pain Split"; // Full name here
            description = "The user adds its HP to the foe's HP, then equally shares the combined HP with the foe."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        private int GetIntendedPokemonHealth(PokemonInstance user, PokemonInstance target)
            => Mathf.CeilToInt((user.health + target.health) / 2F);

        public override int CalculateUserHealthHealed(PokemonInstance user, PokemonInstance target, BattleData battleData, int targetDamageDealt = 0)
        {
            if (user.health < GetIntendedPokemonHealth(user, target))
                return GetIntendedPokemonHealth(user, target) - user.health;
            else
                return 0;
        }

        public override int CalculateUserRecoilDamage(PokemonInstance user, PokemonInstance target, BattleData battleData, int targetDamageDealt = 0)
        {
            if (user.health > GetIntendedPokemonHealth(user, target))
                return user.health - GetIntendedPokemonHealth(user, target);
            else
                return 0;
        }

        public override int CalculateDamageToDeal(float attackDefenseRatio, float modifiersValue, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            if (target.health > GetIntendedPokemonHealth(user, target))
                return target.health - GetIntendedPokemonHealth(user, target);
            else
                return 0;
        }

        public override int CalculateTargetHealthHealed(PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            if (target.health < GetIntendedPokemonHealth(user, target))
                return GetIntendedPokemonHealth(user, target) - target.health;
            else
                return 0;
        }

    }
}
