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
                byte baseAttack, baseDefense, baseSpecialAttack, baseSpecialDefense, baseSpeed, baseHealth;
                Type type1;
                Type? type2;
                GrowthType growthType;
                PokemonSpecies.Evolution[] evolutions;
                Dictionary<byte, int> levelUpMoves;
                int[] discMoves, eggMoves, tutorMoves;

                if (entry.Length < 17)
                {
                    Debug.LogWarning("Invalid PokemonSpecies entry to load - " + entry);
                    continue;
                }

                #region id

                try
                {
                    id = int.Parse(entry[0]);
                }
                catch (ArgumentException)
                {
                    Debug.LogError("Invalid entry id found - " + entry[0]);
                    id = -1;
                }

                #endregion

                #region name

                name = entry[1];

                #endregion

                #region spritesName

                spritesName = entry[2] == "" ? entry[2] : id.ToString();

                #endregion

                #region baseStats

                try
                {
                    baseAttack = byte.Parse(entry[3]);
                    baseDefense = byte.Parse(entry[4]);
                    baseSpecialAttack = byte.Parse(entry[5]);
                    baseSpecialDefense = byte.Parse(entry[6]);
                    baseSpeed = byte.Parse(entry[7]);
                    baseHealth = byte.Parse(entry[8]);
                }
                catch (ArgumentException)
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
                catch (ArgumentException)
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
                catch (ArgumentException)
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

                            try
                            {
                                targetId = int.Parse(entryParts[0]);
                                usedItemId = int.Parse(entryParts[1]);
                                level = byte.Parse(entryParts[2]);
                            }
                            catch (ArgumentException)
                            {
                                targetId = usedItemId = 0;
                                level = 0;
                                Debug.LogError("Invalid evolution entry as id " + id);
                            }

                            evolutionsList.Add(new PokemonSpecies.Evolution()
                            {
                                targetId = targetId,
                                itemId = usedItemId,
                                level = level
                            });

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

                        try
                        {
                            level = byte.Parse(entryParts[0]);
                            moveId = int.Parse(entryParts[1]);
                        }
                        catch (ArgumentException)
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
                catch (ArgumentException)
                {
                    Debug.LogError("Invalid move id in disc, egg or tutor moves for id " + id);
                    discMoves = eggMoves = tutorMoves = new int[0];
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
                    tutorMoves = tutorMoves

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