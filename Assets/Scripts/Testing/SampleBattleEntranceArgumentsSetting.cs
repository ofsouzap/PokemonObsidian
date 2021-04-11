using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using Pokemon;

#if UNITY_EDITOR

namespace Testing
{
    public class SampleBattleEntranceArgumentsSetting : MonoBehaviour
    {

        public BattleManager battleManager;

        [Serializable]
        public struct PokemonSpecification
        {
            public int speciesId;
            public byte level;
            public PokemonInstance.NonVolatileStatusCondition initialNVSC;
        }

        public BattleType battleType;
        public int initialWeatherId = 0;

        [Header("Wild Pokemon Arguments")]
        public PokemonSpecification wildPokemonSpecification;

        [Header("NPC Trainer Arguments")]
        public PokemonSpecification[] npcTrainerPokemonSpecifications;
        public string npcTrainerFullName;
        public string npcTrainerSpriteName;
        public BattleParticipantNPC.Mode npcTrainerMode;

        private void Awake()
        {
            StartCoroutine(InitialiseCoroutine());
        }

        private IEnumerator InitialiseCoroutine()
        {

            yield return new WaitUntil(() =>
                PokemonSpecies.registry != null
                && Nature.registry != null
                && Pokemon.Moves.PokemonMove.registry != null && Pokemon.Moves.PokemonMove.registry.Length > 0
                && Weather.registry != null
                && LoadAllSprites.singleton != null && LoadAllSprites.singleton.loaded
                && LoadItems.singleton != null && LoadItems.singleton.loaded
                );

            BattleEntranceArguments.argumentsSet = true;
            BattleEntranceArguments.battleType = battleType;
            BattleEntranceArguments.initialWeatherId = initialWeatherId;

            switch (battleType)
            {

                case BattleType.WildPokemon:

                    BattleEntranceArguments.wildPokemonBattleArguments.opponentInstance = PokemonFactory.GenerateWild(
                        new int[] { wildPokemonSpecification.speciesId },
                        wildPokemonSpecification.level,
                        wildPokemonSpecification.level
                        );

                    break;

                case BattleType.NPCTrainer:

                    if (npcTrainerPokemonSpecifications.Length > 6 || npcTrainerPokemonSpecifications.Length <= 0)
                    {
                        Debug.LogError("Invalid number of pokemon specifications");
                        BattleEntranceArguments.argumentsSet = false;
                        break;
                    }

                    List<PokemonInstance> opponentPokemon = new List<PokemonInstance>();
                    foreach (PokemonSpecification spec in npcTrainerPokemonSpecifications)
                    {
                        PokemonInstance pokemon = PokemonFactory.GenerateWild(
                            new int[] { spec.speciesId },
                            spec.level,
                            spec.level
                            );
                        pokemon.nonVolatileStatusCondition = spec.initialNVSC;
                        opponentPokemon.Add(pokemon);
                    }

                    PokemonInstance[] opponentPokemonArray = new PokemonInstance[6];
                    Array.Copy(opponentPokemon.ToArray(),
                        opponentPokemonArray,
                        opponentPokemon.Count);
                    
                    BattleEntranceArguments.npcTrainerBattleArguments.opponentPokemon = opponentPokemonArray;
                    BattleEntranceArguments.npcTrainerBattleArguments.opponentFullName = npcTrainerFullName;
                    BattleEntranceArguments.npcTrainerBattleArguments.opponentSpriteResourceName = npcTrainerSpriteName;
                    BattleEntranceArguments.npcTrainerBattleArguments.mode = npcTrainerMode;

                    break;

            }

            battleManager.StartBattle();

        }

    }
}

#endif