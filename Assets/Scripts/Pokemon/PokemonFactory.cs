using System;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;
using Items;

namespace Pokemon
{
    public static class PokemonFactory
    {

        /// <summary>
        /// Generate a PokemonInstance specifying many property values
        /// </summary>
        /// <returns>The PokemonInstance created</returns>
        public static PokemonInstance GenerateFull(
            int speciesId,
            Nature nature,
            Stats<byte> effortValues,
            Stats<byte> individualValues,
            int[] _moves,
            int experience,
            PokemonInstance.NonVolatileStatusCondition nonVolatileStatusCondition,
            PokemonInstance.BattleProperties battleProperties,
            string nickname = "",
            Item heldItem = null,
            int _health = -1
            )
        {

            int[] moves = new int[0];

            if (_moves.Length <= 4)
            {
                moves = _moves;
            }
            else
            {
                Debug.LogWarning("Length of moves passed to GenerateFull was greater than 4");
                Array.Copy(_moves, moves, 4);
            }

            PokemonInstance instance = new PokemonInstance(individualValues)
            {
                speciesId = speciesId,
                nature = nature,
                effortValues = effortValues,
                moveIds = moves,
                experience = experience,
                nonVolatileStatusCondition = nonVolatileStatusCondition,
                battleProperties = battleProperties,
                nickname = nickname,
                heldItem = heldItem,
                health = _health > 0 ? _health : 1
            };

            if (_health <= 0)
                instance.RestoreFully();

            return instance;

        }

        /// <summary>
        /// Generate a PokemonInstance as if for in the wild
        /// </summary>
        /// <param name="possibleSpeciesIds">Array of species ids that the pokemon could be</param>
        /// <param name="minLevel">The minimum level for the PokemonInstance (inclusive)</param>
        /// <param name="maxLevel">The maximum level for the PokemonInstance (inclusive)</param>
        /// <returns>The PokemonInstance created</returns>
        public static PokemonInstance GenerateWild(
            int[] possibleSpeciesIds,
            byte minLevel,
            byte maxLevel
            )
        {

            int speciesId, experience;
            byte level;
            Nature nature;
            Stats<byte> effortValues, individualValues;
            int[] moves;

            speciesId = possibleSpeciesIds[UnityEngine.Random.Range(0, possibleSpeciesIds.Length)];

            level = (byte)UnityEngine.Random.Range(minLevel, maxLevel + 1);
            experience = GrowthTypeData.GetMinimumExperienceForLevel(level, PokemonSpecies.GetPokemonSpeciesById(speciesId).growthType);

            nature = Nature.GetRandomNature();

            effortValues = new Stats<byte>()
            {
                attack = 0,
                defense = 0,
                specialAttack = 0,
                specialDefense = 0,
                speed = 0
            };

            individualValues = new Stats<byte>()
            {
                attack = (byte)UnityEngine.Random.Range(0, 32),
                defense = (byte)UnityEngine.Random.Range(0, 32),
                specialAttack = (byte)UnityEngine.Random.Range(0, 32),
                specialDefense = (byte)UnityEngine.Random.Range(0, 32),
                speed = (byte)UnityEngine.Random.Range(0, 32)
            };

            //Set the moves learnt as the last 4 moves that it could have learnt
            Dictionary<byte, int> levelUpMoves = PokemonSpecies.GetPokemonSpeciesById(speciesId).levelUpMoves;

            moves = new int[4];
            int movesIndex = 0;

            for (byte i = level; i >= 0; i--)
            {

                if (!levelUpMoves.ContainsKey(i))
                    continue;

                moves[movesIndex] = levelUpMoves[i];
                movesIndex++;

            }

            return GenerateFull(
                speciesId: speciesId,
                nature: nature,
                effortValues: effortValues,
                individualValues: individualValues,
                _moves: moves,
                experience: experience,
                nonVolatileStatusCondition: PokemonInstance.NonVolatileStatusCondition.None,
                battleProperties: null
                );

        }

    }
}