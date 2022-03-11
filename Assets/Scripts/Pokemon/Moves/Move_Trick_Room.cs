using Battle;
using Pokemon;
using System;
using UnityEngine;

namespace Pokemon.Moves
{
    public class Move_Trick_Room : PokemonMove
    {

        public Move_Trick_Room()
        {

            id = 433; // ID here
            name = "Trick Room"; // Full name here
            description = "The user creates a bizarre area in which slower Pokémon get to move first for five turns."; // Description here
            type = Type.Psychic; // Type here
            moveType = MoveType.Status; // Move type here
            maxPP = 5; // PP here
            power = 0; // Power here
            accuracy = 0; // Accuracy here

        }

        public override UsageResults CalculateEffect(PokemonInstance user, PokemonInstance target, BattleData battleData, bool allowMissing = true)
        {

            UsageResults usageResults = base.CalculateEffect(user, target, battleData, allowMissing);

            if (usageResults.Succeeded)
            {
                if (battleData.stageModifiers.trickRoomRemainingTurns <= 0)
                    usageResults.setTrickRoomDuration = BattleData.StageModifiers.GetRandomTrickRoomDuration(battleData);
                else
                    usageResults.failed = true; //If trick room already active, fail
            }

            return usageResults;

        }

    }
}
