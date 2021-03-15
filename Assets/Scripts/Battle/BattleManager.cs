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

                    //TODO - check if battle still running (eg. participant fled or ran out of pokemon)

                    //TODO - check if battle still running but an active pokemon has fainted and must be replaced

                }

                //TODO - check if battle still running

                #endregion

                //TODO - continue (use plan to help)

                #region End of Turn

                //TODO - not sure of the order that the effects of the below should be done
                //TODO - damage pokemon if current weather damages pokemon's type. (Check what happens with multi-types)
                //TODO - damage/heal pokemon if needed by volatile status conditions and non-volatiles

                //TODO - disable flinching for every active pokemon (in PokemonInstance.BattleProperties.VolatileStatusConditions.flinch)

                #endregion

            }

            #endregion

        }

        private static bool CheckIfBattleOver(BattleData battleData)
        {

            return (!battleData.participantPlayer.CheckIfDefeated())
                && (!battleData.participantOpponent.CheckIfDefeated())
                && battleData.battleRunning;

        }

        private bool CheckIfBattleOver() => CheckIfBattleOver(battleData);

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

            if (action.user.ActivePokemon.battleProperties.volatileStatusConditions.flinch)
                return;

            PokemonMove move = PokemonMove.GetPokemonMoveById(action.user.ActivePokemon.moveIds[action.fightMoveIndex]);

            PokemonMove.UsageResults usageResults = move.CalculateEffect(
                action.user.ActivePokemon,
                action.fightMoveTarget.ActivePokemon,
                battleData
            );

            string moveUsageMessage = action.user.ActivePokemon.GetDisplayName() + " used " + move.name;
            battleAnimationSequencer.EnqueueSingleText(moveUsageMessage);

            //TODO - animation for move usage

            battleAnimationSequencer.PlayAll();

            #region Damage

            if (usageResults.Succeeded)
            {

                #region Target Effects

                PokemonInstance targetPokemon = action.fightMoveTarget.ActivePokemon;

                #region Target Damage

                targetPokemon.TakeDamage(usageResults.targetDamageDealt);

                //TODO - animation for health reducing

                #region Effectiveness Message

                if (usageResults.effectiveness == true)
                {
                    battleAnimationSequencer.EnqueueSingleText("It was super effective!");
                }
                else if (usageResults.effectiveness == false)
                {
                    battleAnimationSequencer.EnqueueSingleText("It was not very effective!");
                }

                #endregion

                #region Critical Hit

                if (usageResults.criticalHit)
                {
                    battleAnimationSequencer.EnqueueSingleText("It was a critical hit!");
                }

                #endregion

                #endregion

                #region Target Confusion

                if (usageResults.targetConfuse)
                {

                    //Confusion should last between 1-4 turns decided randomly
                    targetPokemon.battleProperties.volatileStatusConditions.confusion = Random.Range(0, 5);

                    battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " became confused!");
                    //TODO - enqueue confusion animation

                }

                #endregion

                #region Non-Volatile Status Conditions

                if (usageResults.targetNonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.None
                    && targetPokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.None)
                {

                    targetPokemon.nonVolatileStatusCondition = usageResults.targetNonVolatileStatusCondition;

                    string nvscInflictionMessage = targetPokemon.GetDisplayName()
                        + ' '
                        + PokemonInstance.nonVolatileStatusConditionMessages[
                            usageResults.targetNonVolatileStatusCondition
                            ];

                    battleAnimationSequencer.EnqueueSingleText(nvscInflictionMessage);
                    //TODO - enqueue NVSC animation

                }

                #endregion

                #region Target Stat Modifiers

                BattleAnimationSequencer.Animation[] targetStatModifierAnimations = ExecuteAction_Fight_StatModifiers(
                    targetPokemon,
                    action.fightMoveTarget == battleData.participantOpponent,
                    usageResults.targetStatChanges,
                    usageResults.targetEvasionChange,
                    usageResults.targetAccuracyChange
                    );

                foreach (BattleAnimationSequencer.Animation animation in targetStatModifierAnimations)
                    battleAnimationSequencer.EnqueueAnimation(animation);

                #endregion

                #region Target Flinching

                if (usageResults.targetFlinch)
                {

                    targetPokemon.battleProperties.volatileStatusConditions.flinch = true;

                    battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " flinched!");

                }

                #endregion

                #endregion

                #region User Effects

                PokemonInstance userPokemon = action.user.ActivePokemon;

                #region Move PP down

                userPokemon.movePPs[action.fightMoveIndex]--;

                #endregion

                #region User Damage

                action.user.ActivePokemon.TakeDamage(usageResults.userDamageDealt);

                //TODO - animation for health reducing

                #endregion

                #region User Stat Modifiers

                BattleAnimationSequencer.Animation[] userStatModifierAnimations = ExecuteAction_Fight_StatModifiers(
                    userPokemon,
                    action.user == battleData.participantOpponent,
                    usageResults.userStatChanges,
                    usageResults.userEvasionChange,
                    usageResults.userAccuracyChange
                    );

                foreach (BattleAnimationSequencer.Animation animation in userStatModifierAnimations)
                    battleAnimationSequencer.EnqueueAnimation(animation);

                #endregion

                #endregion

                battleAnimationSequencer.PlayAll();

            }
            else if (usageResults.failed)
            {

                battleAnimationSequencer.EnqueueSingleText("It failed!");
                battleAnimationSequencer.PlayAll();

            }
            else if (usageResults.missed)
            {

                action.user.ActivePokemon.movePPs[action.fightMoveIndex]--;

                battleAnimationSequencer.EnqueueSingleText("It missed!");
                battleAnimationSequencer.PlayAll();

            }

            #endregion

            //TODO - when special moves made, have their effects inflicted (maybe by separate method made for all special moves to directly cause changes to pokemon and return announcements)

        }

        /// <summary>
        /// Applies provided stat modifier stage changes to a pokemon instance and returns a list of animations that should be shown because of them
        /// </summary>
        /// <param name="pokemon">The PokemonInstance to apply the changes to</param>
        /// <param name="pokemonIsOpponent">Whether the pokemon is the opponent. This is used when generating the animation for the stat modifier changes</param>
        /// <param name="statChanges">The main stat modifier stage changes to apply</param>
        /// <param name="evasionChange">The evasion stat modifier change to apply</param>
        /// <param name="accuracyChange">The accuracy stat modifier change to apply</param>
        /// <returns>The animations that should be shown for the applied stat changes</returns>
        private BattleAnimationSequencer.Animation[] ExecuteAction_Fight_StatModifiers(PokemonInstance pokemon,
            bool pokemonIsOpponent,
            Stats<sbyte> statChanges,
            sbyte evasionChange,
            sbyte accuracyChange)
        {

            List<BattleAnimationSequencer.Animation> animations = new List<BattleAnimationSequencer.Animation>();

            pokemon.battleProperties.statModifiers.attack += statChanges.attack;
            animations.AddRange(ExecuteAction_Fight_StatModifiers_SingleModifierAnimations(
                pokemon,
                pokemonIsOpponent,
                statChanges.attack,
                "attack"
                ));

            pokemon.battleProperties.statModifiers.defense += statChanges.defense;
            animations.AddRange(ExecuteAction_Fight_StatModifiers_SingleModifierAnimations(
                pokemon,
                pokemonIsOpponent,
                statChanges.defense,
                "defense"
                ));

            pokemon.battleProperties.statModifiers.specialAttack += statChanges.specialAttack;
            animations.AddRange(ExecuteAction_Fight_StatModifiers_SingleModifierAnimations(
                pokemon,
                pokemonIsOpponent,
                statChanges.specialAttack,
                "special attack"
                ));

            pokemon.battleProperties.statModifiers.specialDefense += statChanges.specialDefense;
            animations.AddRange(ExecuteAction_Fight_StatModifiers_SingleModifierAnimations(
                pokemon,
                pokemonIsOpponent,
                statChanges.specialDefense,
                "special defense"
                ));

            pokemon.battleProperties.statModifiers.speed += statChanges.speed;
            animations.AddRange(ExecuteAction_Fight_StatModifiers_SingleModifierAnimations(
                pokemon,
                pokemonIsOpponent,
                statChanges.speed,
                "speed"
                ));

            pokemon.battleProperties.evasionModifier += evasionChange;
            animations.AddRange(ExecuteAction_Fight_StatModifiers_SingleModifierAnimations(
                pokemon,
                pokemonIsOpponent,
                evasionChange,
                "evasion"
                ));

            pokemon.battleProperties.accuracyModifier += accuracyChange;
            animations.AddRange(ExecuteAction_Fight_StatModifiers_SingleModifierAnimations(
                pokemon,
                pokemonIsOpponent,
                accuracyChange,
                "accuracy"
                ));

            return animations.ToArray();

        }

        /// <summary>
        /// Gets the animations to play for applying a stat modifier change to a pokemon instance
        /// </summary>
        /// <param name="pokemon">The pokemon to apply the changes to</param>
        /// <param name="pokemonIsOpponent">Whether the pokemon is the opponent. This is used when generating the animation for the stat modifier changes</param>
        /// <param name="statModifierStageChange">The amount to change the pokemon's stat modifier stage by</param>
        /// <param name="statName">The name of the stat. This is used for the message showed for the stat change</param>
        /// <returns>The animations that should be played for the changes</returns>
        public BattleAnimationSequencer.Animation[] ExecuteAction_Fight_StatModifiers_SingleModifierAnimations(PokemonInstance pokemon,
            bool pokemonIsOpponent,
            sbyte statModifierStageChange,
            string statName)
        {

            List<BattleAnimationSequencer.Animation> animationsList = new List<BattleAnimationSequencer.Animation>();

            if (statModifierStageChange == 1)
            {
                animationsList.Add(BattleAnimationSequencer.CreateSingleTextAnimation(pokemon.GetDisplayName() + "'s " + statName + " rose"));
                animationsList.Add(new BattleAnimationSequencer.Animation()
                {
                    type = pokemonIsOpponent
                    ? BattleAnimationSequencer.Animation.Type.OpponentStatModifierUp
                    : BattleAnimationSequencer.Animation.Type.OpponentStatModifierDown
                });
            }
            else if (statModifierStageChange == -1)
            {
                animationsList.Add(BattleAnimationSequencer.CreateSingleTextAnimation(pokemon.GetDisplayName() + "'s " + statName + " fell"));
                animationsList.Add(new BattleAnimationSequencer.Animation()
                {
                    type = pokemonIsOpponent
                    ? BattleAnimationSequencer.Animation.Type.OpponentStatModifierUp
                    : BattleAnimationSequencer.Animation.Type.OpponentStatModifierDown
                });
            }
            else if (statModifierStageChange > 1)
            {
                animationsList.Add(BattleAnimationSequencer.CreateSingleTextAnimation(pokemon.GetDisplayName() + "'s " + statName + " rose sharply"));
                animationsList.Add(new BattleAnimationSequencer.Animation()
                {
                    type = pokemonIsOpponent
                    ? BattleAnimationSequencer.Animation.Type.OpponentStatModifierUp
                    : BattleAnimationSequencer.Animation.Type.OpponentStatModifierDown
                });
            }
            else if (statModifierStageChange < -1)
            {
                animationsList.Add(BattleAnimationSequencer.CreateSingleTextAnimation(pokemon.GetDisplayName() + "'s " + statName + " fell harshly"));
                animationsList.Add(new BattleAnimationSequencer.Animation()
                {
                    type = pokemonIsOpponent
                    ? BattleAnimationSequencer.Animation.Type.OpponentStatModifierUp
                    : BattleAnimationSequencer.Animation.Type.OpponentStatModifierDown
                });
            }

            return animationsList.ToArray();

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

            action.user.activePokemonIndex = action.switchPokemonIndex;

            //TODO - show message that participant switched pokemon
            //TODO - add animation of participant switching pokemon

            battleAnimationSequencer.PlayAll();

        }

        //TODO - have execution function for item actions

    }

}