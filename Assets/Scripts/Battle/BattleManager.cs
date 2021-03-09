using System.Collections;
using UnityEngine;
using Battle;

namespace Battle
{

    public class BattleManager : MonoBehaviour
    {

        public BattleAnimationSequencer battleAnimationSequencer;

        public PlayerUI.PlayerBattleUIController playerBattleUIController;
        public PlayerUI.PlayerPokemonSelectUIController playerPokemonSelectUIController;

        [HideInInspector]
        public Coroutine mainBattleCoroutine;

        [HideInInspector]
        public BattleData battleData;

        public void StartBattle()
        {

            mainBattleCoroutine = StartCoroutine(MainBattleCoroutine());

        }

        public IEnumerator MainBattleCoroutine()
        {

            #region Initial Setup

            //If the battle entrance arguments don't seem to be set, use whatever values are present at the time but still log an error
            if (!BattleEntranceArguments.argumentsSet)
            {
                Debug.LogError("Battle entrance arguments not set");
            }

            #region Opponent Participant

            BattleParticipant participantOpponent;

            switch (BattleEntranceArguments.battleType)
            {

                case BattleType.WildPokemon:

                    participantOpponent = new BattleParticipantNPC(
                        BattleParticipantNPC.Mode.RandomAttack,
                        new Pokemon.PokemonInstance[] {
                            BattleEntranceArguments.wildPokemonBattleArguments.opponentInstance
                        }
                    );

                    break;

                case BattleType.NPCTrainer:

                    participantOpponent = new BattleParticipantNPC(
                        BattleEntranceArguments.npcTrainerBattleArguments.mode,
                        BattleEntranceArguments.npcTrainerBattleArguments.opponentPokemon
                    );

                    break;

                default:

                    Debug.LogError("Unknown battle type");

                    //Generate generic opponent instead of crashing or breaking scene/game
                    participantOpponent = new BattleParticipantNPC(BattleParticipantNPC.Mode.RandomAttack,
                        new Pokemon.PokemonInstance[]
                        {
                            Pokemon.PokemonFactory.GenerateWild(
                                new int[] { 1 },
                                1,
                                1
                            )
                        }
                    );

                    break;

            }

            #endregion

            battleData = new BattleData()
            {
                participantPlayer = new BattleParticipantPlayer(),
                participantOpponent = participantOpponent,
                currentWeatherId = BattleEntranceArguments.initialWeatherId,
                weatherHasBeenChanged = false,
                turnsUntilWeatherFade = 0,
                initialWeatherId = BattleEntranceArguments.initialWeatherId,
            };

            battleData.participantPlayer.battleManager = this;
            battleData.participantOpponent.battleManager = this;

            battleData.participantPlayer.playerBattleUIController = playerBattleUIController;
            battleData.participantPlayer.playerPokemonSelectUIController = playerPokemonSelectUIController;

            battleData.participantPlayer.SetUp();
            
            switch (BattleEntranceArguments.battleType)
            {

                case BattleType.WildPokemon:
                    battleData.participantPlayer.SetPlayerCanFlee(true);
                    break;

                case BattleType.NPCTrainer:
                    battleData.participantPlayer.SetPlayerCanFlee(false);
                    break;

                default:
                    Debug.LogError("Unknown battle type");
                    battleData.participantPlayer.SetPlayerCanFlee(true);
                    break;

            }

            //TODO - queue announcement for opponent based on what type of opponent they are (ie. trainer vs wild pokemon)

            if (Weather.GetWeatherById(battleData.currentWeatherId).announcement != null)
            {
                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = BattleAnimationSequencer.Animation.Type.Text,
                    messages = new string[] { Weather.GetWeatherById(battleData.currentWeatherId).announcement },
                    requireUserContinue = false
                });
            }

            //TODO - when and if abilities made, apply them and announce them if needed

            battleAnimationSequencer.PlayAll();
            yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

            #endregion

            #region Main Loop

            bool battleRunning = true;

            while (battleRunning)
            {

                #region Action Choosing

                battleData.participantPlayer.StartChoosingAction(battleData);
                battleData.participantOpponent.StartChoosingAction(battleData);

                yield return new WaitUntil(() =>
                {
                    return battleData.participantPlayer.actionHasBeenChosen
                    && battleData.participantOpponent.actionHasBeenChosen;
                });

                #endregion

                #region Action Order Deciding

                

                #endregion

            }

            #endregion

        }

    }

}