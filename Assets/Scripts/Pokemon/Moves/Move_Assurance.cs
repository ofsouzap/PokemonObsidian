using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Assurance : PokemonMove
    {

        public Move_Assurance()
        {

            id = 372; // ID here
            name = "Assurance"; // Full name here
            description = "If the foe has already taken some damage in the same turn, this attack's power is doubled."; // Description here
            type = Type.Dark; // Type here
            moveType = MoveType.Physical; // Move type here
            maxPP = 10; // PP here
            power = 50; // Power here
            accuracy = 100; // Accuracy here

        }

        public override int CalculateDamageToDeal(float attackDefenseRatio, float modifiersValue, PokemonInstance user, PokemonInstance target, BattleData battleData)
            => base.CalculateDamageToDeal(attackDefenseRatio, modifiersValue, user, target, battleData) * (target.battleProperties.GetDamageThisTurn() > 0 ? 2 : 1);

    }
}
