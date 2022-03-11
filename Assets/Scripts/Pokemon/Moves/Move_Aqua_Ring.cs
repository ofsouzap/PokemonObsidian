using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Aqua_Ring : PokemonMove
    {

        public Move_Aqua_Ring()
        {

            id = 392; // ID here
            name = "Aqua Ring"; // Full name here
            description = "The user envelops itself in a veil made of water. It regains some HP on every turn."; // Description here
            type = Type.Water; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 20; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override void DoAquaRingUpdate(ref UsageResults usageResults, PokemonInstance user, PokemonInstance target, BattleData battleData)
        {
            base.DoAquaRingUpdate(ref usageResults, user, target, battleData);
            if (user.battleProperties.volatileBattleStatus.aquaRing)
                usageResults.failed = true;
            else
                usageResults.setAquaRing = true;
        }

    }
}
