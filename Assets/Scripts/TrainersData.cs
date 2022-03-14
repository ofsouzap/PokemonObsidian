using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;
using Battle;

public class TrainersData
{

    const string dataPath = "Data/trainers";
    const bool ignoreDataFirstLine = true;

    #region Registry

    public class TrainerDetails : IHasId
    {

        public int GetId() => id;

        public int id;
        public int? gymId;
        public string name;

        /// <summary>
        /// The message this trainer should say when it challenges the player
        /// </summary>
        public string challengeMessage;

        /// <summary>
        /// The message this NPC should say if the player interacts with them but they can't battle. If blank or null, is ignored
        /// </summary>
        public string chatMessage;

        public string[] defeatMessages;

        public string battleBackgroundResourceName;

        /// <summary>
        /// The trainer's class. Used for base payout, battle sprite, trainer class name etc.
        /// </summary>
        public TrainerClass.Class trainerClass;

        public BattleParticipantNPC.Mode mode;

        public PokemonInstance.BasicSpecification[] pokemonSpecifications;

    }

    private static Registry<TrainerDetails> registry = new Registry<TrainerDetails>();

    public static TrainerDetails GetTrainerDetailsByTrainerId(int id)
        => registry.StartingIndexSearch(id, id);

    #endregion

    /* Data CSV Columns:
     * trainer id (int)
     * gym id (int, if applicable otherwise blank for null)
     * name (string)
     * challenge message (string)
     * chat message (string)
     * defeat messages (strings separated by ';')
     * battle background resource name (string)
     * class (string for trainer's trainer class)
     * mode (string for trainer's npc battle participant mode)
     * [for each of 6 pokemon:]
     * pmon i's species id (int)
     * pmon i's gender (m/f/{blank})
     * pmon i's level (int)
     * pmon i's evs (6 ints separated by ';')
     * pmon i's ivs (6 ints separated by ';')
     * pmon i's move ids (4 ints separated by ';' or blank if should use defaults)
     * 
     */

    public static void LoadData()
    {

        List<TrainerDetails> details = new List<TrainerDetails>();

        string[][] data = CSV.ReadCSVResource(dataPath, ignoreDataFirstLine);

        foreach (string[] entry in data)
        {

            int id;
            int? gymId;
            string name, challengeMessage, chatMessage, battleBackgroundResourceName;
            string[] defeatMessages;
            TrainerClass.Class trainerClass;
            BattleParticipantNPC.Mode mode;
            List<PokemonInstance.BasicSpecification> pmonList;

            if (entry.Length < 45)
            {
                Debug.LogWarning("Invalid trainers details to load - " + entry);
                continue;
            }

            // Id
            if (!int.TryParse(entry[0], out id))
            {
                Debug.LogWarning("Invalid id for trainer details - " + entry[0]);
                continue;
            }

            // Gym id
            if (entry[1] == "")
            {
                gymId = null;
            }
            else
            {
                if (int.TryParse(entry[1], out int gymIdParsed))
                {
                    gymId = gymIdParsed;
                }
                else
                {
                    Debug.LogWarning("Invalid gym id for trainer details id - " + id);
                    continue;
                }
            }

            // Name
            name = entry[2];

            // Challenge message
            challengeMessage = entry[3];

            // Chat message
            chatMessage = entry[4];

            // Defeat messages
            defeatMessages = entry[5].Split(';');

            // Battle background resource
            string battleBackgroundResourceNameEntry = entry[6];

            if (battleBackgroundResourceNameEntry == "")
                battleBackgroundResourceName = BattleEntranceArguments.defaultBackgroundName;
            else
                battleBackgroundResourceName = battleBackgroundResourceNameEntry;

            // Trainer class
            if (!TrainerClass.TryParse(entry[7], out trainerClass))
            {
                Debug.LogWarning("Invalid trainer class for trainer details id - " + id);
                continue;
            }

            // Mode
            if (!BattleParticipantNPC.TryParse(entry[8], out mode))
            {
                Debug.LogWarning("Invalid trainer class for trainer details id - " + id);
                continue;
            }

            // Pokemon
            pmonList = new List<PokemonInstance.BasicSpecification>();
            bool invalidPokemonFound = false;
            for (int pmonIndex = 0; pmonIndex < PlayerData.partyCapacity; pmonIndex++)
            {

                const int pmonEntryCount = 6;
                int indexOffset = 9 + (pmonIndex * pmonEntryCount);

                int speciesId;
                bool? gender;
                byte level;
                Stats<byte> evs, ivs;
                int[] moveIds;
                bool useDefaultMoves;

                // Species id

                string idEntry = entry[indexOffset];

                if (idEntry == "") // Pokemon not set
                    continue;
                else if (!int.TryParse(idEntry, out speciesId))
                {
                    Debug.LogWarning("Invalid species id for pokemon for trainer details id  - " + id);
                    invalidPokemonFound = true;
                    break;
                }

                // Gender
                switch (entry[indexOffset + 1].ToLower())
                {

                    case "m":
                        gender = true;
                        break;

                    case "f":
                        gender = false;
                        break;

                    case "":
                    case "n":
                        gender = null;
                        break;

                    default:
                        Debug.LogWarning("Invalid gender for pokemon for trainer details id - " + id);
                        invalidPokemonFound = true;
                        gender = default;
                        break;

                }

                if (invalidPokemonFound)
                    break;

                // Level
                if (!byte.TryParse(entry[indexOffset + 2], out level))
                {
                    Debug.LogWarning("Invalid level for pokemon for trainer details id  - " + id);
                    invalidPokemonFound = true;
                    break;
                }

                // EVs
                byte evAtk, evDef, evSpatk, evSpdef, evHp, evSpd;
                string[] evParts = entry[indexOffset + 3].Split(';');

                try
                {

                    evAtk = byte.Parse(evParts[0]);
                    evDef = byte.Parse(evParts[1]);
                    evSpatk = byte.Parse(evParts[2]);
                    evSpdef = byte.Parse(evParts[3]);
                    evHp = byte.Parse(evParts[4]);
                    evSpd = byte.Parse(evParts[5]);

                }
                catch (FormatException)
                {
                    Debug.LogWarning("Invalid evs entry for pokemon for trainer details id - " + id);
                    invalidPokemonFound = true;
                    break;
                }

                evs = new Stats<byte>()
                {
                    attack = evAtk,
                    defense = evDef,
                    specialAttack = evSpatk,
                    specialDefense = evSpdef,
                    health = evHp,
                    speed = evSpd
                };

                // IVs
                byte ivAtk, ivDef, ivSpatk, ivSpdef, ivHp, ivSpd;
                string[] ivParts = entry[indexOffset + 4].Split(';');

                try
                {

                    ivAtk = byte.Parse(ivParts[0]);
                    ivDef = byte.Parse(ivParts[1]);
                    ivSpatk = byte.Parse(ivParts[2]);
                    ivSpdef = byte.Parse(ivParts[3]);
                    ivHp = byte.Parse(ivParts[4]);
                    ivSpd = byte.Parse(ivParts[5]);

                }
                catch (FormatException)
                {
                    Debug.LogWarning("Invalid ivs entry for pokemon for trainer details id - " + id);
                    invalidPokemonFound = true;
                    break;
                }

                ivs = new Stats<byte>()
                {
                    attack = ivAtk,
                    defense = ivDef,
                    specialAttack = ivSpatk,
                    specialDefense = ivSpdef,
                    health = ivHp,
                    speed = ivSpd
                };

                // Move ids

                string movesEntry = entry[indexOffset + 5];

                if (movesEntry == "")
                {
                    useDefaultMoves = true;
                    moveIds = default;
                }
                else
                {

                    useDefaultMoves = false;
                    moveIds = new int[4] { -1, -1, -1, -1 };

                    int movesIdsIndex = 0;

                    foreach (string s in movesEntry.Split(';'))
                    {

                        if (int.TryParse(s, out int moveId))
                        {
                            moveIds[movesIdsIndex++] = moveId;
                        }
                        else
                        {
                            Debug.LogWarning("Invalid move id for pokemon for trainer details id - " + id);
                            invalidPokemonFound = true;
                            break;
                        }

                    }

                    if (invalidPokemonFound)
                        break;

                }

                // Add to list

                PokemonInstance.BasicSpecification pokemon = new PokemonInstance.BasicSpecification()
                {
                    speciesId = speciesId,
                    gender = PokemonInstance.BasicSpecification.GetGenderEnumVal(gender),
                    useAutomaticMoves = useDefaultMoves,
                    moveIds = moveIds,
                    level = level,
                    EVs = evs,
                    IVs = ivs
                };

                pmonList.Add(pokemon);

            }

            if (invalidPokemonFound)
                continue;

            // Add entry
            TrainerDetails trainer = new TrainerDetails()
            {
                id = id,
                gymId = gymId,
                name = name,
                challengeMessage = challengeMessage,
                chatMessage = chatMessage,
                defeatMessages = defeatMessages,
                battleBackgroundResourceName = battleBackgroundResourceName,
                trainerClass = trainerClass,
                mode = mode,
                pokemonSpecifications = pmonList.ToArray()
            };

            details.Add(trainer);

        }

        registry.SetValues(details.ToArray());

    }

}
