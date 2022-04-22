using System;
using System.Collections.Generic;
using System.Linq;
using Pokemon;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;
using UnityEngine;

public class PlayerData
{

    public const string currencySymbol = "₽";

    /// <summary>
    /// The instance of this
    /// </summary>
    public static PlayerData singleton = new PlayerData();

    #region Pokemon

    public const int partyCapacity = 6;

    private PokemonInstance[] _partyPokemon = new PokemonInstance[partyCapacity];
    public PokemonInstance[] partyPokemon
    {
        get => _partyPokemon;
        set
        {

            if (value.Length == partyCapacity)
            {
                _partyPokemon = value;
            }
            else if (value.Length > partyCapacity)
            {
                Debug.LogError("Provided party pokemon array length greater than 6");
            }
            else
            {
                _partyPokemon = new PokemonInstance[partyCapacity];
                Array.Copy(value, _partyPokemon, value.Length);
            }

            RefreshSettingCheatPokemon();

        }
    }

    public byte GetNumberOfPartyPokemon() => (byte)partyPokemon.Count(x => x != null);

    public bool PartyIsFull { get => GetNumberOfPartyPokemon() == partyPokemon.Length; }

    /// <summary>
    /// The first non-null pokemon in the player's party who isn't fainted
    /// </summary>
    public PokemonInstance PartyConsciousHead => partyPokemon.Where(x => x != null && !x.IsFainted).FirstOrDefault();

    public void HealPartyPokemon()
    {
        foreach (PokemonInstance pokemon in partyPokemon)
            pokemon?.RestoreFully(); //Restore all pokemon in the player's party (any unset pokemon are ignored)
    }

    public void AddNewPartyPokemon(PokemonInstance pokemon)
    {

        if (partyPokemon.All(x => x != null))
        {
            Debug.LogError("Trying to add party pokemon when party pokemon full");
            return;
        }

        RefreshSettingCheatPokemon();

        for (int i = 0; i < partyPokemon.Length; i++)
            if (partyPokemon[i] == null)
            {
                partyPokemon[i] = pokemon;
                break;
            }

    }

    public void ClearAllPokemon()
    {

        boxPokemon.ResetBoxes();
        partyPokemon = new PokemonInstance[partyCapacity];

    }

    /// <summary>
    /// Has a 0.5 chance of increasing every pokemon in the player's party's friendship for completing a step cycle
    /// </summary>
    public void RefreshAddFriendshipForStepCycle()
    {

        //0.5 chance of not increasing frienship
        if (UnityEngine.Random.Range(0, 2) == 0)
            return;

        foreach (PokemonInstance pokemon in partyPokemon)
        {

            if (pokemon != null)
                pokemon.AddFriendship(1);

        }

    }

    /// <summary>
    /// Adds friendship to all the player's party pokemon for defeating a gym leader, elite four member or the champion
    /// </summary>
    public void AddPartyPokemonFriendshipForGymVictory()
    {

        foreach (PokemonInstance pokemon in partyPokemon)
            pokemon?.AddFriendshipForGymVictory();

    }

    #region Box Pokemon

    /// <summary>
    /// A box for storing the player's pokemon in
    /// </summary>
    public class PokemonBox
    {

        public const int size = 30;

        /// <summary>
        /// The pokemon in this box
        /// </summary>
        public PokemonInstance[] pokemon = new PokemonInstance[size];

        public bool IsFull { get => pokemon.All(x => x != null); }

        /// <summary>
        /// Getting and setting of the pokemon in the box by referencing an instance of this by index instead of having to look at its pokemon property
        /// </summary>
        public PokemonInstance this[int index]
        {
            get
            {
                return pokemon[index];
            }
            set
            {
                pokemon[index] = value;
            }
        }

        public PokemonBox()
        {
            pokemon = new PokemonInstance[size];
        }

        /// <summary>
        /// Generate a box with the given pokemon within it
        /// </summary>
        /// <param name="pokemon">The pokemon to have in the box</param>
        public PokemonBox(PokemonInstance[] pokemon)
        {

            this.pokemon = new PokemonInstance[size];

            if (pokemon.Length > size)
            {

                Debug.Log("Length of pokemon array provided for box initialisation too great (" + pokemon.Length + ")");
                
            }
            else
            {

                Array.Copy(pokemon, this.pokemon, pokemon.Length);

            }

        }

        public int GetNumberOfPokemonInBox() => pokemon.Count(x => x != null);

        /// <summary>
        /// Adds a pokemon in the first available place
        /// </summary>
        /// <param name="pokemon">The pokemon to add</param>
        public void AddPokemon(PokemonInstance pokemon)
        {

            if (IsFull)
            {
                Debug.LogError("Trying to add pokemon to box when box is full");
                return;
            }

            for (int i = 0; i < this.pokemon.Length; i++)
                if (this.pokemon[i] == null)
                {
                    this.pokemon[i] = pokemon;
                    break;
                }

        }

        /// <summary>
        /// Removes any empty spaces in the box so that there is a continuous sequence of pokemon without any null
        /// </summary>
        public void CleanEmptySpaces()
        {

            List<PokemonInstance> newList = new List<PokemonInstance>();

            foreach (PokemonInstance pokemonInstance in pokemon)
            {
                if (pokemonInstance != null)
                    newList.Add(pokemonInstance);
            }

            PokemonInstance[] newArray = new PokemonInstance[size];

            Array.Copy(newList.ToArray(), newArray, newList.Count);

            pokemon = newArray;

        }

    }

    /// <summary>
    /// The collection of all the player's pokemon boxes aka their storage system
    /// </summary>
    public class PokemonStorageSystem
    {

        public const int boxCount = 18;
        public const int totalStorageSystemSize = boxCount * PokemonBox.size;

        /// <summary>
        /// The player's pokemon boxes
        /// </summary>
        public PokemonBox[] boxes = new PokemonBox[boxCount];

        public bool IsFull { get => boxes.All(x => x.IsFull); }

        private const string boxNamePrefix = "Box ";

        public static string GetBoxName(int boxIndex)
            => boxNamePrefix + (boxIndex + 1).ToString(); //Start box name indexes at 1

        public void ResetBoxes()
        {
            for (int i = 0; i < boxes.Length; i++)
                boxes[i] = new PokemonBox();
        }

        public PokemonStorageSystem()
        {
            boxes = new PokemonBox[boxCount];
            ResetBoxes();
        }

        /// <summary>
        /// Initialise a pokemon storage system using a 1-dimensional array of pokemon to place into the system
        /// </summary>
        /// <param name="pokemon">The pokemon to place</param>
        public PokemonStorageSystem(PokemonInstance[] pokemon)
        {

            boxes = new PokemonBox[boxCount];
            ResetBoxes();

            if (pokemon.Length > totalStorageSystemSize)
            {
                Debug.LogError("Length of pokemon array provided for storage system initialisation too great (" + pokemon.Length + ")");
            }
            else
            {

                int boxesToFill = pokemon.Length / PokemonBox.size;

                //For loop with index as box number to place pokemon into. The final box is excluded and so is dealt with afterwards as the number of pokemon to copy has changed
                for (int boxIndex = 0; boxCount < boxesToFill; boxIndex++)
                {

                    Array.Copy(
                        pokemon,
                        boxIndex * PokemonBox.size,
                        boxes[boxIndex].pokemon,
                        0,
                        PokemonBox.size
                        );

                }

                int remainingPokemon = pokemon.Length % PokemonBox.size;

                Array.Copy(
                    pokemon,
                    pokemon.Length - remainingPokemon,
                    boxes[boxesToFill].pokemon, //This uses the box after the final filled box
                    0,
                    remainingPokemon
                    );

            }

        }

        /// <summary>
        /// Initialise a storage system using a 2-dimensional array of pokemon to represent what pokemon should be in what boxes
        /// </summary>
        /// <param name="pokemonBoxes">The pokemon boxes to create</param>
        public PokemonStorageSystem(PokemonInstance[][] pokemonBoxes)
        {

            boxes = new PokemonBox[boxCount];
            ResetBoxes();

            if (pokemonBoxes.Length > boxCount)
            {
                Debug.LogError("Number of requested pokemon boxes greater than storage system box count");
            }
            else
            {

                for (int i = 0; i < pokemonBoxes.Length; i++)
                {

                    boxes[i] = new PokemonBox(pokemonBoxes[i]);

                }

            }

        }

        public int GetNumberOfPokemonInSystem() => boxes.Select(x => x.GetNumberOfPokemonInBox()).Sum();

        /// <summary>
        /// Adds a pokemon in the first available place
        /// </summary>
        /// <param name="pokemon">The pokemon to add</param>
        public void AddPokemon(PokemonInstance pokemon)
        {

            if (IsFull)
            {
                Debug.LogError("Trying to add pokemon to storage system when storage system is full");
                return;
            }

            foreach (PokemonBox box in boxes)
                if (!box.IsFull)
                {
                    box.AddPokemon(pokemon);
                    break;
                }

        }

        /// <summary>
        /// Removes any empty spaces in any boxes so that there are continuous sequences of pokemon without any null
        /// </summary>
        public void CleanEmptySpaces()
        {
            foreach (PokemonBox box in boxes)
                box.CleanEmptySpaces();
        }

    }

    /// <summary>
    /// The player's pokemon in their boxes
    /// </summary>
    public PokemonStorageSystem boxPokemon = new PokemonStorageSystem();

    public int GetNumberOfPokemonInBoxes() => boxPokemon.GetNumberOfPokemonInSystem();

    /// <summary>
    /// Adds a pokemon to the player's pokemon storage system in the first available place
    /// </summary>
    /// <param name="pokemon">The pokemon to add</param>
    public void AddBoxPokemon(PokemonInstance pokemon)
    {

        if (stats.cheatsUsed)
            pokemon.SetCheatPokemon();

        boxPokemon.AddPokemon(pokemon);

    }

    #endregion

    public struct PokemonLocator
    {

        /// <summary>
        /// Whether the pokemon is in the player's party. If not, the pokemon is in the player's storage system
        /// </summary>
        public bool inParty;

        /// <summary>
        /// The index of the pokemon in the player's party (only used if pokemon is in party)
        /// </summary>
        public int partyIndex;

        /// <summary>
        /// The index of the pokemon's box in the player's storage system (only used if pokemon is not in party)
        /// </summary>
        public int boxIndex;

        /// <summary>
        /// The index of the pokemon in its box (only used if pokemon is not in party)
        /// </summary>
        public int boxPositionIndex;

        public PokemonLocator(int partyIndex)
        {
            inParty = true;
            this.partyIndex = partyIndex;
            boxIndex = boxPositionIndex = -1;
        }

        public PokemonLocator(int boxIndex, int boxPositionIndex)
        {
            inParty = false;
            this.boxIndex = boxIndex;
            this.boxPositionIndex = boxPositionIndex;
            partyIndex = -1;
        }

        public PokemonInstance Get(PlayerData player)
        {

            if (inParty)
                return player.partyPokemon[partyIndex];
            else
                return player.boxPokemon.boxes[boxIndex][boxPositionIndex];

        }

        public void Set(PlayerData player, PokemonInstance value)
        {

            if (inParty)
                player.partyPokemon[partyIndex] = value;
            else
                player.boxPokemon.boxes[boxIndex][boxPositionIndex] = value;

            player.RefreshSettingCheatPokemon();

        }

    }

    public void HealStorageSystemPokemon()
    {

        foreach (PokemonBox box in boxPokemon.boxes)
            foreach (PokemonInstance pmon in box.pokemon)
                pmon?.RestoreFully();

    }

    public void HealAllPokemon()
    {
        HealPartyPokemon();
        HealStorageSystemPokemon();
    }

    #region Trade-Received Pokemon

    private List<Guid> tradeReceivedPokemonGuids = new List<Guid>();

    public Guid[] TradeReceivedPokemonArray
        => tradeReceivedPokemonGuids.ToArray();

    public bool CheckPokemonIsTradeReceived(Guid guid)
        => tradeReceivedPokemonGuids.Contains(guid);

    public bool CheckPokemonIsTradeReceived(PokemonInstance pmon)
        => CheckPokemonIsTradeReceived(pmon.guid);

    public void AddTradeReceivedPokemon(Guid guid)
        => tradeReceivedPokemonGuids.Add(guid);

    public void AddTradeReceivedPokemon(PokemonInstance pmon)
        => AddTradeReceivedPokemon(pmon.guid);

    public void SetTradeReceivedPokemon(Guid[] guids)
    {
        tradeReceivedPokemonGuids.Clear();
        tradeReceivedPokemonGuids.AddRange(guids);
    }

    public void SetTradeReceivedPokemon(PokemonInstance[] pmon)
    {
        tradeReceivedPokemonGuids.Clear();
        tradeReceivedPokemonGuids.AddRange(pmon.Select(p => p.guid));
    }

    public void TryRemoveTradeReceivedPokemon(Guid guid)
    {
        if (tradeReceivedPokemonGuids.Contains(guid))
            tradeReceivedPokemonGuids.Remove(guid);
    }

    public void TryRemoveTradeReceivedPokemon(PokemonInstance pmon)
        => TryRemoveTradeReceivedPokemon(pmon.guid);

    #endregion

    #endregion

    #region Profile

    public class Profile
    {

        #region Sprite

        public static readonly Dictionary<byte, string> playerSpriteIdNames = new Dictionary<byte, string>()
        {
            { 0, "player_male_0" },
            { 1, "player_female_0" },
            { 2, "player_male_1" },
            { 3, "player_female_1" },
            { 4, "player_male_2" },
            { 5, "player_female_2" }
        };

        public byte spriteId = 0;

        public static string GetPlayerSpriteNameById(byte id)
            => playerSpriteIdNames[id];

        public string SpriteName
            => GetPlayerSpriteNameById(spriteId);

        #endregion

        public Guid guid = Guid.Empty;

        public string name = "";

        public const int maximumMoney = 9999999;
        public int money = 0;

        public List<int> defeatedGymIds = new List<int>();

    }

    public Profile profile = new Profile();

    public void SetRandomGuid()
    {
        profile.guid = Guid.NewGuid();
    }

    public void AddMoney(int amount)
    {

        if (amount >= 0)
        {

            if (profile.money > Profile.maximumMoney - amount)
                profile.money = Profile.maximumMoney;
            else
                profile.money += amount;

        }
        else
        {

            if (Mathf.Abs(amount) > profile.money)
                profile.money = 0;
            else
                profile.money += amount;

        }

    }

    public void SetGymDefeated(int gymId)
    {

        if (!profile.defeatedGymIds.Contains(gymId))
            profile.defeatedGymIds.Add(gymId);
        else
            Debug.LogWarning("Trying to record gym as battled when already recorded");

    }

    #endregion

    #region Stats

    public class Stats
    {

        /// <summary>
        /// The total distance the player has walked in grid steps
        /// </summary>
        public ulong distanceWalked;

        /// <summary>
        /// The total time the player has spent playing in seconds
        /// </summary>
        public ulong timePlayed;

        /// <summary>
        /// The time the game was started (in epoch seconds)
        /// </summary>
        public ulong gameStartTime;

        /// <summary>
        /// Whether the player has used cheats or not (opening the cheat console without using any cheats doesn't count)
        /// </summary>
        public bool cheatsUsed = false;

    }

    public Stats stats = new Stats();

    public void SetGameStartTimeAsNow()
    {
        stats.gameStartTime = Convert.ToUInt64(EpochTime.SecondsNow);
    }

    public void AddStepWalked()
    {
        stats.distanceWalked++;
    }

    public void AddSecondsPlayed(ulong seconds)
    {
        stats.timePlayed += seconds;
    }

    public void RefreshSettingCheatPokemon()
    {
        if (stats.cheatsUsed)
            SetPokemonAsCheatPokemon();
    }

    private void SetPokemonAsCheatPokemon()
    {

        foreach (PokemonInstance pmon in partyPokemon)
            pmon?.SetCheatPokemon();

        foreach (PokemonBox box in boxPokemon.boxes)
            foreach (PokemonInstance pmon in box.pokemon)
                pmon?.SetCheatPokemon();

    }

    public void SetCheatsUsed()
    {

        stats.cheatsUsed = true;

        RefreshSettingCheatPokemon();

    }

    #endregion

    #region Pokedex

    public class Pokedex
    {

        public Pokedex()
        {
            entries = new List<Entry>();
        }

        public Pokedex(Entry[] entries)
        {
            this.entries = new List<Entry>(entries);
        }

        public struct Entry
        {

            public int speciesId;
            public int seen;
            public int caught;

            public Entry(int speciesId, int seen, int caught)
            {
                this.speciesId = speciesId;
                this.seen = seen;
                this.caught = caught;
            }

            public Entry(int speciesId) : this(speciesId, 0, 0) { }

        }

        private List<Entry> entries;

        public Entry this[int speciesId]
        {

            private set
            {

                if (value.speciesId != speciesId)
                    throw new ArgumentException("Species id to set mismatch");

                int i = entries.FindIndex(e => e.speciesId == speciesId);

                if (i >= 0)
                    entries[i] = value;
                else
                    entries.Add(value);

            }

            get
            {

                Entry[] es = entries.Where(e => e.speciesId == speciesId).ToArray();

                if (es.Length > 0)
                    return es[0];
                else
                    return new Entry(speciesId);

            }

        }

        public int GetSeenCount(int speciesId)
            => this[speciesId].seen;

        public bool GetHasBeenEncountered(int speciesId)
            => GetSeenCount(speciesId) > 0 || GetHasBeenCaught(speciesId);

        public int GetCaughtCount(int speciesId)
            => this[speciesId].caught;

        public bool GetHasBeenCaught(int speciesId)
            => GetCaughtCount(speciesId) > 0;

        public void AddPokemonSeen(int speciesId, int count = 1)
        {

            Entry oldEntry = this[speciesId];
            Entry newEntry = new Entry(speciesId, oldEntry.seen + count, oldEntry.caught);
            this[speciesId] = newEntry;

        }

        public void AddPokemonSeen(PokemonInstance pmon)
            => AddPokemonSeen(pmon.speciesId);

        public void AddPokemonCaught(int speciesId, int count = 1)
        {

            Entry oldEntry = this[speciesId];
            Entry newEntry = new Entry(speciesId, oldEntry.seen, oldEntry.caught + count);
            this[speciesId] = newEntry;

        }

        public void AddPokemonCaught(PokemonInstance pmon)
            => AddPokemonCaught(pmon.speciesId);

        /// <summary>
        /// Get all the entries that have been saved without creating fake ones with values of 0. To be used for serialization of the pokedex
        /// </summary>
        public Entry[] GetAllSavedEntries()
            => entries.ToArray();

    }

    public Pokedex pokedex = new Pokedex();

    #endregion

    #region Items

    public class Inventory
    {

        public class InventorySection
        {

            public Func<int, Item> GetItemById;

            public InventorySection(Func<int, Item> RegistrySearchFunction)
            {
                GetItemById = RegistrySearchFunction;
            }

            private Dictionary<int, int> quantities = new Dictionary<int, int>();

            public bool IsEmpty
            {
                get
                {

                    if (quantities.Count == 0)
                        return true;

                    foreach (int itemId in quantities.Keys)
                        if (quantities[itemId] > 0)
                            return false;

                    return true;

                }
            }

            public void AddItem(int itemId,
                uint amount = 1)
            {
                if (quantities.ContainsKey(itemId))
                    quantities[itemId] += (int)amount;
                else
                    quantities.Add(itemId, (int)amount);
            }

            public void RemoveItem(int itemId,
                uint amount = 1)
            {

                if (quantities.ContainsKey(itemId))

                    if (quantities[itemId] > amount)
                        quantities[itemId] -= (int)amount;

                    else if (quantities[itemId] == amount)
                        quantities.Remove(itemId);

                    else
                        Debug.LogError("Requested amount for removal exceeds quantity present");

                else

                    Debug.LogError("InventorySection doesn't contain specified item (id " + itemId + ")");

            }

            public void SetItems(Dictionary<int, int> items) => quantities = items;

            public int GetQuantity(int itemId)
                => quantities.ContainsKey(itemId) ? quantities[itemId] : 0;

            /// <summary>
            /// Gets the ids of all the items that the InventorySection contains at least 1 of
            /// </summary>
            public int[] GetItemIds()
                => quantities.Keys.ToArray();

            /// <summary>
            /// Gets all the Items that the InventorySection contains at least 1 of
            /// </summary>
            public Item[] GetItems()
                => GetItemIds().Select(x => GetItemById(x)).ToArray();

            public Dictionary<Item, int> ItemQuantites
            {
                get
                {

                    Dictionary<Item, int> output = new Dictionary<Item, int>();

                    foreach (int itemId in GetItemIds())
                        output.Add(GetItemById(itemId), quantities[itemId]);

                    return output;

                }
            }

        }

        public InventorySection generalItems = new InventorySection((id) => GeneralItem.GetGeneralItemItemById(id));
        public InventorySection medicineItems = new InventorySection((id) => MedicineItem.GetMedicineItemById(id));
        public InventorySection battleItems = new InventorySection((id) => BattleItem.GetBattleItemById(id));
        public InventorySection pokeBalls = new InventorySection((id) => PokeBall.GetPokeBallById(id));
        public InventorySection tmItems = new InventorySection((id) => TMItem.GetTMItemById(id));

        public InventorySection GetItemInventorySection(Item item)
        {

            if (item is GeneralItem)
                return generalItems;
            else if (item is PokeBall)
                return pokeBalls;
            else if (item is TMItem)
                return tmItems;
            else if (item is MedicineItem)
                return medicineItems;
            else if (item is BattleItem)
                return battleItems;
            else
            {
                Debug.LogError("Couldn't find inventory section for provided item");
                return null;
            }

        }

        public bool IsEmpty
            => generalItems.IsEmpty
            && medicineItems.IsEmpty
            && battleItems.IsEmpty
            && pokeBalls.IsEmpty
            && tmItems.IsEmpty;

        public void AddItem(Item item,
            uint amount = 1)
            => GetItemInventorySection(item).AddItem(item.id, amount);

        public void RemoveItem(Item item,
            uint amount = 1)
            => GetItemInventorySection(item).RemoveItem(item.id, amount);

    }

    public Inventory inventory = new Inventory();

    #region Dropped Items

    public List<int> collectedDroppedItemsIds = new List<int>();

    public void SetDroppedItemCollected(int id)
    {

        if (collectedDroppedItemsIds.Contains(id))
            Debug.LogWarning("Item id being set for collection already set as collected (" + id + ")");
        else
            collectedDroppedItemsIds.Add(id);

    }

    public bool GetDroppedItemHasBeenCollected(int id)
        => collectedDroppedItemsIds.Contains(id);

    #endregion

    #endregion

    #region NPCs

    #region NPCs Battled

    /// <summary>
    /// A list of which battle-challenging NPCs the player has battled and so won't try to challenge the player on spotting them.
    /// The elements are the IDs of the NPCs as set in their public properties (found in FreeRoaming.NPCs.NPCController)
    /// </summary>
    public List<int> npcsBattled = new List<int>();

    public void SetNPCBattled(int npcId)
    {

        if (FreeRoaming.NPCs.NPCController.IdIsUnset(npcId))
        {
            Debug.LogError("Trying to set unset NPC id as battled");
            return;
        }

        if (npcsBattled.Contains(npcId))
        {
            Debug.LogWarning("NPC already  marked as battled - " + npcId);
        }
        else
        {
            npcsBattled.Add(npcId);
        }

    }

    public bool GetNPCBattled(int npcId)
    {

        if (FreeRoaming.NPCs.NPCController.IdIsUnset(npcId))
        {
            Debug.LogError("Trying to get unset NPC id is battled");
            return false;
        }

        return npcsBattled.Contains(npcId);

    }

    #endregion

    #region NPCs Talked To

    /// <summary>
    /// A list of NPCs the player has talked to
    /// </summary>
    public List<int> npcsTalkedTo = new List<int>();

    public void SetNPCTalkedTo(int npcId)
    {

        if (FreeRoaming.NPCs.NPCController.IdIsUnset(npcId))
        {
            Debug.LogError("Trying to set unset NPC id as talked to");
            return;
        }

        if (npcsTalkedTo.Contains(npcId))
        {
            Debug.LogWarning("NPC already marked as talked to - " + npcId);
        }
        else
        {
            npcsTalkedTo.Add(npcId);
        }

    }

    public bool GetNPCTalkedTo(int npcId)
    {

        if (FreeRoaming.NPCs.NPCController.IdIsUnset(npcId))
        {
            Debug.LogError("Trying to get unset NPC id is talked to");
            return false;
        }

        return npcsTalkedTo.Contains(npcId);

    }

    public int GetNPCsTalkedToCount()
        => npcsTalkedTo.Count;

    #endregion

    #endregion

    #region Respawning

    public bool respawnSceneStackSet = false;
    public GameSceneManager.SceneStack respawnSceneStack;

    #endregion

    #region Progression Events

    public List<ProgressionEvent> completedProgressionEvents = new List<ProgressionEvent>();

    public int[] CompletedProgressionEventCodesArray
        => completedProgressionEvents
        .Select(pe => (int)pe)
        .ToArray();

    public void AddCompletedProgressionEvent(ProgressionEvent evt)
    {

        if (completedProgressionEvents.Contains(evt))
            Debug.LogWarning("Trying to complete progression event when event already marked as completed");
        else
            completedProgressionEvents.Add(evt);

    }

    public bool GetProgressionEventCompleted(ProgressionEvent evt)
        => completedProgressionEvents.Contains(evt);

    #endregion

}