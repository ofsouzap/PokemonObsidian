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
        }
    }

    public byte GetNumberOfPartyPokemon() => (byte)partyPokemon.Count(x => x != null);

    public bool PartyIsFull { get => GetNumberOfPartyPokemon() == partyPokemon.Length; }

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
        => boxPokemon.AddPokemon(pokemon);

    #endregion

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
        /// How many NPCs the player has talked to
        /// </summary>
        public ulong npcsTalkedTo;

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

    public void AddNPCTalkedTo()
    {
        stats.npcsTalkedTo++;
    }

    public void AddSecondsPlayed(ulong seconds)
    {
        stats.timePlayed += seconds;
    }

    public void SetCheatsUsed()
    {
        stats.cheatsUsed = true;
    }

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

    #region Respawning

    public bool respawnSceneStackSet = false;
    public GameSceneManager.SceneStack respawnSceneStack;

    #endregion

}