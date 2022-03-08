using Battle;
using Pokemon;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Endeavor : PokemonMove
    {

        public Move_Endeavor()
        {

            id = 283; // ID here
            name = "Endeavor"; // Full name here
            description = "An attack move that cuts down the foe's HP to equal the user's HP."; // Description here
            type = Type.Normal; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here

        }

        public override bool RunFailureCheck(BattleData battleData, PokemonInstance user, PokemonInstance target)
            => base.RunFailureCheck(battleData, user, target) || (user.health > target.health);

        public override int CalculateDamageToDeal(float attackDefenseRatio, float modifiersValue, PokemonInstance user, PokemonInstance target, BattleData battleData)
            => target.health - user.health; // Cut the target's health to equal the user's

    }
}
