using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;

namespace Testing
{
    public class SetRandomBoxPokemon : MonoBehaviour
    {

        public int maxSpeciesId;

        [Range(1, 100)]
        public byte minPokemonLevel;
        [Range(1, 100)]
        public byte maxPokemonLevel;

        [Range(0, PlayerData.PokemonStorageSystem.boxCount)]
        public byte minBoxUsedCount;
        [Range(0, PlayerData.PokemonStorageSystem.boxCount)]
        public byte maxBoxUsedCount;

        [Range(0, PlayerData.PokemonBox.size)]
        public byte minBoxPokemonCount;
        [Range(0, PlayerData.PokemonBox.size)]
        public byte maxBoxPokemonCount;

        private int[] possibleSpeciesIds;

        private void Start()
        {

            SetPossibleSpeciesIds();

            SetBoxPokemon();

        }

        private void SetBoxPokemon()
        {

            byte usedBoxCount = (byte)Random.Range(minBoxUsedCount, maxBoxUsedCount + 1);

            for (byte boxIndex = 0; boxIndex < usedBoxCount; boxIndex++)
            {

                byte boxPokemonCount = (byte)Random.Range(minBoxPokemonCount, maxBoxPokemonCount);

                for (int pmonIndex = 0; pmonIndex < boxPokemonCount; pmonIndex++)
                {
                    PlayerData.singleton.boxPokemon.boxes[boxIndex].AddPokemon(GeneratePokemon());
                }

            }

        }

        private void SetPossibleSpeciesIds()
        {

            List<int> possibleSpeciesIdsList = new List<int>();
            for (int i = 1; i <= maxSpeciesId; i++)
                possibleSpeciesIdsList.Add(i);
            possibleSpeciesIds = possibleSpeciesIdsList.ToArray();

        }

        private PokemonInstance GeneratePokemon()
        {
            return PokemonFactory.GenerateWild(possibleSpeciesIds, minPokemonLevel, maxPokemonLevel);
        }

    }
}
