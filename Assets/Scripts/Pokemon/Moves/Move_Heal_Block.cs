using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Heal_Block : PokemonMove
    {

        public Move_Heal_Block()
        {

            id = 377; // ID here
            name = "Heal Block"; // Full name here
            description = "The user prevents the foe from using any HP-recovery moves for five turns."; // Description here
            type = Type.Psychic; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 15; // PP here
            power = 0; // Power here
            accuracy = 100; // Accuracy here
            
        }

        public override bool GetInflictsHealBlock(PokemonInstance user, PokemonInstance target, BattleData battleData)
            => true;

    }
}
