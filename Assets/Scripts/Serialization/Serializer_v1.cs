using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pokemon;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;

namespace Serialization
{
    public class Serializer_v1 : Serializer
    {

        protected override ushort GetVersionCode()
            => 0x0001;

        #region Serialization

        public override byte[] SerializeData(PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            List<byte> bytes = new List<byte>();

            //Save details
            bytes.AddRange(fileSignatureBytes);
            bytes.AddRange(BitConverter.GetBytes(GetVersionCode()));
            bytes.AddRange(BitConverter.GetBytes(EpochTime.SecondsNow));

            //Player
            bytes.AddRange(player.profile.guid.ToByteArray());
            bytes.AddRange(BitConverter.GetBytes(player.stats.gameStartTime));

            //Pokemon
            bytes.AddRange(SerializePlayerPartyAndStorageSystemPokemon(player));

            //Scene stacks
            bytes.AddRange(SerializeSceneStack(GameSceneManager.CurrentSceneStack));
            bytes.Add(SerializeBool(player.respawnSceneStackSet));
            if (player.respawnSceneStackSet)
                bytes.AddRange(SerializeSceneStack(player.respawnSceneStack));

            //Profile Details
            bytes.Add(player.profile.spriteId);
            bytes.AddRange(BitConverter.GetBytes(SerializeString(player.profile.name, out byte[] nameBytes)));
            bytes.AddRange(nameBytes);
            bytes.AddRange(BitConverter.GetBytes(player.profile.money));

            //Defeated gyms
            bytes.AddRange(BitConverter.GetBytes(player.profile.defeatedGymIds.Count));
            foreach (int gymId in player.profile.defeatedGymIds)
                bytes.AddRange(BitConverter.GetBytes(gymId));

            //Stats
            bytes.AddRange(BitConverter.GetBytes(player.stats.distanceWalked));
            bytes.AddRange(BitConverter.GetBytes(player.stats.npcsTalkedTo));
            bytes.AddRange(BitConverter.GetBytes(player.stats.timePlayed));
            bytes.Add(SerializeBool(player.stats.cheatsUsed));

            //Inventory
            bytes.AddRange(SerializeInventorySection(player.inventory.generalItems));
            bytes.AddRange(SerializeInventorySection(player.inventory.medicineItems));
            bytes.AddRange(SerializeInventorySection(player.inventory.battleItems));
            bytes.AddRange(SerializeInventorySection(player.inventory.pokeBalls));
            bytes.AddRange(SerializeInventorySection(player.inventory.tmItems));

            //NPCs battled
            bytes.AddRange(BitConverter.GetBytes(player.npcsBattled.Count));
            foreach (int npcId in player.npcsBattled)
                bytes.AddRange(BitConverter.GetBytes(npcId));

            //Settings
            bytes.AddRange(BitConverter.GetBytes(Array.IndexOf(
                GameSettings.textSpeedOptions, GameSettings.singleton.textSpeed
                )));
            bytes.AddRange(BitConverter.GetBytes(GameSettings.singleton.musicVolume));
            bytes.AddRange(BitConverter.GetBytes(GameSettings.singleton.sfxVolume));

            //Return output
            return bytes.ToArray();

        }

        protected override byte[] SerializePlayerPartyAndStorageSystemPokemon(PlayerData player = null)
        {

            List<byte> bytes = new List<byte>();

            if (player == null)
                player = PlayerData.singleton;

            int pokemonListIndex = 0;
            List<byte> pokemonBytes = new List<byte>();

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
                    pokemonBytes.AddRange(SerializePokemonInstance(pokemon));
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
                        pokemonBytes.AddRange(SerializePokemonInstance(pokemon));
                    }

                }
            }

            #endregion

            bytes.AddRange(BitConverter.GetBytes(pokemonListIndex)); //This is the number of serialized pokemon
            bytes.AddRange(pokemonBytes);

            foreach (int value in partyPokemonIndexes)
                bytes.AddRange(BitConverter.GetBytes(value));

            foreach (int value in storageSystemPokemonIndexes)
                bytes.AddRange(BitConverter.GetBytes(value));

            return bytes.ToArray();

        }

        protected override byte[] SerializePokemonInstance(PokemonInstance pokemon)
        {

            List<byte> bytes = new List<byte>();

            //Species
            bytes.AddRange(BitConverter.GetBytes(pokemon.speciesId));

            //Details
            bytes.AddRange(pokemon.guid.ToByteArray());
            bytes.AddRange(BitConverter.GetBytes(SerializeString(pokemon.nickname, out byte[] nicknameBytes)));
            bytes.AddRange(nicknameBytes);
            bytes.AddRange(SerializeHeldItem(pokemon));
            bytes.Add(SerializeNullableBool(pokemon.gender));

            //Catch
            bytes.AddRange(BitConverter.GetBytes(pokemon.pokeBallId));
            bytes.AddRange(BitConverter.GetBytes(pokemon.catchTime));

            //OT
            bytes.AddRange(BitConverter.GetBytes(SerializeString(pokemon.originalTrainerName, out byte[] otNameBytes)));
            bytes.AddRange(otNameBytes);
            bytes.AddRange(pokemon.originalTrainerGuid.ToByteArray());

            //Stats
            bytes.AddRange(SerializeByteStats(pokemon.effortValues));
            bytes.AddRange(SerializeByteStats(pokemon.individualValues));
            bytes.AddRange(BitConverter.GetBytes(pokemon.natureId));
            bytes.AddRange(SerializeIntStats(pokemon.GetStats()));
            bytes.Add(pokemon.friendship);

            //Moves
            bytes.AddRange(BitConverter.GetBytes(pokemon.moveIds[0]));
            bytes.AddRange(BitConverter.GetBytes(pokemon.moveIds[1]));
            bytes.AddRange(BitConverter.GetBytes(pokemon.moveIds[2]));
            bytes.AddRange(BitConverter.GetBytes(pokemon.moveIds[3]));
            bytes.AddRange(pokemon.movePPs);

            bytes.AddRange(BitConverter.GetBytes(pokemon.experience));
            bytes.AddRange(BitConverter.GetBytes((int)pokemon.nonVolatileStatusCondition));
            bytes.AddRange(BitConverter.GetBytes(pokemon.health));

            return bytes.ToArray();

        }

        protected override byte[] SerializeInventorySection(PlayerData.Inventory.InventorySection section)
        {

            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(section.ItemQuantites.Count));

            foreach (Item item in section.ItemQuantites.Keys)
                bytes.AddRange(SerializeInventoryItem(item.GetId(), section.ItemQuantites[item]));

            return bytes.ToArray();
            
        }

        protected override byte[] SerializeInventoryItem(int itemId, int quantity)
        {

            List<byte> bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(itemId));
            bytes.AddRange(BitConverter.GetBytes(quantity));

            return bytes.ToArray();

        }

        protected override int SerializeString(string s,
            out byte[] bytes)
        {

            bytes = Encoding.ASCII.GetBytes(s);
            return bytes.Length;

        }

        protected override byte[] SerializeSceneStack(GameSceneManager.SceneStack stack)
        {

            List<byte> bytes = new List<byte>();

            string s = stack.AsString;
            byte[] stringBytes = Encoding.ASCII.GetBytes(s);

            bytes.AddRange(BitConverter.GetBytes(stringBytes.Length));
            bytes.AddRange(stringBytes);

            return bytes.ToArray();

        }

        #endregion

        #region Deserialization

        public override void DeserializeData(byte[] data,
            int startOffset,
            out long saveTime,
            out PlayerData playerData,
            out GameSettings gameSettings,
            out GameSceneManager.SceneStack sceneStack,
            out int byteLength)
        {

            Guid playerGuid;
            ulong gameStartTime, distanceWalked, npcsTalkedTo, timePlayed;
            PokemonInstance[] partyPokemon;
            PlayerData.PokemonStorageSystem storageSystemPokemon;
            GameSceneManager.SceneStack respawnSceneStack;
            int money;
            byte spriteId;
            string name;
            List<int> defeatedGymIds, npcsBattled;
            bool respawnSceneStackSet, cheatsUsed;
            Dictionary<int, int> generalItems, medicineItems, battleItems, pokeBallItems, tmItems;
            GameSettings.TextSpeed textSpeed;
            float musicVolume, sfxVolume;

            int offset = startOffset;

            long fileSignature = BitConverter.ToInt64(data, offset);
            offset += 8;
            if (fileSignature != Serializer.fileSignature)
                throw new ArgumentException("Invalid file signature of provided data");

            ushort saveFileVersion = BitConverter.ToUInt16(data, offset);
            offset += 2;
            if (saveFileVersion != GetVersionCode())
                throw new ArgumentException("Save file version of provided data isn't valid for this serializer");

            saveTime = BitConverter.ToInt64(data, offset);
            offset += 8;

            byte[] playerGuidBytes = new byte[16];
            Array.Copy(data, offset, playerGuidBytes, 0, 16);
            offset += 16;
            playerGuid = new Guid(playerGuidBytes);

            gameStartTime = BitConverter.ToUInt64(data, offset);
            offset += 8;

            DeserializePlayerPartyAndStorageSystemPokemon(data, offset,
                out partyPokemon,
                out storageSystemPokemon,
                out int storedPokemonByteLength);
            offset += storedPokemonByteLength;

            sceneStack = DeserializeSceneStack(data, offset, out int sceneStackByteLength);
            offset += sceneStackByteLength;

            respawnSceneStackSet = DeserializeBool(data[offset]);
            offset += 1;

            if (respawnSceneStackSet)
            {
                respawnSceneStack = DeserializeSceneStack(data, offset, out int respawnSceneStackByteLength);
                offset += respawnSceneStackByteLength;
            }
            else
            {
                respawnSceneStack = default;
            }

            spriteId = data[offset];
            offset += 1;

            name = DeserializeString(data, offset, out int nameByteLength);
            offset += nameByteLength;

            money = BitConverter.ToInt32(data, offset);
            offset += 4;

            int numberOfDefeatedGyms = BitConverter.ToInt32(data, offset);
            offset += 4;

            defeatedGymIds = new List<int>();
            for (int i = 0; i < numberOfDefeatedGyms; i++)
            {
                defeatedGymIds.Add(BitConverter.ToInt32(data, offset));
                offset += 4;
            }

            distanceWalked = BitConverter.ToUInt64(data, offset);
            offset += 8;

            npcsTalkedTo = BitConverter.ToUInt64(data, offset);
            offset += 8;

            timePlayed = BitConverter.ToUInt64(data, offset);
            offset += 8;

            cheatsUsed = DeserializeBool(data[offset]);
            offset += 1;

            generalItems = DeserializeInventorySection(data, offset, out int generalItemsByteLength);
            offset += generalItemsByteLength;

            medicineItems = DeserializeInventorySection(data, offset, out int medicineItemsByteLength);
            offset += medicineItemsByteLength;

            battleItems = DeserializeInventorySection(data, offset, out int battleItemsByteLength);
            offset += battleItemsByteLength;

            pokeBallItems = DeserializeInventorySection(data, offset, out int pokeBallItemsByteLength);
            offset += pokeBallItemsByteLength;

            tmItems = DeserializeInventorySection(data, offset, out int tmItemsByteLength);
            offset += tmItemsByteLength;

            int numberOfNpcsBattled = BitConverter.ToInt32(data, offset);
            offset += 4;

            npcsBattled = new List<int>();
            for (int i = 0; i < numberOfNpcsBattled; i++)
            {
                npcsBattled.Add(BitConverter.ToInt32(data, offset));
                offset += 4;
            }

            textSpeed = GameSettings.textSpeedOptions[BitConverter.ToInt32(data, offset)];
            offset += 4;

            musicVolume = BitConverter.ToSingle(data, offset);
            offset += 4;

            sfxVolume = BitConverter.ToSingle(data, offset);
            offset += 4;

            //Setting byte length
            byteLength = offset - startOffset;

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
                npcsBattled = npcsBattled,
                respawnSceneStackSet = respawnSceneStackSet,
                respawnSceneStack = respawnSceneStack
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

        protected override void DeserializePlayerPartyAndStorageSystemPokemon(byte[] data, int startOffset, out PokemonInstance[] partyPokemon, out PlayerData.PokemonStorageSystem storageSystem, out int byteLength)
        {

            int offset = startOffset;

            int pokemonCount = BitConverter.ToInt32(data, offset);
            offset += 4;

            //Pokemon list

            List<PokemonInstance> pokemonList = new List<PokemonInstance>();

            for (int i = 0; i < pokemonCount; i++)
            {
                PokemonInstance newPokemon = DeserializePokemonInstance(data, offset, out int newPokemonByteLength);
                offset += newPokemonByteLength;
                pokemonList.Add(newPokemon);
            }

            //Party pokemon

            partyPokemon = new PokemonInstance[PlayerData.partyCapacity];

            for (int i = 0; i < PlayerData.partyCapacity; i++)
            {

                int pokeIndex = BitConverter.ToInt32(data, offset);
                offset += 4;

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

                    int pokeIndex = BitConverter.ToInt32(data, offset);
                    offset += 4;

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

            //Return outputs
            byteLength = offset - startOffset;

        }

        protected override PokemonInstance DeserializePokemonInstance(byte[] data, int startOffset, out int byteLength)
        {

            int offset = startOffset;

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

            speciesId = BitConverter.ToInt32(data, offset);
            offset += 4;

            byte[] guidByteArray = new byte[16];
            Array.Copy(data, offset, guidByteArray, 0, 16);
            offset += 16;
            guid = new Guid(guidByteArray);

            nickname = DeserializeString(data, offset, out int nicknameByteLength);
            offset += nicknameByteLength;

            heldItem = DeserializeItem(data, offset, out int heldItemByteLength);
            offset += heldItemByteLength;

            gender = DeserializeNullableBool(data[offset]);
            offset += 1;

            pokeBallId = BitConverter.ToInt32(data, offset);
            offset += 4;

            catchTime = BitConverter.ToInt64(data, offset);
            offset += 8;

            originalTrainerName = DeserializeString(data, offset, out int otNameByteLength);
            offset += otNameByteLength;

            byte[] otGuidByteArray = new byte[16];
            Array.Copy(data, offset, otGuidByteArray, 0, 16);
            offset += 16;
            originalTrainerGuid = new Guid(otGuidByteArray);

            effortValues = DeserializeByteStats(data, offset);
            offset += 6;

            individualValues = DeserializeByteStats(data, offset);
            offset += 6;

            natureId = BitConverter.ToInt32(data, offset);
            offset += 4;

            currentStats = DeserializeIntStats(data, offset);
            offset += 24;

            friendship = data[offset];
            offset += 1;

            moveIds = new int[4];
            for (int i = 0; i < 4; i++)
            {
                moveIds[i] = BitConverter.ToInt32(data, offset);
                offset += 4;
            }

            movePPs = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                movePPs[i] = data[offset];
                offset += 1;
            }

            experience = BitConverter.ToInt32(data, offset);
            offset += 4;

            nonVolatileStatusCondition = (PokemonInstance.NonVolatileStatusCondition)BitConverter.ToInt32(data, offset);
            offset += 4;

            health = BitConverter.ToInt32(data, offset);
            offset += 4;

            byteLength = offset - startOffset;
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

        protected override Dictionary<int, int> DeserializeInventorySection(byte[] data, int startOffset, out int byteLength)
        {

            int offset = startOffset;

            int count = BitConverter.ToInt32(data, offset);
            offset += 4;

            Dictionary<int, int> entries = new Dictionary<int, int>();

            for (int i = 0; i < count; i++)
            {

                DeserializeInventoryItem(data, offset, out int itemId, out int quantity, out int itemByteLength);
                offset += itemByteLength;

                entries.Add(itemId, quantity);

            }

            byteLength = offset - startOffset;
            return entries;

        }

        protected override void DeserializeInventoryItem(byte[] data, int startOffset, out int itemId, out int quantity, out int byteLength)
        {

            int offset = startOffset;

            itemId = BitConverter.ToInt32(data, offset);
            offset += 4;

            quantity = BitConverter.ToInt32(data, offset);
            offset += 4;

            byteLength = offset - startOffset;

        }

        protected override string DeserializeString(byte[] data, int startOffset, out int byteLength)
        {

            int offset = startOffset;

            int stringLength = BitConverter.ToInt32(data, offset);
            offset += 4;

            string s;
            if (stringLength > 0)
            {
                s = Encoding.ASCII.GetString(data, offset, stringLength);
                offset += stringLength;
            }
            else
            {
                s = "";
            }

            byteLength = offset - startOffset;
            return s;

        }

        protected override GameSceneManager.SceneStack DeserializeSceneStack(byte[] data, int startOffset, out int byteLength)
        {

            int offset = startOffset;

            int stringLength = BitConverter.ToInt32(data, offset);
            offset += 4;

            string sceneStackString = Encoding.ASCII.GetString(data, offset, stringLength);
            offset += stringLength;

            if (!GameSceneManager.SceneStack.TryParse(sceneStackString,
                out GameSceneManager.SceneStack sceneStack,
                out string errMsg))
            {
                throw new ArgumentException("Unable to parse scene stack string when deserializing:\n" + errMsg);
            }

            byteLength = offset - startOffset;
            return sceneStack;

        }

        #endregion

    }
}
