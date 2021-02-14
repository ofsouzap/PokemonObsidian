using System;
using System.Collections.Generic;
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
         * level-up move ids
         *     TODO - design
         * tm move ids (move ids separated by ':')
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

                if (entry.Length < 13)
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
                    evolutions = evolutions
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