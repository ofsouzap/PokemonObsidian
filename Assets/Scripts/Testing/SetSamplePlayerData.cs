using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pokemon;
using Items;

namespace Testing
{
    public class SetSamplePlayerData : MonoBehaviour
    {

        [Serializable]
        public struct PokemonSpecification
        {
            public int speciesId;
            public byte level;
            [Tooltip("The proportion of the way the pokemon is to the next level in experience")]
            [Range(0,0.9999F)]
            public float levelExperience;
            public PokemonInstance.NonVolatileStatusCondition initialNVSC;
        }

        public PokemonSpecification[] pokemon;

        private void Start()
        {

            #region Pokemon

            if (pokemon.Length > 6 || pokemon.Length <= 0)
            {
                Debug.LogError("Invalid number of pokemon specifications");
            }

            List<PokemonInstance> playerPokemon = new List<PokemonInstance>();

            foreach (PokemonSpecification spec in pokemon)
            {

                PokemonInstance pokemon = PokemonFactory.GenerateWild(
                    new int[] { spec.speciesId },
                    spec.level,
                    spec.level
                    );

                pokemon.nonVolatileStatusCondition = spec.initialNVSC;
                pokemon.originalTrainerName = PlayerData.singleton.profile.name;
                pokemon.originalTrainerGuid = PlayerData.singleton.profile.guid;

                if (pokemon.GetLevel() < 100)
                {
                    pokemon.experience = Mathf.FloorToInt(Mathf.Lerp(
                        GrowthTypeData.GetMinimumExperienceForLevel(pokemon.GetLevel(), pokemon.growthType),
                        GrowthTypeData.GetMinimumExperienceForLevel((byte)(pokemon.GetLevel() + 1), pokemon.growthType),
                        spec.levelExperience
                    ));
                }

                playerPokemon.Add(pokemon);

            }

            PlayerData.singleton.partyPokemon = playerPokemon.ToArray();

            #endregion

            #region Items

            PlayerData.singleton.inventory.battleItems.SetItems(new Dictionary<int, int>()
            {
                { Item.typeIdBattleItem + 56, 1 },
                { Item.typeIdBattleItem + 57, 1 },
                { Item.typeIdBattleItem + 58, 102 },
                { Item.typeIdBattleItem + 59, 10 },
                { Item.typeIdBattleItem + 60, 4 },
                { Item.typeIdBattleItem + 61, 1 },
                { Item.typeIdBattleItem + 62, 3 }
            });

            PlayerData.singleton.inventory.medicineItems.SetItems(new Dictionary<int, int>()
            {
                { Item.typeIdMedicine + 17, 1 },
                { Item.typeIdMedicine + 18, 102 },
                { Item.typeIdMedicine + 19, 10 },
                { Item.typeIdMedicine + 20, 4 },
                { Item.typeIdMedicine + 21, 3 },
                { Item.typeIdMedicine + 22, 4 },
                { Item.typeIdMedicine + 29, 1 },
                { Item.typeIdMedicine + 24, 1 },
                { Item.typeIdMedicine + 28, 2 },
                { Item.typeIdMedicine + 25, 2 },
            });

            PlayerData.singleton.inventory.pokeBalls.SetItems(new Dictionary<int, int>()
            {
                { Item.typeIdPokeBall + 2, 14 },
                { Item.typeIdPokeBall + 3, 3 },
                { Item.typeIdPokeBall + 4, 7 },
                { Item.typeIdPokeBall + 6, 10 },
                { Item.typeIdPokeBall + 1, 10 },
            });

            PlayerData.singleton.inventory.tmItems.SetItems(new Dictionary<int, int>()
            {
                { Item.typeIdTM + 1, 3 },
                { Item.typeIdTM + 39, 1 },
                { Item.typeIdTM + 55, 20 },
                { Item.typeIdTM + 72, 5 },
                { Item.typeIdTM + 24, 5 },
                { Item.typeIdTM + 45, 2 },
                { Item.typeIdTM + 95, 2 },
                { Item.typeIdTM + 101, 2 },
                { Item.typeIdTM + 85, 2 },
            });

            PlayerData.singleton.inventory.generalItems.SetItems(new Dictionary<int, int>()
            {
                { Item.typeIdGeneral + 80, 3 },
                { Item.typeIdGeneral + 81, 3 },
                { Item.typeIdGeneral + 82, 3 },
                { Item.typeIdGeneral + 83, 3 },
                { Item.typeIdGeneral + 84, 3 },
                { Item.typeIdGeneral + 85, 3 },
                { Item.typeIdGeneral + 108, 3 },
                { Item.typeIdGeneral + 109, 3 },
            });

            #endregion

            PlayerData.singleton.profile.name = "Test";

        }

    }
}
