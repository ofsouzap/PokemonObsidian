using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Pokemon;

namespace Pokemon
{

    public static class PokemonSpeciesData
    {

        const string dataPath = "Data/pokemonSpecies";
        const bool ignoreDataFirstLine = true;

        public static readonly Regex validEvolutionsEntryRegex = new Regex(@"^\[([0-9]+:[0-9]*:[0-9]{0,3}?)(;([0-9]+:[0-9]*:[0-9]{0,3}?))*\]$");
        public static readonly Regex validLevelUpMovesEntryRegex = new Regex(@"^\[([0-9]+:[0-9]+)(;[0-9]+:[0-9]+)*\]$");
        public static readonly Regex validEVYieldRegex = new Regex(@"^[0-9]{0,3}(:[0-9]{0,3}){4}$");

        /* Data CSV Columns:
         * id
         * name
         * sprites name (a string) (if empty, use id (as string))
         * base attack
         * base defense
         * base special attack
         * base special defense
         * base speed
         * base health
         * type 1 (lowercase type name)
         * type 2 (lowercase type name or blank if none)
         * growth type (lowercase growth type name)
         * basic evolutions (format: "[evolution;evolution...]")
         *     evolution format: "targetSpeciesId:usedItemId:level"
         *     note that, if both used item and level conditions are used, they will both be required to evolve; not either
         *     eg. for bulbasaur: [2::16]
         *     eg. for eevee: [134:{waterStoneId}:;135:{thunderStoneId}:;136:{fireStoneId}:] ({xId} means id for item x. Yet to be set)
         * level-up move ids (format: "[level:moveId;level:moveId...]")
         *     eg. for bulbasuar: [0:{tackle};0:{growl};3:{vineWhip};6:{growth} etc. etc.]
         *         where {x} means id of move x which is yet to be set
         * disc move ids (move ids separated by ':')
         * egg move ids (move ids separated by ':')
         * tutor move ids (move ids separated by ':')
         * ev yield
         *     five values separated by ':' for attack, defense, specialAttack, specialDefense, speed
         *     can't be blank
         *     eg. bulbasaur 0:0:1:0:0
         * catch rate (byte)
         * base expereience yield (0 <= x <= 65,535)
         */

        public static void LoadData()
        {

            LoadPokemonSpeciesRegistry();

            SetPokemonSpeciesSpecialData();

        }

        private static void LoadPokemonSpeciesRegistry()
        {

            List<PokemonSpecies> species = new List<PokemonSpecies>();

            string[][] stringData = CSV.ReadCSVResource(dataPath, ignoreDataFirstLine);

            foreach (string[] entry in stringData)
            {

                string name, spritesName;
                int id;
                byte baseAttack, baseDefense, baseSpecialAttack, baseSpecialDefense, baseSpeed, baseHealth, catchRate;
                Type type1;
                Type? type2;
                GrowthType growthType;
                PokemonSpecies.Evolution[] evolutions;
                Dictionary<byte, int> levelUpMoves;
                int[] discMoves, eggMoves, tutorMoves;
                Stats<byte> evYield;
                ushort baseExperienceYield;

                if (entry.Length < 20)
                {
                    Debug.LogWarning("Invalid PokemonSpecies entry to load - " + entry);
                    continue;
                }

                #region id

                if (!int.TryParse(entry[0], out id))
                {
                    Debug.LogError("Invalid entry id found - " + entry[0]);
                    id = -1;
                }

                #endregion

                #region name

                name = entry[1];

                #endregion

                #region spritesName

                spritesName = entry[2] != "" ? entry[2] : id.ToString();

                #endregion

                #region baseStats

                bool baseAttackSuccess,
                    baseDefenseSuccess,
                    baseSpecialAttackSuccess,
                    baseSpecialDefenseSuccess,
                    baseSpeedSuccess,
                    baseHealthSuccess;

                baseAttackSuccess = byte.TryParse(entry[3], out baseAttack);
                baseDefenseSuccess = byte.TryParse(entry[4], out baseDefense);
                baseSpecialAttackSuccess = byte.TryParse(entry[5], out baseSpecialAttack);
                baseSpecialDefenseSuccess = byte.TryParse(entry[6], out baseSpecialDefense);
                baseSpeedSuccess = byte.TryParse(entry[7], out baseSpeed);
                baseHealthSuccess = byte.TryParse(entry[8], out baseHealth);

                if (!(baseAttackSuccess
                    && baseDefenseSuccess
                    && baseSpecialAttackSuccess
                    && baseSpecialDefenseSuccess
                    && baseSpeedSuccess
                    && baseHealthSuccess))
                {
                    Debug.LogError("Invalid base stats found for id " + id);
                    baseAttack = baseDefense = baseSpecialAttack = baseSpecialDefense = baseSpeed = baseHealth = 1;
                }

                #endregion

                #region types

                string type1String = entry[9];
                string type2String = entry[10];

                try
                {

                    type1 = TypeFunc.Parse(type1String);

                    if (type2String != "")
                        type2 = TypeFunc.Parse(type2String);
                    else
                        type2 = null;

                }
                catch (FormatException)
                {
                    Debug.LogError("Invalid type found for id - " + id);
                    type1 = Type.Normal;
                    type2 = null;
                }

                #endregion

                #region growthType

                try
                {
                    growthType = GrowthTypeFunc.Parse(entry[11]);
                }
                catch (FormatException)
                {
                    Debug.LogError("Unknown growth type found for id " + id);
                    growthType = GrowthType.Slow;
                }

                #endregion

                #region evolutions

                string evolutionsString = entry[12];

                if (evolutionsString == "" || evolutionsString == "[]")
                {
                    evolutions = new PokemonSpecies.Evolution[0];
                }
                else
                {

                    if (validEvolutionsEntryRegex.IsMatch(evolutionsString))
                    {

                        List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>();

                        string[] evolutionStringEntries = evolutionsString.Substring(1, evolutionsString.Length - 2).Split(';');

                        foreach (string evolutionStringEntry in evolutionStringEntries)
                        {

                            string[] entryParts = evolutionStringEntry.Split(':');
                            int targetId, usedItemId;
                            byte level;

                            bool targetIdSuccess,
                                usedItemIdSuccess,
                                levelSuccess;

                            targetIdSuccess = int.TryParse(entryParts[0], out targetId);
                            usedItemIdSuccess = int.TryParse(entryParts[1], out usedItemId);
                            levelSuccess = byte.TryParse(entryParts[2], out level);

                            if (targetIdSuccess && usedItemIdSuccess && levelSuccess)
                            {

                                evolutionsList.Add(new PokemonSpecies.Evolution()
                                {
                                    targetId = targetId,
                                    itemId = usedItemId,
                                    level = level
                                });

                            }
                            else
                            {
                                Debug.LogError("Invalid evolution entry at id " + id);
                            }

                        }

                        evolutions = evolutionsList.ToArray();

                    }
                    else
                    {
                        Debug.LogError("Invalid evolutions entry found for id " + id);
                        evolutions = new PokemonSpecies.Evolution[0];
                    }

                }

                #endregion

                #region moves

                #region levelUpMoves

                levelUpMoves = new Dictionary<byte, int>();

                string levelUpMovesString = entry[13];

                if (validLevelUpMovesEntryRegex.IsMatch(levelUpMovesString))
                {

                    string[] levelUpMovesEntries = levelUpMovesString.Split(';');

                    foreach (string levelUpMoveEntry in levelUpMovesEntries)
                    {

                        byte level;
                        int moveId;

                        string[] entryParts = levelUpMoveEntry.Split(':');

                        bool levelSuccess = byte.TryParse(entryParts[0], out level);
                        bool moveIdSuccess = int.TryParse(entryParts[1], out moveId);

                        if (!(levelSuccess && moveIdSuccess))
                        {
                            Debug.LogError("Invalid entry for level up move - " + levelUpMoveEntry + " (id " + id + ")");
                            level = 0;
                            moveId = 0;
                        }

                        levelUpMoves.Add(level, moveId);

                    }

                }
                else
                {
                    Debug.LogError("Invalid level up moves entry for id " + id);
                }

                #endregion

                try
                {
                    discMoves = entry[14].Split(':').Select((x) => int.Parse(x)).ToArray();
                    eggMoves = entry[15].Split(':').Select((x) => int.Parse(x)).ToArray();
                    tutorMoves = entry[16].Split(':').Select((x) => int.Parse(x)).ToArray();
                }
                catch (FormatException)
                {
                    Debug.LogError("Invalid move id in disc, egg or tutor moves for id " + id);
                    discMoves = eggMoves = tutorMoves = new int[0];
                }

                #endregion

                #region evYield

                string evYieldEntry = entry[17];

                if (validEVYieldRegex.IsMatch(evYieldEntry))
                {

                    string[] parts = evYieldEntry.Split(':');

                    byte yieldAttack,
                        yieldDefense,
                        yieldSpecialAttack,
                        yieldSpecialDefense,
                        yieldSpeed;

                    bool yieldAttackSuccess,
                        yieldDefenseSuccess,
                        yieldSpecialAttackSuccess,
                        yieldSpecialDefenseSuccess,
                        yieldSpeedSuccess;

                    yieldAttackSuccess = byte.TryParse(parts[0], out yieldAttack);
                    yieldDefenseSuccess = byte.TryParse(parts[1], out yieldDefense);
                    yieldSpecialAttackSuccess = byte.TryParse(parts[2], out yieldSpecialAttack);
                    yieldSpecialDefenseSuccess = byte.TryParse(parts[3], out yieldSpecialDefense);
                    yieldSpeedSuccess = byte.TryParse(parts[4], out yieldSpeed);

                    if (yieldAttackSuccess
                        && yieldDefenseSuccess
                        && yieldSpecialAttackSuccess
                        && yieldSpecialDefenseSuccess
                        && yieldSpeedSuccess)
                    {

                        evYield = new Stats<byte>()
                        {
                            attack = yieldAttack,
                            defense = yieldDefense,
                            specialAttack = yieldSpecialAttack,
                            specialDefense = yieldSpecialDefense,
                            speed = yieldSpeed
                        };

                    }
                    else
                    {
                        Debug.LogError("Invalid EV yield entry value for id " + id);
                        evYield = new Stats<byte>();
                    }

                }
                else
                {
                    Debug.LogError("Invalid EV yield format for id " + id);
                    evYield = new Stats<byte>();
                }

                #endregion

                #region catchRate

                if (!byte.TryParse(entry[18], out catchRate))
                {
                    Debug.LogError("Invalid catch rate entry for id " + id);
                    catchRate = 127;
                }

                #endregion

                #region baseExperienceYield

                if (!ushort.TryParse(entry[19], out baseExperienceYield))
                {
                    Debug.LogError("Invalid base experience yield entry for id " + id);
                    baseExperienceYield = 0;
                }

                #endregion

                species.Add(new PokemonSpecies()
                {
                    name = name,
                    resourceName = spritesName,
                    id = id,
                    baseStats = new Stats<byte>()
                    {
                        attack = baseAttack,
                        defense = baseDefense,
                        specialAttack = baseSpecialAttack,
                        specialDefense = baseSpecialDefense,
                        speed = baseSpeed,
                        health = baseHealth
                    },
                    type1 = type1,
                    type2 = type2,
                    growthType = growthType,
                    evolutions = evolutions,

                    levelUpMoves = levelUpMoves,
                    discMoves = discMoves,
                    eggMoves = eggMoves,
                    tutorMoves = tutorMoves,

                    evYield = evYield,
                    catchRate = catchRate,
                    baseExperienceYield = baseExperienceYield

                });

            }

            PokemonSpecies.registry.SetValues(species.ToArray());

        }

        private static void SetPokemonSpeciesSpecialData()
        {

            //TODO - set special evolutions (eg. with trades)
            //TODO - set anything special about certain species

        }

    }

}