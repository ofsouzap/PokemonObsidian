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
                && Pokemon.Moves.PokemonMove.registry != null
                && Weather.registry != null
                && (LoadGameCharacterSprites.singleton?.loaded == true )
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
                        opponentPokemon.Add(PokemonFactory.GenerateWild(
                            new int[] { spec.speciesId },
                            spec.level,
                            spec.level
                            ));
                    }

                    BattleEntranceArguments.npcTrainerBattleArguments.opponentPokemon = opponentPokemon.ToArray();
                    BattleEntranceArguments.npcTrainerBattleArguments.opponentFullName = npcTrainerFullName;
                    BattleEntranceArguments.npcTrainerBattleArguments.opponentSpriteResourcePath = npcTrainerSpriteName;
                    BattleEntranceArguments.npcTrainerBattleArguments.mode = npcTrainerMode;

                    break;

            }

            battleManager.StartBattle();

        }

    }
}

#endif