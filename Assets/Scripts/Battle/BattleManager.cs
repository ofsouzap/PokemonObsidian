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

        #region TMP - REMOVE ONCE FINISHED TESTING
        private void Start()
        {
            StartCoroutine(LateStart());
        }
        private IEnumerator LateStart()
        {
            yield return new WaitForFixedUpdate();
            StartBattle();
        }
        #endregion

        public void StartBattle()
        {

            mainBattleCoroutine = StartCoroutine(MainBattleCoroutine());

        }

        public IEnumerator MainBattleCoroutine()
        {

            #region Initial Setup

            BattleParticipant participantOpponent;

            //TODO - create instance of the required BattleParticipant child class based on entrance arguments
            participantOpponent = null; //TODO - remove once ready to write

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
            //TODO - uncomment below line once opponent participant isn't set to null
            //battleData.participantOpponent.battleManager = this;

            battleData.participantPlayer.playerBattleUIController = playerBattleUIController;
            battleData.participantPlayer.playerPokemonSelectUIController = playerPokemonSelectUIController;

            battleData.participantPlayer.SetUp();
            //TODO - set player can flee depending on opponent. (Can't flee trainers; can flee wild)

            //TODO - remove yield break once finished testing
            yield break;

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

            //TODO - main loop

        }

    }

}