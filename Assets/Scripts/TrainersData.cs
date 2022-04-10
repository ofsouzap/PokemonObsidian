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

        public string GetFullName()
        {

            string prefixPart =
                TrainerClass.classNamesPrefixes.ContainsKey(trainerClass)
                    && TrainerClass.classNamesPrefixes[trainerClass] != ""
                ? TrainerClass.classNamesPrefixes[trainerClass]
                : "";

            string namePart =
                name != null
                    && name != ""
                ? name
                : "";

            if (namePart == "" && prefixPart == "")
                return "Trainer";
            else if (namePart == "")
                return prefixPart;
            else if (prefixPart == "")
                return namePart;
            else
                return prefixPart + ' ' + namePart;

        }

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

        public string GetBattleSpriteResourceName()
            => TrainerClass.classBattleSpriteNames[trainerClass];

        public byte GetBasePayout()
        {

            //Gym leaders don't have payouts
            if (gymId != null)
                return 0;

            return TrainerClass.classBasePayouts.ContainsKey(trainerClass)
                ? TrainerClass.classBasePayouts[trainerClass]
                : (byte)0;

        }

        public BattleParticipantNPC.Mode mode;

        public int leaderHealingItemId;
        public int leaderMaxTimesHealed;

        public PokemonInstance.BasicSpecification[] pokemonSpecifications;

        public PokemonInstance[] GenerateParty()
            => pokemonSpecifications.Select(x => x.Generate()).ToArray();

    }

    private static Registry<TrainerDetails> registry = new Registry<TrainerDetails>();

    public static TrainerDetails GetTrainerDetailsByTrainerId(int id)
        => registry.BinarySearch(id);

    #endregion

    /* Data CSV Columns:
     * npc id (int)
     * gym id (int, if applicable otherwise blank for null)
     * name (string)
     * challenge message (string)
     * chat message (string)
     * defeat messages (strings separated by ';')
     * battle background resource name (string)
     * class (string for trainer's trainer class)
     * mode (string for trainer's npc battle participant mode)
     * (for gym leaders) healing item id (int or blank)
     * (for gym leaders) max times healed (int or blank)
     * [for each of 6 pokemon:]
     *     pmon i's species id (int)
     *     pmon i's gender (m/f/{blank})
     *     pmon i's level (int)
     *     pmon i's evs (6 ints separated by ';')
     *     pmon i's ivs (6 ints separated by ';')
     *     pmon i's move ids (4 ints separated by ';' or blank if should use defaults)
     */

    public static void LoadData()
    {

        List<TrainerDetails> details = new List<TrainerDetails>();

        string[][] data = CSV.ReadCSVResource(dataPath, ignoreDataFirstLine);

        foreach (string[] entry in data)
        {

            int id, leaderHealingItemId, leaderMaxTimesHealed;
            int? gymId;
            string name, challengeMessage, chatMessage, battleBackgroundResourceName;
            string[] defeatMessages;
            TrainerClass.Class trainerClass;
            BattleParticipantNPC.Mode mode;
            List<PokemonInstance.BasicSpecification> pmonList;

            if (entry.Length < 47)
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
            string defeatMessagesEntry = entry[5];
            if (defeatMessagesEntry == "")
                defeatMessages = new string[0];
            else
                defeatMessages = defeatMessagesEntry.Split(';');

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

            string modeEntry = entry[8];
            if (modeEntry == "")
            {

                if (gymId == null)
                    mode = BattleParticipantNPC.defaultTrainerMode;
                else
                    mode = BattleParticipantNPC.defaultGymLeaderMode;

            }
            else if (!BattleParticipantNPC.TryParse(modeEntry, out mode))
            {

                Debug.LogWarning("Invalid trainer class for trainer details id - " + id);
                continue;

            }

            // Gym leader healing settings

            string leaderHealingItemIdEntry = entry[9];
            if (leaderHealingItemIdEntry == "")
                leaderHealingItemId = Battle.NPCBattleParticipantModes.GymLeader.defaultHealingItemId;
            else
            {
                if (!int.TryParse(leaderHealingItemIdEntry, out leaderHealingItemId))
                {
                    Debug.LogError("Invalid leader healing item id for trainer id - " + id);
                    leaderHealingItemId = Battle.NPCBattleParticipantModes.GymLeader.defaultHealingItemId;
                }
                else
                {
                    Items.Item item = Items.Item.GetItemById(leaderHealingItemId);
                    if (item == null || !(item is Items.MedicineItems.HealthMedicineItem))
                    {
                        Debug.LogError("Inappropriate leader healing item id for trainer id - " + id);
                        leaderHealingItemId = Battle.NPCBattleParticipantModes.GymLeader.defaultHealingItemId;
                    }
                }
            }
            
            string leaderMaxTimesHealedEntry = entry[10];
            if (leaderMaxTimesHealedEntry == "")
                leaderMaxTimesHealed = Battle.NPCBattleParticipantModes.GymLeader.defaultMaxTimesHealed;
            else if (!int.TryParse(leaderMaxTimesHealedEntry, out leaderMaxTimesHealed))
            {
                Debug.LogError("Invalid leader max times healed entry for trainer id - " + id);
            }

            // Pokemon
            pmonList = new List<PokemonInstance.BasicSpecification>();
            bool invalidPokemonFound = false;
            for (int pmonIndex = 0; pmonIndex < PlayerData.partyCapacity; pmonIndex++)
            {

                const int pmonEntryCount = 6;
                int indexOffset = 11 + (pmonIndex * pmonEntryCount);

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
                leaderHealingItemId = leaderHealingItemId,
                leaderMaxTimesHealed = leaderMaxTimesHealed,
                pokemonSpecifications = pmonList.ToArray()
            };

            details.Add(trainer);

        }

        registry.SetValues(details.ToArray());

    }

}
