using System.Collections;
using UnityEngine;
using Pokemon;
using Battle;
using FreeRoaming.NPCs;
using FreeRoaming;

#if UNITY_EDITOR

namespace Testing
{
    public class WildPokemonBattleLaunchNPC : NPCController
    {

        public string battleBackgroundResourceName = "field";
        public PokemonInstance.BasicSpecification pokemonSpecification;

        public override void Interact(GameCharacterController interacter)
        {

            BattleEntranceArguments.battleBackgroundResourceName = battleBackgroundResourceName;
            BattleEntranceArguments.battleType = BattleType.WildPokemon;
            BattleEntranceArguments.wildPokemonBattleArguments = new BattleEntranceArguments.WildPokemonBattleArguments()
            {
                opponentInstance = pokemonSpecification.Generate()
            };
            BattleEntranceArguments.initialWeatherId = 0;
            BattleEntranceArguments.argumentsSet = true;

            GameSceneManager.LaunchBattleScene();

        }

    }
}

#endif
