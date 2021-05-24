using System;
using System.Collections.Generic;
using System.Linq;
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
            int natureId,
            Stats<byte> effortValues,
            Stats<byte> individualValues,
            int[] _moves,
            byte[] movePPs,
            int experience,
            PokemonInstance.NonVolatileStatusCondition nonVolatileStatusCondition,
            PokemonInstance.BattleProperties battleProperties,
            string nickname = "",
            Item heldItem = null,
            int _health = -1,
            bool? gender = true,
            Stats<int> currentStats = new Stats<int>(), //If all of these values are 0, they won't be used
            int pokeBallId = PokemonInstance.defaultPokeBallId, //The poke ball id INCLUDING the poke ball type id
            string originalTrainerName = "",
            long catchTime = 0
            )
        {

            int[] moves = new int[4];

            //By default, the moves will be unset
            for (int i = 0; i < moves.Length; i++)
                moves[i] = -1;

            if (_moves.Length > 4)
            {
                Debug.LogWarning("Length of moves passed to GenerateFull was greater than 4");
                Array.Copy(_moves, moves, 4);
            }

            Array.Copy(_moves, moves, _moves.Length);

            PokemonInstance instance = new PokemonInstance(individualValues)
            {
                speciesId = speciesId,
                natureId = natureId,
                effortValues = effortValues,
                moveIds = moves,
                movePPs = movePPs,
                experience = experience,
                nonVolatileStatusCondition = nonVolatileStatusCondition,
                battleProperties = battleProperties,
                nickname = nickname,
                heldItem = heldItem,
                health = _health > 0 ? _health : 1,
                gender = gender,
                pokeBallId = pokeBallId,
                originalTrainerName = originalTrainerName,
                catchTime = catchTime
            };

            #region Setting Current Stats

            bool needToSetCurrentStats = false;

            foreach (Stats<int>.Stat stat in (Stats<int>.Stat[])Enum.GetValues(typeof(Stats<int>.Stat)))
            {
                if (currentStats.GetStat(stat) != 0)
                {
                    needToSetCurrentStats = true;
                    break;
                }
            }

            if (needToSetCurrentStats)
                instance.SetCurrentStats(currentStats);

            #endregion

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
            int natureId;
            Stats<byte> effortValues;
            Stats<byte> individualValues;
            int[] moves;
            byte[] movePPs;
            bool? gender;

            speciesId = ChooseRandomSpeciesId(possibleSpeciesIds);
            PokemonSpecies species = PokemonSpecies.GetPokemonSpeciesById(speciesId);

            #region Stats

            level = ChooseLevelInRange(minLevel, maxLevel);
            experience = GrowthTypeData.GetMinimumExperienceForLevel(level, species.growthType);

            natureId = Nature.GetRandomNatureId();

            effortValues = new Stats<byte>()
            {
                attack = 0,
                defense = 0,
                specialAttack = 0,
                specialDefense = 0,
                speed = 0,
                health = 0
            };

            individualValues = GenerateRandomIVs();

            #endregion

            moves = ChooseMovesFromSpeciesAndLevel(speciesId, level, out movePPs);

            gender = ChooseRandomGender(speciesId);

            return GenerateFull(
                speciesId: speciesId,
                natureId: natureId,
                effortValues: effortValues,
                individualValues: individualValues,

                _moves: moves,
                movePPs: movePPs,
                experience: experience,
                nonVolatileStatusCondition: PokemonInstance.NonVolatileStatusCondition.None,
                battleProperties: null,
                gender: gender
                );

        }

        public static PokemonInstance GenerateFromBasicSpecification(PokemonInstance.BasicSpecification spec)
        {

            int speciesId, experience;
            int natureId;
            Stats<byte> effortValues;
            Stats<byte> individualValues;
            int[] moves;
            byte[] movePPs;
            bool? gender;

            speciesId = spec.speciesId;
            experience = spec.GetExperienceFromLevel();
            natureId = Nature.GetRandomNatureId();
            effortValues = spec.EVs;
            individualValues = spec.useRandomIVs ? GenerateRandomIVs() : spec.IVs;
            if (spec.useAutomaticMoves)
            {
                moves = ChooseMovesFromSpeciesAndLevel(speciesId, spec.level, out movePPs);
            }
            else
            {
                moves = spec.moveIds;
                movePPs = moves.Select(x => Moves.PokemonMove.GetPokemonMoveById(x).maxPP).ToArray();
            }
            gender = spec.GetGender();

            return GenerateFull(
                speciesId: speciesId,
                natureId: natureId,
                effortValues: effortValues,
                individualValues: individualValues,

                _moves: moves,
                movePPs: movePPs,
                experience: experience,
                nonVolatileStatusCondition: PokemonInstance.NonVolatileStatusCondition.None,
                battleProperties: null,
                gender: gender,

                pokeBallId: spec.pokeBallId > 0 ? spec.pokeBallId + Item.typeIdPokeBall : PokemonInstance.defaultPokeBallId,
                nickname: spec.nickname
                );

        }

        #region Random Attribute Choosing

        public static int ChooseRandomSpeciesId(int[] options)
            => options[UnityEngine.Random.Range(0, options.Length)];

        public static byte ChooseLevelInRange(byte minLevel, byte maxLevel)
            => (byte)UnityEngine.Random.Range(minLevel, maxLevel + 1);

        public static Stats<byte> GenerateRandomEVs()
            => new Stats<byte>()
            {
                attack = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumEffortValue + 1),
                defense = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumEffortValue + 1),
                specialAttack = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumEffortValue + 1),
                specialDefense = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumEffortValue + 1),
                speed = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumEffortValue + 1),
                health = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumEffortValue + 1)
            };

        public static Stats<byte> GenerateRandomIVs()
            => new Stats<byte>()
            {
                attack = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumIndividualValue + 1),
                defense = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumIndividualValue + 1),
                specialAttack = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumIndividualValue + 1),
                specialDefense = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumIndividualValue + 1),
                speed = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumIndividualValue + 1),
                health = (byte)UnityEngine.Random.Range(0, PokemonInstance.maximumIndividualValue + 1)
            };

        public static int[] ChooseMovesFromSpeciesAndLevel(int speciesId,
            byte level,
            out byte[] movePPs)
        {

            PokemonSpecies species = PokemonSpecies.GetPokemonSpeciesById(speciesId);

            //Set the moves learnt as the last 4 moves that it could have learnt
            Dictionary<byte, int[]> levelUpMoves = species.levelUpMoves;

            int[] moves = new int[4];
            //By default, the moves will be unset
            for (int moveIndex = 0; moveIndex < moves.Length; moveIndex++)
                moves[moveIndex] = -1;

            movePPs = new byte[4];
            int movesIndex = 0;
            bool allMovesSet = false;

            byte i = level;
            while (true)
            {

                if (levelUpMoves.ContainsKey(i))
                {

                    foreach (int moveId in levelUpMoves[i])
                    {

                        moves[movesIndex] = moveId;
                        movePPs[movesIndex] = Moves.PokemonMove.GetPokemonMoveById(moveId).maxPP;
                        movesIndex++;

                        if (movesIndex == 4)
                        {
                            allMovesSet = true;
                            break;
                        }

                    }

                }

                if (allMovesSet)
                    break;

                if (i <= 0)
                    break;

                i--;

            }

            //Add base moves if moves array isn't full yet
            if (!allMovesSet)
            {

                foreach (int moveId in species.baseMoves)
                {

                    moves[movesIndex] = moveId;
                    movePPs[movesIndex] = Moves.PokemonMove.GetPokemonMoveById(moveId).maxPP;
                    movesIndex++;

                    if (movesIndex == 4)
                    {
                        allMovesSet = true;
                        break;
                    }

                }

            }

            return moves;

        }

        public static bool? ChooseRandomGender(int speciesId)
            => PokemonSpecies.GetPokemonSpeciesById(speciesId).GetRandomWeightedGender();

        #endregion

    }
}
