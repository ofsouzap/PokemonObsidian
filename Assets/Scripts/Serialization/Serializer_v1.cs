using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Pokemon;
using Items;
using Battle;

namespace Serialization
{
    public class Serializer_v1 : Serializer
    {

        public override ushort GetVersionCode()
            => 0x0001;

        #region Serialization

        public override void SerializeData(Stream stream, PlayerData player = null)
        {

            byte[] buffer;

            if (player == null)
                player = PlayerData.singleton;

            //Save details
            stream.Write(fileSignatureBytes, 0, 8);

            buffer = BitConverter.GetBytes(GetVersionCode());
            stream.Write(buffer, 0, 2);

            buffer = BitConverter.GetBytes(EpochTime.SecondsNow);
            stream.Write(buffer, 0, 8);

            //Player
            buffer = player.profile.guid.ToByteArray();
            stream.Write(buffer, 0, 16);

            buffer = BitConverter.GetBytes(player.stats.gameStartTime);
            stream.Write(buffer, 0, 8);

            //Pokemon
            SerializePlayerPartyAndStorageSystemPokemon(stream, player);

            //Scene stacks
            SerializeSceneStack(stream, GameSceneManager.CurrentSceneStack);
            SerializeBool(stream, player.respawnSceneStackSet);
            if (player.respawnSceneStackSet)
                SerializeSceneStack(stream, player.respawnSceneStack);

            //Profile Details
            buffer = new byte[1] { player.profile.spriteId };
            stream.Write(buffer, 0, 1);

            SerializeString(stream, player.profile.name);

            buffer = BitConverter.GetBytes(player.profile.money);
            stream.Write(buffer, 0, 4);

            //Defeated gyms
            buffer = BitConverter.GetBytes(player.profile.defeatedGymIds.Count);
            stream.Write(buffer, 0, 4);
            foreach (int gymId in player.profile.defeatedGymIds)
            {
                buffer = BitConverter.GetBytes(gymId);
                stream.Write(buffer, 0, 4);
            }

            //Stats
            buffer = BitConverter.GetBytes(player.stats.distanceWalked);
            stream.Write(buffer, 0, 8);

            buffer = BitConverter.GetBytes(player.stats.npcsTalkedTo);
            stream.Write(buffer, 0, 8);

            buffer = BitConverter.GetBytes(player.stats.timePlayed);
            stream.Write(buffer, 0, 8);
            
            SerializeBool(stream, player.stats.cheatsUsed);

            //Inventory
            SerializeInventorySection(stream, player.inventory.generalItems);
            SerializeInventorySection(stream, player.inventory.medicineItems);
            SerializeInventorySection(stream, player.inventory.battleItems);
            SerializeInventorySection(stream, player.inventory.pokeBalls);
            SerializeInventorySection(stream, player.inventory.tmItems);

            //Pokedex
            SerializePokedex(stream, player.pokedex);

            //NPCs battled
            buffer = BitConverter.GetBytes(player.npcsBattled.Count);
            stream.Write(buffer, 0, 4);
            foreach (int npcId in player.npcsBattled)
            {
                buffer = BitConverter.GetBytes(npcId);
                stream.Write(buffer, 0, 4);
            }

            //Settings
            int textSpeedIndex = GameSettings.singleton.textSpeedIndex;
            
            buffer = BitConverter.GetBytes(textSpeedIndex);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(GameSettings.singleton.musicVolume);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(GameSettings.singleton.sfxVolume);
            stream.Write(buffer, 0, 4);

            //Dropped items collected
            buffer = BitConverter.GetBytes(player.collectedDroppedItemsIds.Count);
            stream.Write(buffer, 0, 4);
            foreach (int droppedItemId in player.collectedDroppedItemsIds)
            {
                buffer = BitConverter.GetBytes(droppedItemId);
                stream.Write(buffer, 0, 4);
            }

        }

        public override void SerializePlayerPartyAndStorageSystemPokemon(Stream stream, PlayerData player = null)
        {

            byte[] buffer;

            if (player == null)
                player = PlayerData.singleton;

            int totalPokemonCount =
                PlayerData.singleton.partyPokemon.Count(x => x != null)
                + PlayerData.singleton.boxPokemon.boxes.Sum(box => box.pokemon.Count(x => x != null));
            buffer = BitConverter.GetBytes(totalPokemonCount);
            stream.Write(buffer, 0, 4);

            int pokemonListIndex = 0;

            int[] partyPokemonIndexes = new int[PlayerData.partyCapacity];
            int[] storageSystemPokemonIndexes = new int[PlayerData.PokemonStorageSystem.totalStorageSystemSize];

            #region Party Pokemon

            for (int i = 0; i < PlayerData.partyCapacity; i++)
            {

                PokemonInstance pokemon = player.partyPokemon[i];

                if (pokemon == null)
                {
                    partyPokemonIndexes[i] = -1;
                }
                else
                {
                    partyPokemonIndexes[i] = pokemonListIndex;
                    pokemonListIndex++;
                    SerializePokemonInstance(stream, pokemon);
                }

            }

            #endregion

            #region Box Pokemon

            for (int boxIndex = 0; boxIndex < PlayerData.PokemonStorageSystem.boxCount; boxIndex++)
            {
                for (int slotIndex = 0; slotIndex < PlayerData.PokemonBox.size; slotIndex++)
                {

                    int totalIndex = (boxIndex * PlayerData.PokemonBox.size) + slotIndex;
                    PokemonInstance pokemon = player.boxPokemon.boxes[boxIndex][slotIndex];

                    if (pokemon == null)
                    {
                        storageSystemPokemonIndexes[totalIndex] = -1;
                    }
                    else
                    {
                        storageSystemPokemonIndexes[totalIndex] = pokemonListIndex;
                        pokemonListIndex++;
                        SerializePokemonInstance(stream, pokemon);
                    }

                }
            }

            #endregion

            foreach (int value in partyPokemonIndexes)
            {
                buffer = BitConverter.GetBytes(value);
                stream.Write(buffer, 0, 4);
            }

            foreach (int value in storageSystemPokemonIndexes)
            {
                buffer = BitConverter.GetBytes(value);
                stream.Write(buffer, 0, 4);
            }

        }

        public override void SerializePokemonInstance(Stream stream, PokemonInstance pokemon)
        {

            byte[] buffer;

            //Species
            buffer = BitConverter.GetBytes(pokemon.speciesId);
            stream.Write(buffer, 0, 4);

            //Details
            buffer = pokemon.guid.ToByteArray();
            stream.Write(buffer, 0, 16);

            SerializeString(stream, pokemon.nickname);
            SerializeHeldItem(stream, pokemon);
            SerializeNullableBool(stream, pokemon.gender);

            //Catch
            buffer = BitConverter.GetBytes(pokemon.pokeBallId);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(pokemon.catchTime);
            stream.Write(buffer, 0, 8);

            //OT
            SerializeString(stream, pokemon.originalTrainerName);
            buffer = pokemon.originalTrainerGuid.ToByteArray();
            stream.Write(buffer, 0, 16);

            //Stats
            SerializeByteStats(stream, pokemon.effortValues);
            SerializeByteStats(stream, pokemon.individualValues);

            buffer = BitConverter.GetBytes(pokemon.natureId);
            stream.Write(buffer, 0, 4);

            SerializeIntStats(stream, pokemon.GetStats());

            buffer = new byte[1] { pokemon.friendship };
            stream.Write(buffer, 0, 1);

            //Moves
            buffer = BitConverter.GetBytes(pokemon.moveIds[0]);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(pokemon.moveIds[1]);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(pokemon.moveIds[2]);
            stream.Write(buffer, 0, 4);
            buffer = BitConverter.GetBytes(pokemon.moveIds[3]);
            stream.Write(buffer, 0, 4);

            buffer = pokemon.movePPs;
            stream.Write(buffer, 0, 4); //4 byte values

            buffer = BitConverter.GetBytes(pokemon.experience);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes((int)pokemon.nonVolatileStatusCondition);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(pokemon.health);
            stream.Write(buffer, 0, 4);

        }

        public override void SerializeInventorySection(Stream stream, PlayerData.Inventory.InventorySection section)
        {

            byte[] buffer;

            buffer = BitConverter.GetBytes(section.ItemQuantites.Count);
            stream.Write(buffer, 0, 4);

            foreach (Item item in section.ItemQuantites.Keys)
            {
                SerializeInventoryItem(stream, item.GetId(), section.ItemQuantites[item]);
            }
            
        }

        public override void SerializeInventoryItem(Stream stream, int itemId, int quantity)
        {

            byte[] buffer;

            buffer = BitConverter.GetBytes(itemId);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(quantity);
            stream.Write(buffer, 0, 4);

        }

        public override void SerializePokedex(Stream stream, PlayerData.Pokedex pokedex)
        {

            byte[] buffer;

            PlayerData.Pokedex.Entry[] entries = pokedex.GetAllSavedEntries();

            //Number of entries
            buffer = BitConverter.GetBytes(entries.Length);
            stream.Write(buffer, 0, 4);
            
            //Entries
            foreach (PlayerData.Pokedex.Entry entry in entries)
            {

                //Species id
                buffer = BitConverter.GetBytes(entry.speciesId);
                stream.Write(buffer, 0, 4);

                //Seen
                buffer = BitConverter.GetBytes(entry.seen);
                stream.Write(buffer, 0, 4);

                //Caught
                buffer = BitConverter.GetBytes(entry.caught);
                stream.Write(buffer, 0, 4);
                
            }

        }

        public override void SerializeString(Stream stream, string s)
        {

            byte[] buffer;

            buffer = BitConverter.GetBytes(s.Length);
            stream.Write(buffer, 0, 4);

            buffer = Encoding.ASCII.GetBytes(s);
            stream.Write(buffer, 0, s.Length);

        }

        public override void SerializeBattleAction(Stream stream, BattleParticipant.Action action)
        {

            byte[] buffer;

            buffer = BitConverter.GetBytes((int)action.type);
            stream.Write(buffer, 0, 4);

            SerializeBool(stream, action.fightUsingStruggle);

            buffer = BitConverter.GetBytes(action.fightMoveIndex);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(action.switchPokemonIndex);
            stream.Write(buffer, 0, 4);

            int itemId = action.useItemItemToUse != null
                ? action.useItemItemToUse.GetId()
                : default;
            buffer = BitConverter.GetBytes(itemId);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(action.useItemTargetPartyIndex);
            stream.Write(buffer, 0, 4);

            buffer = BitConverter.GetBytes(action.useItemTargetMoveIndex);
            stream.Write(buffer, 0, 4);

            SerializeBool(stream, action.useItemDontConsumeItem);

        }

        public override void SerializeSceneStack(Stream stream, GameSceneManager.SceneStack stack)
        {

            List<byte> bytes = new List<byte>();

            string s = stack.AsString;
            byte[] stringBytes = Encoding.ASCII.GetBytes(s);

            bytes.AddRange(BitConverter.GetBytes(stringBytes.Length));
            bytes.AddRange(stringBytes);

            stream.Write(bytes.ToArray(), 0, bytes.Count);

        }

        #endregion

        #region Deserialization

        public override void DeserializeData(Stream stream,
            out long saveTime,
            out PlayerData playerData,
            out GameSettings gameSettings,
            out GameSceneManager.SceneStack sceneStack)
        {

            byte[] buffer;

            Guid playerGuid;
            ulong gameStartTime, distanceWalked, npcsTalkedTo, timePlayed;
            PokemonInstance[] partyPokemon;
            PlayerData.PokemonStorageSystem storageSystemPokemon;
            GameSceneManager.SceneStack respawnSceneStack;
            int money;
            byte spriteId;
            string name;
            List<int> defeatedGymIds, npcsBattled, collectedDroppedItemIds;
            bool respawnSceneStackSet, cheatsUsed;
            Dictionary<int, int> generalItems, medicineItems, battleItems, pokeBallItems, tmItems;
            PlayerData.Pokedex pokedex;
            GameSettings.TextSpeed textSpeed;
            float musicVolume, sfxVolume;

            buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            long fileSignature = BitConverter.ToInt64(buffer, 0);
            if (fileSignature != Serializer.fileSignature)
                throw new ArgumentException("Invalid file signature of provided data");

            buffer = new byte[2];
            stream.Read(buffer, 0, 2);
            ushort saveFileVersion = BitConverter.ToUInt16(buffer, 0);
            if (saveFileVersion != GetVersionCode())
                throw new SerializerVersionMismatchException("Save file version of provided data isn't valid for this serializer", saveFileVersion, GetVersionCode());

            buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            saveTime = BitConverter.ToInt64(buffer, 0);

            buffer = new byte[16];
            stream.Read(buffer, 0, 16);
            playerGuid = new Guid(buffer);

            buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            gameStartTime = BitConverter.ToUInt64(buffer, 0);

            DeserializePlayerPartyAndStorageSystemPokemon(stream,
                out partyPokemon,
                out storageSystemPokemon);

            sceneStack = DeserializeSceneStack(stream);

            respawnSceneStackSet = DeserializeBool(stream);

            if (respawnSceneStackSet)
            {
                respawnSceneStack = DeserializeSceneStack(stream);
            }
            else
            {
                respawnSceneStack = default;
            }

            buffer = new byte[1];
            stream.Read(buffer, 0, 1);
            spriteId = buffer[0];

            name = DeserializeString(stream);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            money = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int numberOfDefeatedGyms = BitConverter.ToInt32(buffer, 0);

            defeatedGymIds = new List<int>();
            for (int i = 0; i < numberOfDefeatedGyms; i++)
            {
                buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                defeatedGymIds.Add(BitConverter.ToInt32(buffer, 0));
            }

            buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            distanceWalked = BitConverter.ToUInt64(buffer, 0);

            buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            npcsTalkedTo = BitConverter.ToUInt64(buffer, 0);

            buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            timePlayed = BitConverter.ToUInt64(buffer, 0);

            cheatsUsed = DeserializeBool(stream);

            generalItems = DeserializeInventorySection(stream);

            medicineItems = DeserializeInventorySection(stream);

            battleItems = DeserializeInventorySection(stream);

            pokeBallItems = DeserializeInventorySection(stream);

            tmItems = DeserializeInventorySection(stream);

            pokedex = DeserializePokedex(stream);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int numberOfNpcsBattled = BitConverter.ToInt32(buffer, 0);

            npcsBattled = new List<int>();
            for (int i = 0; i < numberOfNpcsBattled; i++)
            {
                buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                npcsBattled.Add(BitConverter.ToInt32(buffer, 0));
            }

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int textSpeedIndex = BitConverter.ToInt32(buffer, 0);
            textSpeed = GameSettings.textSpeedOptions[textSpeedIndex];

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            musicVolume = BitConverter.ToSingle(buffer, 0);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            sfxVolume = BitConverter.ToSingle(buffer, 0);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int numberOfCollectedDroppedItemIds = BitConverter.ToInt32(buffer, 0);

            collectedDroppedItemIds = new List<int>();
            for (int i = 0; i < numberOfCollectedDroppedItemIds; i++)
            {
                buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                collectedDroppedItemIds.Add(BitConverter.ToInt32(buffer, 0));
            }

            //Setting output
            playerData = new PlayerData()
            {
                partyPokemon = partyPokemon,
                boxPokemon = storageSystemPokemon,
                profile = new PlayerData.Profile()
                {
                    guid = playerGuid,
                    name = name,
                    spriteId = spriteId,
                    money = money,
                    defeatedGymIds = defeatedGymIds
                },
                stats = new PlayerData.Stats()
                {
                    gameStartTime = gameStartTime,
                    distanceWalked = distanceWalked,
                    npcsTalkedTo = npcsTalkedTo,
                    timePlayed = timePlayed,
                    cheatsUsed = cheatsUsed
                },
                inventory = new PlayerData.Inventory(),
                pokedex = pokedex,
                npcsBattled = npcsBattled,
                respawnSceneStackSet = respawnSceneStackSet,
                respawnSceneStack = respawnSceneStack,
                collectedDroppedItemsIds = collectedDroppedItemIds
            };

            playerData.inventory.generalItems.SetItems(generalItems);
            playerData.inventory.medicineItems.SetItems(medicineItems);
            playerData.inventory.battleItems.SetItems(battleItems);
            playerData.inventory.pokeBalls.SetItems(pokeBallItems);
            playerData.inventory.tmItems.SetItems(tmItems);

            gameSettings = new GameSettings()
            {
                textSpeed = textSpeed,
                musicVolume = musicVolume,
                sfxVolume = sfxVolume
            };

        }

        public override void DeserializePlayerPartyAndStorageSystemPokemon(Stream stream, out PokemonInstance[] partyPokemon, out PlayerData.PokemonStorageSystem storageSystem)
        {

            byte[] buffer;

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int pokemonCount = BitConverter.ToInt32(buffer, 0);

            //Pokemon list

            List<PokemonInstance> pokemonList = new List<PokemonInstance>();

            for (int i = 0; i < pokemonCount; i++)
            {
                PokemonInstance newPokemon = DeserializePokemonInstance(stream);
                pokemonList.Add(newPokemon);
            }

            //Party pokemon

            partyPokemon = new PokemonInstance[PlayerData.partyCapacity];

            for (int i = 0; i < PlayerData.partyCapacity; i++)
            {

                buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                int pokeIndex = BitConverter.ToInt32(buffer, 0);

                if (pokeIndex < 0)
                {
                    partyPokemon[i] = null;
                }
                else
                {
                    partyPokemon[i] = pokemonList[pokeIndex];
                }

            }

            //Storage system

            PokemonInstance[][] boxes = new PokemonInstance[PlayerData.PokemonStorageSystem.boxCount][];

            for (int boxIndex = 0; boxIndex < PlayerData.PokemonStorageSystem.boxCount; boxIndex++)
            {

                PokemonInstance[] box = new PokemonInstance[PlayerData.PokemonBox.size];

                for (int slotIndex = 0; slotIndex < PlayerData.PokemonBox.size; slotIndex++)
                {

                    buffer = new byte[4];
                    stream.Read(buffer, 0, 4);
                    int pokeIndex = BitConverter.ToInt32(buffer, 0);

                    if (pokeIndex < 0)
                    {
                        box[slotIndex] = null;
                    }
                    else
                    {
                        box[slotIndex] = pokemonList[pokeIndex];
                    }

                }

                boxes[boxIndex] = box;

            }

            storageSystem = new PlayerData.PokemonStorageSystem(boxes);

        }

        public override PokemonInstance DeserializePokemonInstance(Stream stream)
        {

            byte[] buffer;

            int speciesId, natureId, experience, health;
            Guid guid, originalTrainerGuid;
            string nickname, originalTrainerName;
            Item heldItem;
            bool? gender;
            int pokeBallId;
            long catchTime;
            Stats<byte> effortValues, individualValues;
            Stats<int> currentStats;
            byte friendship;
            int[] moveIds;
            byte[] movePPs;
            PokemonInstance.NonVolatileStatusCondition nonVolatileStatusCondition;

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            speciesId = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[16];
            stream.Read(buffer, 0, 16);
            guid = new Guid(buffer);

            nickname = DeserializeString(stream);

            heldItem = DeserializeItem(stream);

            gender = DeserializeNullableBool(stream);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            pokeBallId = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            catchTime = BitConverter.ToInt64(buffer, 0);

            originalTrainerName = DeserializeString(stream);

            buffer = new byte[16];
            stream.Read(buffer, 0, 16);
            originalTrainerGuid = new Guid(buffer);

            effortValues = DeserializeByteStats(stream);

            individualValues = DeserializeByteStats(stream);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            natureId = BitConverter.ToInt32(buffer, 0);

            currentStats = DeserializeIntStats(stream);

            buffer = new byte[1];
            stream.Read(buffer, 0, 1);
            friendship = buffer[0];

            moveIds = new int[4];
            for (int i = 0; i < 4; i++)
            {
                buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                moveIds[i] = BitConverter.ToInt32(buffer, 0);
            }

            movePPs = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                buffer = new byte[1];
                stream.Read(buffer, 0, 1);
                movePPs[i] = buffer[0];
            }

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            experience = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            nonVolatileStatusCondition = (PokemonInstance.NonVolatileStatusCondition)BitConverter.ToInt32(buffer, 0);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            health = BitConverter.ToInt32(buffer, 0);

            return PokemonFactory.GenerateFull(
                speciesId: speciesId,
                natureId: natureId,
                effortValues: effortValues,
                individualValues: individualValues,
                _moves: moveIds,
                movePPs: movePPs,
                experience: experience,
                nonVolatileStatusCondition: nonVolatileStatusCondition,
                null,
                _guid: guid,
                nickname: nickname,
                heldItem: heldItem,
                _health: health,
                gender: gender,
                currentStats: currentStats,
                pokeBallId: pokeBallId,
                originalTrainerName: originalTrainerName,
                _originalTrainerGuid: originalTrainerGuid,
                catchTime: catchTime,
                _friendship: friendship);

        }

        public override Dictionary<int, int> DeserializeInventorySection(Stream stream)
        {

            byte[] buffer;

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int count = BitConverter.ToInt32(buffer, 0);

            Dictionary<int, int> entries = new Dictionary<int, int>();

            for (int i = 0; i < count; i++)
            {

                DeserializeInventoryItem(stream, out int itemId, out int quantity);

                entries.Add(itemId, quantity);

            }

            return entries;

        }

        public override void DeserializeInventoryItem(Stream stream, out int itemId, out int quantity)
        {

            byte[] buffer;

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            itemId = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            quantity = BitConverter.ToInt32(buffer, 0);

        }

        public override PlayerData.Pokedex DeserializePokedex(Stream stream)
        {

            byte[] buffer;

            PlayerData.Pokedex pokedex;
            int entryCount;
            PlayerData.Pokedex.Entry[] entries;

            //Number of entries
            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            entryCount = BitConverter.ToInt32(buffer, 0);
            
            entries = new PlayerData.Pokedex.Entry[entryCount];
            
            //Entries
            for (int i = 0; i < entryCount; i++)
            {

                int speciesId, seen, caught;

                //Species id
                buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                speciesId = BitConverter.ToInt32(buffer, 0);

                //Seen
                buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                seen = BitConverter.ToInt32(buffer, 0);

                //Caught
                buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                caught = BitConverter.ToInt32(buffer, 0);

                //Add entry
                entries[i] = new PlayerData.Pokedex.Entry(speciesId, seen, caught);

            }

            pokedex = new PlayerData.Pokedex(entries);

            return pokedex;

        }

        public override string DeserializeString(Stream stream)
        {

            byte[] buffer;

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int stringLength = BitConverter.ToInt32(buffer, 0);

            string s;
            if (stringLength > 0)
            {
                buffer = new byte[stringLength];
                stream.Read(buffer, 0, stringLength);
                s = Encoding.ASCII.GetString(buffer, 0, stringLength);
            }
            else
            {
                s = "";
            }

            return s;

        }

        public override BattleParticipant.Action DeserializeBattleAction(Stream stream,
            BattleParticipant user,
            BattleParticipant target)
        {

            BattleParticipant.Action.Type type;
            bool fightUsingStruggle, useItemDontConsumeItem;
            int fightMoveIndex, switchPokemonIndex, useItemTargetPartyIndex, useItemTargetMoveIndex;
            Item useItemItemToUse;

            byte[] buffer;

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            type = (BattleParticipant.Action.Type)BitConverter.ToInt32(buffer, 0);

            fightUsingStruggle = DeserializeBool(stream);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            fightMoveIndex = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            switchPokemonIndex = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            useItemItemToUse = Item.GetItemById(BitConverter.ToInt32(buffer, 0));

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            useItemTargetPartyIndex = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            useItemTargetMoveIndex = BitConverter.ToInt32(buffer, 0);

            useItemDontConsumeItem = DeserializeBool(stream);

            return new BattleParticipant.Action(user)
            {

                type = type,
                fightUsingStruggle = fightUsingStruggle,
                fightMoveIndex = fightMoveIndex,
                switchPokemonIndex = switchPokemonIndex,
                useItemItemToUse = useItemItemToUse,
                useItemTargetPartyIndex = useItemTargetPartyIndex,
                useItemTargetMoveIndex = useItemTargetMoveIndex,
                useItemDontConsumeItem = useItemDontConsumeItem,

                fightMoveTarget = target,
                useItemPokeBallTarget = target

            };

        }

        public override GameSceneManager.SceneStack DeserializeSceneStack(Stream stream)
        {

            byte[] buffer;

            buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            int stringLength = BitConverter.ToInt32(buffer, 0);

            buffer = new byte[stringLength];
            stream.Read(buffer, 0, stringLength);
            string sceneStackString = Encoding.ASCII.GetString(buffer, 0, stringLength);

            if (!GameSceneManager.SceneStack.TryParse(sceneStackString,
                out GameSceneManager.SceneStack sceneStack,
                out string errMsg))
            {
                throw new ArgumentException("Unable to parse scene stack string when deserializing:\n" + errMsg);
            }

            return sceneStack;

        }

        #endregion

    }
}
