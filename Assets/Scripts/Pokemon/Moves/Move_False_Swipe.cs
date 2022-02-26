using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_False_Swipe : PokemonMove
    {

        public Move_False_Swipe()
        {

            id = 206; // ID here
            name = "False Swipe"; // Full name here
            description = "A restrained attack that prevents the foe from fainting. The target is left with at least 1 HP."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 40; // PP here
            power = 40; // Power here
            accuracy = 100; // Accuracy here

        }

        public override int CalculateDamageToDeal(float attackDefenseRatio, float modifiersValue, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {

            int baseDamage = base.CalculateDamageToDeal(attackDefenseRatio, modifiersValue, user, target, battleData);

            if (baseDamage >= target.health)
                return target.health - 1;
            else
                return baseDamage;

        }

    }
}
