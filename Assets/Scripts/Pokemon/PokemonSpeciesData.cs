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

        public static readonly Regex validEvolutionsEntryRegex = new Regex(@"^([0-9]*;[0-9]*;[0-9]{0,3}?)(,([0-9]*;[0-9]*;[0-9]{0,3}?))*$");
        public static readonly Regex validLevelUpMovesEntryRegex = new Regex(@"^([0-9]+;[0-9]+)(,[0-9]+;[0-9]+)*$");
        public static readonly Regex validEVYieldRegex = new Regex(@"^[0-9]{0,3}(;[0-9]{0,3}){5}$");

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
         * basic evolutions (format: "evolution,evolution...")
         *     evolution format: "targetSpeciesId;usedItemId;level"
         *     note that, if both used item and level conditions are used, they will both be required to evolve; not either
         *     eg. for bulbasaur: 2;;16
         *     eg. for eevee: 134;{waterStoneId};,135;{thunderStoneId};,136;{fireStoneId}; ({xId} means id for item x. Yet to be set)
         * base moves (move ids separated by ';')
         * level-up move ids (format: "level;moveId,level;moveId...")
         *     eg. for bulbasuar: 0;{tackle},0;{growl},3;{vineWhip},6;{growth} etc. etc.
         *         where {x} means id of move x which is yet to be set
         * disc move ids (move ids separated by ';')
         * egg move ids (move ids separated by ';')
         * tutor move ids (move ids separated by ';')
         * ev yield
         *     six values separated by ';' for attack, defense, specialAttack, specialDefense, speed, health
         *     can't be blank
         *     eg. bulbasaur 0;0;1;0;0;0
         * catch rate (byte)
         * base expereience yield (0 <= x <= 65,535)
         * relative proportion of gender male (0 <= x <= 255)
         * relative proportion of gender female (0 <= x <= 255)
         * relative proportion of gender genderless (0 <= x <= 255)
         * base friendship (0 <= x <= 255)
         * egg group 1
         *     blank will be taken as none
         *     "undiscovered" will also be taken as none
         *     options:
         *         monster
         *         human-like
         *         water 1
         *         water 2
         *         water 3
         *         bug
         *         mineral
         *         flying
         *         amorphous
         *         field
         *         fairy
         *         grass
         *         dragon
         * egg group 2 (as with egg group 1)
         *     if egg group 1 is none, this shouldn't be set
         * egg cycles (number of cycles needed to hatch an egg)
         * weight (float)
         * height (float)
         */

        public static void LoadData()
        {

            LoadPokemonSpeciesRegistry();

        }

        private static void LoadPokemonSpeciesRegistry()
        {

            List<PokemonSpecies> species = new List<PokemonSpecies>();

            string[][] stringData = CSV.ReadCSVResource(dataPath, ignoreDataFirstLine);

            foreach (string[] entry in stringData)
            {

                string name, spritesName;
                int id;
                byte baseAttack, baseDefense, baseSpecialAttack, baseSpecialDefense, baseSpeed, baseHealth, catchRate,
                    maleRelativeGenderProportion, femaleRelativeGenderProportion, genderlessRelativeGenderProportion,
                    baseFriendship, eggCycles;
                Type type1;
                Type? type2;
                GrowthType growthType;
                PokemonSpecies.Evolution[] evolutions;
                int[] baseMoves, discMoves, eggMoves, tutorMoves;
                Dictionary<byte, int[]> levelUpMoves;
                Stats<byte> evYield;
                ushort baseExperienceYield;
                EggGroup? eggGroup1, eggGroup2;
                float height, weight;

                if (entry.Length < 30)
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

                if (evolutionsString == "")
                {
                    evolutions = new PokemonSpecies.Evolution[0];
                }
                else
                {

                    if (validEvolutionsEntryRegex.IsMatch(evolutionsString))
                    {

                        List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>();

                        string[] evolutionStringEntries = evolutionsString.Split(',');

                        foreach (string evolutionStringEntry in evolutionStringEntries)
                        {

                            string[] entryParts = evolutionStringEntry.Split(';');
                            int targetId, usedItemId;
                            byte level;
                            bool usingItemId, usingLevel;

                            bool targetIdSuccess,
                                usedItemIdSuccess,
                                levelSuccess;

                            targetIdSuccess = int.TryParse(entryParts[0], out targetId);

                            if (entryParts[1] != "")
                            {
                                usedItemIdSuccess = int.TryParse(entryParts[1], out usedItemId);
                                usingItemId = true;
                            }
                            else
                            {
                                usedItemIdSuccess = true;
                                usedItemId = 0;
                                usingItemId = false;
                            }

                            if (entryParts[2] != "")
                            {
                                levelSuccess = byte.TryParse(entryParts[2], out level);
                                usingLevel = true;
                            }
                            else
                            {
                                levelSuccess = true;
                                level = 0;
                                usingLevel = false;
                            }

                            if (!usingItemId && !usingLevel)
                            {
                                Debug.LogError("Neither level nor item id used for evolution for id " + id);
                            }
                            else if (targetIdSuccess && usedItemIdSuccess && levelSuccess)
                            {

                                evolutionsList.Add(new PokemonSpecies.Evolution()
                                {
                                    targetId = targetId,
                                    useItemId = usingItemId ? usedItemId : (int?)null,
                                    level = usingLevel ? level : (byte?)null
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

                baseMoves = ParseMovesArrayValue(entry[13], "base", id.ToString());

                #region levelUpMoves

                Dictionary<byte, List<int>> levelUpMovesListDictionary = new Dictionary<byte, List<int>>();

                string levelUpMovesString = entry[14];

                if (levelUpMovesString != "")
                {

                    if (validLevelUpMovesEntryRegex.IsMatch(levelUpMovesString))
                    {

                        string[] levelUpMovesEntries = levelUpMovesString.Split(',');

                        foreach (string levelUpMoveEntry in levelUpMovesEntries)
                        {

                            byte level;
                            int moveId;

                            string[] entryParts = levelUpMoveEntry.Split(';');

                            bool levelSuccess = byte.TryParse(entryParts[0], out level);
                            bool moveIdSuccess = int.TryParse(entryParts[1], out moveId);

                            if (!(levelSuccess && moveIdSuccess))
                            {
                                Debug.LogError("Invalid entry for level up move - " + levelUpMoveEntry + " (id " + id + ")");
                                level = 0;
                                moveId = 0;
                            }

                            if (levelUpMovesListDictionary.ContainsKey(level))
                            {
                                levelUpMovesListDictionary[level].Add(moveId);
                            }
                            else
                            {
                                levelUpMovesListDictionary.Add(level, new List<int>() { moveId });
                            }

                        }

                    }
                    else
                    {
                        Debug.LogError("Invalid level up moves entry for id " + id);
                    }

                    levelUpMoves = new Dictionary<byte, int[]>();
                    foreach (byte key in levelUpMovesListDictionary.Keys)
                    {
                        levelUpMoves.Add(key, levelUpMovesListDictionary[key].ToArray());
                    }

                }
                else
                {
                    levelUpMoves = new Dictionary<byte, int[]>();
                }

                #endregion

                discMoves = ParseMovesArrayValue(entry[15], "disc", id.ToString());
                eggMoves = ParseMovesArrayValue(entry[16], "egg", id.ToString());
                tutorMoves = ParseMovesArrayValue(entry[17], "tutor", id.ToString());

                #endregion

                #region evYield

                string evYieldEntry = entry[18];

                if (validEVYieldRegex.IsMatch(evYieldEntry))
                {

                    string[] parts = evYieldEntry.Split(';');

                    byte yieldAttack,
                        yieldDefense,
                        yieldSpecialAttack,
                        yieldSpecialDefense,
                        yieldSpeed,
                        yieldHealth;

                    bool yieldAttackSuccess,
                        yieldDefenseSuccess,
                        yieldSpecialAttackSuccess,
                        yieldSpecialDefenseSuccess,
                        yieldSpeedSuccess,
                        yieldHealthSuccess;

                    yieldAttackSuccess = byte.TryParse(parts[0], out yieldAttack);
                    yieldDefenseSuccess = byte.TryParse(parts[1], out yieldDefense);
                    yieldSpecialAttackSuccess = byte.TryParse(parts[2], out yieldSpecialAttack);
                    yieldSpecialDefenseSuccess = byte.TryParse(parts[3], out yieldSpecialDefense);
                    yieldSpeedSuccess = byte.TryParse(parts[4], out yieldSpeed);
                    yieldHealthSuccess = byte.TryParse(parts[5], out yieldHealth);

                    if (yieldAttackSuccess
                        && yieldDefenseSuccess
                        && yieldSpecialAttackSuccess
                        && yieldSpecialDefenseSuccess
                        && yieldSpeedSuccess
                        && yieldHealthSuccess)
                    {

                        evYield = new Stats<byte>()
                        {
                            attack = yieldAttack,
                            defense = yieldDefense,
                            specialAttack = yieldSpecialAttack,
                            specialDefense = yieldSpecialDefense,
                            speed = yieldSpeed,
                            health = yieldHealth
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

                if (!byte.TryParse(entry[19], out catchRate))
                {
                    Debug.LogError("Invalid catch rate entry for id " + id);
                    catchRate = 127;
                }

                #endregion

                #region baseExperienceYield

                if (!ushort.TryParse(entry[20], out baseExperienceYield))
                {
                    Debug.LogError("Invalid base experience yield entry for id " + id);
                    baseExperienceYield = 0;
                }

                #endregion

                #region relativeGenderProportions

                if (entry[21] == "")
                {
                    maleRelativeGenderProportion = 0;
                }
                else
                {
                    if (!byte.TryParse(entry[21], out maleRelativeGenderProportion))
                    {
                        Debug.LogError("Invalid male relative gender proportion for id " + id);
                        maleRelativeGenderProportion = 1;
                    }
                }

                if (entry[22] == "")
                {
                    femaleRelativeGenderProportion = 0;
                }
                else
                {
                    if (!byte.TryParse(entry[22], out femaleRelativeGenderProportion))
                    {
                        Debug.LogError("Invalid female relative gender proportion for id " + id);
                        femaleRelativeGenderProportion = 1;
                    }
                }

                if (entry[23] == "")
                {
                    genderlessRelativeGenderProportion = 0;
                }
                else
                {
                    if (!byte.TryParse(entry[23], out genderlessRelativeGenderProportion))
                    {
                        Debug.LogError("Invalid genderless relative gender proportion for id " + id);
                        genderlessRelativeGenderProportion = 1;
                    }
                }

                if (maleRelativeGenderProportion == femaleRelativeGenderProportion
                    && femaleRelativeGenderProportion == genderlessRelativeGenderProportion
                    && genderlessRelativeGenderProportion == 0)
                {
                    Debug.LogError("No gender proportions set for id " + id);
                    maleRelativeGenderProportion = 1;
                    femaleRelativeGenderProportion = 1;
                }

                #endregion

                #region baseFriendship

                if (!byte.TryParse(entry[24], out baseFriendship))
                {
                    Debug.LogError("Invalid base friendship entry for id " + id);
                    baseFriendship = 0;
                }

                #endregion

                #region eggGroups

                string eggGroup1Entry = entry[25];
                string eggGroup2Entry = entry[26];

                if (eggGroup1Entry == "" || eggGroup1Entry == "undiscovered")
                {
                    eggGroup1 = null;
                }
                else
                {

                    try
                    {
                        eggGroup1 = EggGroupFunc.Parse(eggGroup1Entry);
                    }
                    catch (FormatException)
                    {
                        Debug.LogError("Unknown egg group 1 found for id " + id);
                        eggGroup1 = null;
                    }

                }

                if (eggGroup2Entry == "" || eggGroup2Entry == "undiscovered")
                {
                    eggGroup2 = null;
                }
                else
                {

                    try
                    {
                        eggGroup2 = EggGroupFunc.Parse(eggGroup2Entry);
                    }
                    catch (FormatException)
                    {
                        Debug.LogError("Unknown egg group 2 found for id " + id);
                        eggGroup2 = null;
                    }

                }

                #endregion

                #region eggCycles

                if (!byte.TryParse(entry[27], out eggCycles))
                {
                    Debug.LogError("Invalid egg cycles entry for id " + id);
                    eggCycles = 0;
                }

                #endregion

                #region height

                if (!float.TryParse(entry[28], out height))
                {
                    Debug.LogError("Invalid height entry for id " + id);
                    height = 0;
                }

                #endregion

                #region weight

                if (!float.TryParse(entry[29], out weight))
                {
                    Debug.LogError("Invalid weight entry for id " + id);
                    weight = 0;
                }

                #endregion

                #region Special Pokemon Data

                //Setting special data about the pokemon that can't be stored in the data files

                #region Evolutions

                /* Template

                //SPECIES1 evolves to SPECIES1 through METHOD
                if (id == ID)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = TRGT,
                        level = LVL,
                        requireTrade = REQT,
                        heldItemId = ITEM,
                        useItemId = ITEM,
                        requireFriendship = REQF,
                        condition = (pmon) => COND
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                */

                #region Gen I

                //Kadabra evolves to Alakazam thorugh trade
                if (id == 64)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 65,
                        requireTrade = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Machoke evolves to Machamp through trade
                if (id == 67)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 68,
                        requireTrade = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Graveler evolves to Golem through trade
                if (id == 75)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 76,
                        requireTrade = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Haunter evolves to Gengar through trade
                if (id == 93)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 94,
                        requireTrade = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Golbat evolves to Crobat through friendship
                if (id == 42)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 169,
                        requireFriendship = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                #endregion

                #region Gen II

                //Pichu evolves to Pikachu through friendship
                if (id == 172)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 25,
                        requireFriendship = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Cleffa evolves to Clefairy through friendship
                if (id == 173)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 35,
                        requireFriendship = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Igglybuff evolves to Jigglypuff through friendship
                if (id == 174)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 39,
                        requireFriendship = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Togepi evolves to Togetic through friendship
                if (id == 175)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 176,
                        requireFriendship = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Poliwhirl evolves to Politoed through trade whilst holding a king's rock
                if (id == 61)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 186,
                        requireTrade = true,
                        heldItemId = 221
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Eevee evolves to Espeon through friendship in daytime
                if (id == 133)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 196,
                        requireFriendship = true,
                        condition = _ => Daytime.IsDaytime
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Eevee evolves to Umbreon through friendship in daytime
                if (id == 133)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 197,
                        requireFriendship = true,
                        condition = _ => !Daytime.IsDaytime
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Slowpoke evolves to Slowking through trade whilst holding a king's rock
                if (id == 79)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 199,
                        requireTrade = true,
                        heldItemId = 221
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Onix evolves to Steelix through trade whilst holding a metal coat
                if (id == 95)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 208,
                        requireTrade = true,
                        heldItemId = 233
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Scyther evolves to Scizor through trade whilst holding a metal coat
                if (id == 123)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 212,
                        requireTrade = true,
                        heldItemId = 233
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Seadra evolves to Kingdra through trade whilst holding a dragon scale
                if (id == 117)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 230,
                        requireTrade = true,
                        heldItemId = 235
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Porygon evolves to Porygon2 through trade whilst holding a Up-Grade
                if (id == 137)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 223,
                        requireTrade = true,
                        heldItemId = 252
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Tyrogue evolves to Hitmonlee through trade at level 20 when its attack stat exeeds its defense stat
                if (id == 236)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 106,
                        level = 20,
                        condition = pmon => pmon.GetStats().attack > pmon.GetStats().defense
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Tyrogue evolves to Hitmonchan through trade at level 20 when its attack stat is lesser than its defense stat
                if (id == 236)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 107,
                        level = 20,
                        condition = pmon => pmon.GetStats().attack < pmon.GetStats().defense
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Tyrogue evolves to Hitmontop through trade at level 20 when its attack stat equals its defense stat
                if (id == 236)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 237,
                        level = 20,
                        condition = pmon => pmon.GetStats().attack == pmon.GetStats().defense
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Chansey evolves to Blissey through friendship
                if (id == 113)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 242,
                        requireFriendship = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                #endregion

                #region Gen III

                //Wurmple evolves to Silcoon at level 7 with guid first byte ending with 0
                if (id == 265)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 266,
                        level = 7,
                        condition = (pmon) => pmon.guid.ToByteArray()[0] % 2 == 0
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Wurmple evolves to Cascoon at level 7 with guid first byte ending with 1
                if (id == 265)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 268,
                        level = 7,
                        condition = (pmon) => pmon.guid.ToByteArray()[0] % 2 == 1
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Azurill evolves to Marill through friendship
                if (id == 298)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 183,
                        requireFriendship = true
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Feebas evolves to Milotic through trade
                if (id == 349)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 350,
                        requireTrade = true
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Clamperl evolves to Huntail through trade whilst holding deep sea tooth
                if (id == 366)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 367,
                        requireTrade = true,
                        heldItemId = 226
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Clamperl evolves to Gorebyss through trade whilst holding deep sea scale
                if (id == 366)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 368,
                        requireTrade = true,
                        heldItemId = 227
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                #endregion

                #region Gen IV

                //Budew evolves to Roselia through 315
                if (id == 406)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 315,
                        requireFriendship = true,
                        condition = (pmon) => Daytime.IsDaytime
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Burmy evolves to Wormadam through level 20 and female
                if (id == 412)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 413,
                        level = 20,
                        condition = (pmon) => pmon.gender == false
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Burmy evolves to Mothim through level 20 and male
                if (id == 412)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 414,
                        level = 20,
                        condition = (pmon) => pmon.gender == true
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Combee evolves to Vespiquen through level 21 and female
                if (id == 415)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 416,
                        level = 21,
                        condition = (pmon) => pmon.gender == false
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Aipom evolves to Ambipom through level up knowing double hit (move)
                if (id == 190)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 424,
                        condition = (pmon) => pmon.moveIds.Contains(458) //Double hit move id is 458
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Buneary evolves to Lopunny through friendship
                if (id == 427)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 428,
                        requireFriendship = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Chingling evolves to Chimecho through friendship during night
                if (id == 433)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 358,
                        requireFriendship = true,
                        condition = (pmon) => !Daytime.IsDaytime
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Bonsly evolves to Sudowoodo through level up knowing mimic (move)
                if (id == 438)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 185,
                        condition = (pmon) => pmon.moveIds.Contains(102)
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Mime Jr. evolves to Mr. Mime through level up knowing mimic (move)
                if (id == 439)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 122,
                        condition = (pmon) => pmon.moveIds.Contains(102)
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Happiny evolves to Chansey through level up holding oval stone during day
                if (id == 440)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 113,
                        heldItemId = 110,
                        condition = (pmon) => Daytime.IsDaytime
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Muchlax evolves to Snorlax through friendship
                if (id == 446)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 143,
                        requireFriendship = true,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Riolu evolves to Lucario through friendship during day
                if (id == 447)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 448,
                        requireFriendship = true,
                        condition = (pmon) => Daytime.IsDaytime
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Mantyke evolves to Mantine through level up with remoraid in party
                if (id == 458)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 226,
                        condition = (pmon) => PlayerData.singleton.partyPokemon.Where(p => p.speciesId == 223).Count() > 0 //Player has remoraid (id 223) in party
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Sneasel evolves to Weavile through level up holding razor claw during night
                if (id == 215)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 461,
                        heldItemId = 326,
                        condition = (pmon) => !Daytime.IsDaytime
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Lickitung evolves to Lickilicky through level up knowing rollout (move)
                if (id == 108)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 463,
                        condition = (pmon) => pmon.moveIds.Contains(205)
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Rhydon evolves to Rhyperior through trade holding protector
                if (id == 112)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 464,
                        requireTrade = true,
                        heldItemId = 321,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Tangela evolves to Tangrowth through level up knowing ancient power (move)
                if (id == 114)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 465,
                        condition = (pmon) => pmon.moveIds.Contains(246)
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Electabuzz evolves to Electivire through trade holding electirizer
                if (id == 125)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 466,
                        requireTrade = true,
                        heldItemId = 322,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Magmar evolves to Magmortar through trade holding magmariser
                if (id == 126)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 467,
                        requireTrade = true,
                        heldItemId = 323,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Yanma evolves to Yanmega through level up knowing ancient power (move)
                if (id == 193)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 469,
                        condition = (pmon) => pmon.moveIds.Contains(246)
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Gligar evolves to Gliscor through level up holding razor fang during night
                if (id == 207)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 472,
                        heldItemId = 327,
                        condition = (pmon) => !Daytime.IsDaytime
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Piloswine evolves to Mamoswine through level up knowing ancient power (move)
                if (id ==221)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 473,
                        condition = (pmon) => pmon.moveIds.Contains(246)
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Porygon2 evolves to Porygon-Z through trade holding dubious disc
                if (id == 233)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 474,
                        requireTrade = true,
                        heldItemId = 324,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Kirlia evolves to Gallade through using dawn stone and male
                if (id == 281)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 475,
                        useItemId = 109,
                        condition = (pmon) => pmon.gender == true
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Dusclops evolves to Dusknoir through trade holding reaper cloth
                if (id == 356)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 477,
                        requireTrade = true,
                        heldItemId = 325,
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                //Snorunt evolves to Froslass through using dawn stone and female
                if (id == 361)
                {

                    PokemonSpecies.Evolution newEvolution = new PokemonSpecies.Evolution()
                    {
                        targetId = 478,
                        useItemId = 475,
                        condition = (pmon) => pmon.gender == false
                    };

                    List<PokemonSpecies.Evolution> evolutionsList = new List<PokemonSpecies.Evolution>(evolutions);
                    evolutionsList.Add(newEvolution);
                    evolutions = evolutionsList.ToArray();

                }

                #endregion

                #endregion

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

                    baseMoves = baseMoves,
                    levelUpMoves = levelUpMoves,
                    discMoves = discMoves,
                    eggMoves = eggMoves,
                    tutorMoves = tutorMoves,

                    maleRelativeGenderProportion = maleRelativeGenderProportion,
                    femaleRelativeGenderProportion = femaleRelativeGenderProportion,
                    genderlessRelativeGenderProportion = genderlessRelativeGenderProportion,

                    evYield = evYield,
                    catchRate = catchRate,
                    baseExperienceYield = baseExperienceYield,

                    baseFriendship = baseFriendship,

                    eggGroup1 = eggGroup1,
                    eggGroup2 = eggGroup2,
                    eggCycles = eggCycles,

                    height = height,
                    weight = weight

                });

            }

            PokemonSpecies.registry.SetValues(species.ToArray());

        }

        /// <summary>
        /// Parse a value into an array of integers for move ids
        /// </summary>
        /// <param name="value">The value to parse</param>
        /// <param name="errorDescName">A descriptive name for the move type being examined used for error logging description</param>
        /// <param name="entryId">The entry id. Used for error logging description</param>
        /// <returns>The array of move ids (int[]</returns>
        private static int[] ParseMovesArrayValue(string value,
            string errorDescName,
            string entryId)
        {

            int[] output;

            if (value != "")
            {
                try
                {
                    output = value.Split(';').Select((x) => int.Parse(x)).ToArray();
                }
                catch (FormatException)
                {
                    Debug.LogError("Invalid move id in " + errorDescName + " moves for id " + entryId);
                    output = new int[0];
                }
            }
            else
            {
                output = new int[0];
            }

            return output;

        }

    }

}