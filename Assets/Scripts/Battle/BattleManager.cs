using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Pokemon;
using Pokemon.Moves;
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
                initialWeatherId = BattleEntranceArguments.initialWeatherId
            };

            battleData.participantPlayer.battleManager = this;
            battleData.participantOpponent.battleManager = this;

            foreach (PokemonInstance pokemon in battleData.participantPlayer.GetPokemon())
                pokemon.ResetBattleProperties();

            foreach (PokemonInstance pokemon in battleData.participantOpponent.GetPokemon())
                pokemon.ResetBattleProperties();

            battleData.participantPlayer.playerBattleUIController = playerBattleUIController;
            battleData.participantPlayer.playerPokemonSelectUIController = playerPokemonSelectUIController;

            battleData.participantPlayer.SetUp();
            
            switch (BattleEntranceArguments.battleType)
            {

                case BattleType.WildPokemon:
                    battleData.SetPlayerCanFlee(true);
                    break;

                case BattleType.NPCTrainer:
                    battleData.SetPlayerCanFlee(false);
                    break;

                default:
                    Debug.LogError("Unknown battle type");
                    battleData.SetPlayerCanFlee(true);
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

            while (battleData.battleRunning)
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

                BattleParticipant.Action[] actions = new BattleParticipant.Action[]
                {
                    battleData.participantPlayer.chosenAction,
                    battleData.participantOpponent.chosenAction
                };

                Queue<BattleParticipant.Action> actionQueue = new Queue<BattleParticipant.Action>(
                    actions.OrderByDescending(
                        x => x,
                        new BattleParticipant.Action.PriorityComparer()
                    )
                );

                #endregion

                #region Action Execution

                while (actionQueue.Count > 0)
                {

                    ExecuteAction(actionQueue.Dequeue());

                    //TODO - check if battle still running (eg. participant fled or ran out of pokemon). Method in BattleData?

                }

                #endregion

                //TODO - continue

                #region End of Turn

                //TODO - not sure of the order that the effects of the below should be done
                //TODO - damage pokemon if current weather damages pokemon's type. (Check what happens with multi-types)
                //TODO - damage/heal pokemon if needed by volatile status conditions and non-volatiles

                #endregion

            }

            #endregion

        }

        /// <summary>
        /// Select the action execution method for the provided action and run it using the action. This includes adding and running animations
        /// </summary>
        /// <param name="action">The action to execute</param>
        private void ExecuteAction(BattleParticipant.Action action)
        {

            switch (action.type)
            {

                case BattleParticipant.Action.Type.Fight:
                    ExecuteAction_Fight(action);
                    break;

                case BattleParticipant.Action.Type.Flee:
                    ExecuteAction_Flee(action);
                    break;

                case BattleParticipant.Action.Type.SwitchPokemon:
                    ExecuteAction_SwitchPokemon(action);
                    break;

                //TODO - case for item using

                default:
                    Debug.LogError("Unknown action type - " + action.type);
                    break;

            }

        }

        /// <summary>
        /// Execute a fight action
        /// </summary>
        private void ExecuteAction_Fight(BattleParticipant.Action action)
        {

            PokemonMove move = PokemonMove.GetPokemonMoveById(action.user.ActivePokemon.moveIds[action.fightMoveIndex]);

            PokemonMove.UsageResults usageResults = move.CalculateEffect(
                action.user.ActivePokemon,
                action.fightMoveTarget.ActivePokemon
            );

            //TODO - deal with each part of usage results in order

            //TODO - deal with damage
            //TODO - deal with stat changes (incl. accuracy and evasion)
            //TODO - deal with flinching (plan in todo)
            //TODO - deal with non-volatile status conditions (if target doesn't already have one)
            //TODO - deal with confusion

            //TODO - when special moves made, have their effects inflicted (maybe by separate method made for all special moves to directly cause changes to pokemon and return announcements)

        }

        /// <summary>
        /// Returns a number which represents a chance (in the range [0,255]) that a pokemon should successfully escape a battle provided the unmodified speeds of each pokemon
        /// </summary>
        /// <param name="escaperSpeed">The unmodified speed of the pokemon trying to escape</param>
        /// <param name="opponentSpeed">The unmodified speed of the opponent pokemon</param>
        /// <param name="escapeAttempts">The number of times the player has tried to escape during this battle</param>
        /// <returns>The chance (in the range [0,255]) that the pokemon should escape</returns>
        private byte CalculateEscapeChance(int escaperSpeed,
            int opponentSpeed,
            int escapeAttempts)
        {

            float speedComparison = ((float)(escaperSpeed * 128)) / opponentSpeed;
            return (byte)(speedComparison + (30 * escapeAttempts)); //Casting the result to a byte will automatically apply "mod 256" to the result

        }

        /// <summary>
        /// Execute a flee action
        /// </summary>
        private void ExecuteAction_Flee(BattleParticipant.Action action)
        {

            if (!battleData.playerCanFlee)
            {
                Debug.LogError("Player attempted to flee but shouldn't be allowed to");
                return;
            }

            //This should always be the case
            if (action.user == battleData.participantPlayer)
            {
                battleData.playerEscapeAttempts++;
            }

            byte escapeChance = CalculateEscapeChance(
                battleData.participantPlayer.ActivePokemon.GetStats().speed,
                battleData.participantOpponent.ActivePokemon.GetStats().speed,
                battleData.playerEscapeAttempts
            );

            bool escapeSuccess = Random.Range(0, 256) < escapeChance;

            if (escapeSuccess)
            {

                //TODO - show message that player escaped successfully

                battleData.battleRunning = false;

            }
            else
            {

                //TODO - show message that player failed to escape

            }

        }

        /// <summary>
        /// Execute a switch pokemon action
        /// </summary>
        private void ExecuteAction_SwitchPokemon(BattleParticipant.Action action)
        {

            //TODO

        }

        //TODO - have execution function for item actions

    }

}