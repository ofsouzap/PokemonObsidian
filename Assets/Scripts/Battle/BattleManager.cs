using System;
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
                        BattleEntranceArguments.wildPokemonBattleArguments.opponentInstance.GetDisplayName(),
                        new PokemonInstance[] {
                            BattleEntranceArguments.wildPokemonBattleArguments.opponentInstance
                        }
                    );

                    break;

                case BattleType.NPCTrainer:

                    participantOpponent = new BattleParticipantNPC(
                        BattleEntranceArguments.npcTrainerBattleArguments.mode,
                        BattleEntranceArguments.npcTrainerBattleArguments.opponentFullName,
                        BattleEntranceArguments.npcTrainerBattleArguments.opponentPokemon
                    );

                    break;

                default:

                    Debug.LogError("Unknown battle type");

                    //Generate generic opponent instead of crashing or breaking scene/game
                    participantOpponent = new BattleParticipantNPC(BattleParticipantNPC.Mode.RandomAttack,
                        "Erroneous Opponent",
                        new PokemonInstance[]
                        {
                            PokemonFactory.GenerateWild(
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
                isWildBattle = BattleEntranceArguments.battleType == BattleType.WildPokemon,
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

                #region Used Pokemon per Opposing Pokemon Record Updating

                int opponentPokemonIndex = battleData.participantOpponent.activePokemonIndex;
                int playerPokemonIndex = battleData.participantPlayer.activePokemonIndex;

                if (!battleData.playerUsedPokemonPerOpponentPokemon[opponentPokemonIndex].Contains(playerPokemonIndex))
                    battleData.playerUsedPokemonPerOpponentPokemon[opponentPokemonIndex].Add(playerPokemonIndex);

                #endregion

                while (actionQueue.Count > 0)
                {

                    yield return StartCoroutine(ExecuteAction(actionQueue.Dequeue()));

                    yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                    if (!CheckIfBattleRunning())
                        break;

                }

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region End of Turn

                #region Weather Damage

                yield return StartCoroutine(MainBattleCoroutine_ApplyWeatherDamageToParticipant(battleData.participantPlayer));

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(MainBattleCoroutine_ApplyWeatherDamageToParticipant(battleData.participantOpponent));

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Non-Volatile Status Conditions

                yield return StartCoroutine(MainBattleCoroutine_ApplyNonVolatileStatusConditionDamageUsingParticipant(battleData.participantPlayer));

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(MainBattleCoroutine_ApplyNonVolatileStatusConditionDamageUsingParticipant(battleData.participantOpponent));

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.flinch = false;
                battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.flinch = false;

                #endregion

            }

            #endregion

            #region Ending Battle

            #region Convert Bad Poisons to Poisons

            foreach (PokemonInstance pokemon in battleData.participantPlayer.GetPokemon())
                if (pokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned)
                    pokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.Poisoned;

            foreach (PokemonInstance pokemon in battleData.participantOpponent.GetPokemon())
                if (pokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned)
                    pokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.Poisoned;

            #endregion

            if (battleData.participantPlayer.CheckIfDefeated())
            {

                //If player lost (a draw counts as the player losing)

                #region Out of Usable Pokemon Message

                battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name + " ran out of usable pokemon...", true);

                #endregion

                #region Dropping Money

                int moneyToDrop = PlayerData.singleton.profile.money / 2;

                PlayerData.singleton.AddMoney(-moneyToDrop);

                string moneyDropMessage = battleData.isWildBattle ? "dropped" : "handed over";

                battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name + ' ' + moneyDropMessage + " P" + moneyToDrop + "...");

                #endregion

                #region Blacked Out

                battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name + " blacked out!");

                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = BattleAnimationSequencer.Animation.Type.Blackout
                });

                #endregion

                battleAnimationSequencer.PlayAll();
                yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

                //TODO - return player to last place they healed

            }
            else
            {

                //If player won

                //TODO - use yet-to-be-made custom scene manager to unload this scene and return to free-roaming scene

            }

            #endregion

        }

        private IEnumerator MainBattleCoroutine_CheckPokemonFainted()
        {

            #region Pokemon Fainting Animation

            PokemonInstance playerActivePokemon = battleData.participantPlayer.ActivePokemon;
            if (playerActivePokemon.IsFainted)
            {

                battleAnimationSequencer.EnqueueSingleText(GetActivePokemonFaintMessage(
                    battleData.participantPlayer,
                    playerActivePokemon
                    ));
                //TODO - animation for player active pokemon fainting

                battleAnimationSequencer.PlayAll();
                yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

            }

            PokemonInstance opponentActivePokemon = battleData.participantOpponent.ActivePokemon;
            if (opponentActivePokemon.IsFainted)
            {

                battleAnimationSequencer.EnqueueSingleText(GetActivePokemonFaintMessage(
                    battleData.participantOpponent,
                    opponentActivePokemon
                    ));
                //TODO - animation for opponent active pokemon fainting

                battleAnimationSequencer.PlayAll();
                yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

                #region Experience and EV Yielding

                List<int> pokemonUsedIndexes = battleData
                    .playerUsedPokemonPerOpponentPokemon[battleData.participantOpponent.activePokemonIndex];

                ushort opponentPokemonBaseExperienceYield = opponentActivePokemon.species.baseExperienceYield;
                byte opponentPokemonLevel = opponentActivePokemon.GetLevel();

                foreach (int playerPokemonIndex in pokemonUsedIndexes)
                {

                    PokemonInstance playerPokemonInstance = battleData.participantPlayer.GetPokemon()[playerPokemonIndex];

                    #region Experience Yielding

                    int experienceToAdd = (opponentPokemonBaseExperienceYield * opponentPokemonLevel) / (7 * pokemonUsedIndexes.Count);

                    if (!battleData.isWildBattle)
                        experienceToAdd = Mathf.FloorToInt(experienceToAdd * 1.5F);

                    //TODO - if playerPokemonInstance holding lucky egg, multiply by 1.5

                    //TODO - if playerPokemonInstance was traded (aka isn't with original trainer), multiply by 1.5

                    byte previousPlayerPokemonLevel = playerPokemonInstance.GetLevel();
                    playerPokemonInstance.AddMaxExperience(experienceToAdd);

                    battleAnimationSequencer.EnqueueSingleText(
                        playerPokemonInstance.GetDisplayName()
                        + " gained "
                        + experienceToAdd.ToString()
                        + " experience"
                        );

                    if (previousPlayerPokemonLevel != playerPokemonInstance.GetLevel())
                    {

                        battleAnimationSequencer.EnqueueSingleText(
                            playerPokemonInstance.GetDisplayName()
                            + " levelled up to level "
                            + playerPokemonInstance.GetLevel().ToString()
                            );
                        //TODO - level up animation

                        //TODO - if new move learnt, deal with this (incl. check if current moves full, asking whether player wants to replace current move, choosing move to replace)

                    }

                    battleAnimationSequencer.PlayAll();
                    yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

                    #endregion

                    #region EV Yielding

                    Stats<byte> opponentPokemonEVYield = opponentActivePokemon.species.evYield;

                    playerPokemonInstance.AddEffortValuePoints(opponentPokemonEVYield);

                    #endregion

                }

                #endregion

            }

            #endregion

            if (!CheckIfBattleRunning())
            {
                yield break;
            }

            #region Participants Replacing Pokemon

            if (battleData.participantPlayer.ActivePokemon.IsFainted)
            {

                battleData.participantPlayer.StartChoosingNextPokemon();

                yield return new WaitUntil(() => battleData.participantPlayer.nextPokemonHasBeenChosen);

                battleData.participantPlayer.activePokemonIndex = battleData.participantPlayer.chosenNextPokemonIndex;

                battleAnimationSequencer.EnqueueSingleText(GetReplacedPokemonMessage(battleData.participantPlayer));
                //TODO - animation for player sending out new pokemon

                battleAnimationSequencer.PlayAll();
                yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

            }

            if (battleData.participantOpponent.ActivePokemon.IsFainted)
            {

                battleData.participantOpponent.StartChoosingNextPokemon();

                yield return new WaitUntil(() => battleData.participantOpponent.nextPokemonHasBeenChosen);

                battleData.participantOpponent.activePokemonIndex = battleData.participantOpponent.chosenNextPokemonIndex;

                battleAnimationSequencer.EnqueueSingleText(GetReplacedPokemonMessage(battleData.participantOpponent));
                //TODO - animation for opponent sending out new pokemon

                battleAnimationSequencer.PlayAll();
                yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

            }

            #endregion

        }

        private IEnumerator MainBattleCoroutine_ApplyWeatherDamageToParticipant(BattleParticipant participant)
        {

            Pokemon.Type[] weatherDamagedTypes = battleData.CurrentWeather.damagedPokemonTypes;

            PokemonSpecies participantPokemonSpecies = participant.ActivePokemon.species;

            //If the pokemon's species has only one type, it will be set for both values of the array so that the array size is always 2
            Pokemon.Type[] participantPokemonTypes = new Pokemon.Type[]
            {
                    participantPokemonSpecies.type1,
                    participantPokemonSpecies.type2 != null ? (Pokemon.Type)participantPokemonSpecies.type2 : participantPokemonSpecies.type1
            };

            if (weatherDamagedTypes.Contains(participantPokemonTypes[0])
                && weatherDamagedTypes.Contains(participantPokemonTypes[1]))
            {

                participant.ActivePokemon.TakeDamage(
                    Mathf.RoundToInt(
                        participant.ActivePokemon.GetStats().health
                        * Weather.damageMaxHealthProportion
                    )
                );

                battleAnimationSequencer.EnqueueSingleText(
                    participant.ActivePokemon.GetDisplayName()
                    + " was damaged from the weather");

                //TODO - player pokemon health reduction animation

                battleAnimationSequencer.PlayAll();
                yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    yield break;

            }

        }

        private IEnumerator MainBattleCoroutine_ApplyNonVolatileStatusConditionDamageUsingParticipant(BattleParticipant participant)
        {

            switch (participant.ActivePokemon.nonVolatileStatusCondition)
            {

                case PokemonInstance.NonVolatileStatusCondition.Burn:

                    participant.ActivePokemon.TakeDamage(
                        Mathf.RoundToInt(
                            participant.ActivePokemon.GetStats().health * 0.125F
                        )
                    );

                    battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName() + " was hurt by its burn");
                    //TODO - damage animation

                    break;

                case PokemonInstance.NonVolatileStatusCondition.Poisoned:

                    participant.ActivePokemon.TakeDamage(
                        Mathf.RoundToInt(
                            participant.ActivePokemon.GetStats().health * 0.125F
                        )
                    );

                    battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName() + " was hurt by its poison");
                    //TODO - damage animation

                    break;

                case PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned:

                    participant.ActivePokemon.TakeDamage(
                        Mathf.RoundToInt(
                            participant.ActivePokemon.badlyPoisonedCounter * 0.0625F
                        )
                    );
                    participant.ActivePokemon.badlyPoisonedCounter++;

                    battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName() + " was hurt by its bad poison");
                    //TODO - damage animation

                    break;

                default:
                    yield break;

            }

            battleAnimationSequencer.PlayAll();
            yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

        }

        private static bool CheckIfBattleRunning(BattleData battleData)
        {

            return (!battleData.participantPlayer.CheckIfDefeated())
                && (!battleData.participantOpponent.CheckIfDefeated())
                && battleData.battleRunning;

        }

        private bool CheckIfBattleRunning() => CheckIfBattleRunning(battleData);

        private static string GetActivePokemonFaintMessage(BattleParticipant participant,
            PokemonInstance pokemon)
        {
            if (participant.GetName() != null && participant.GetName() != "")
            {
                return participant.GetName() + "'s " + pokemon.GetDisplayName() + " fainted!";
            }
            else
            {
                return pokemon.GetDisplayName() + " fainted!";
            }
        }

        private static string GetReplacedPokemonMessage(BattleParticipant participant)
            => participant.GetName()
            + " sent out "
            + participant.ActivePokemon.GetDisplayName();

        /// <summary>
        /// Select the action execution method for the provided action and run it using the action. This includes adding and running animations
        /// </summary>
        /// <param name="action">The action to execute</param>
        private IEnumerator ExecuteAction(BattleParticipant.Action action)
        {

            switch (action.type)
            {

                case BattleParticipant.Action.Type.Fight:
                    yield return StartCoroutine(ExecuteAction_Fight(action));
                    break;

                case BattleParticipant.Action.Type.Flee:
                    yield return StartCoroutine(ExecuteAction_Flee(action));
                    break;

                case BattleParticipant.Action.Type.SwitchPokemon:
                    yield return StartCoroutine(ExecuteAction_SwitchPokemon(action));
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
        private IEnumerator ExecuteAction_Fight(BattleParticipant.Action action)
        {

            //TODO - when special moves made, have their effects inflicted (maybe by separate method made for all special moves to directly cause changes to pokemon and return announcements)

            if (action.user.ActivePokemon.battleProperties.volatileStatusConditions.flinch)
                yield break;

            PokemonMove move;

            if (action.fightUsingStruggle)
            {

                move = PokemonMove.struggle;

            }
            else
            {

                if (action.user.ActivePokemon.movePPs[action.fightMoveIndex] <= 0)
                {
                    throw new ArgumentException("Participant selected move with 0 PP (Move Index " + action.fightMoveIndex + ")");
                }

                move = PokemonMove.GetPokemonMoveById(action.user.ActivePokemon.moveIds[action.fightMoveIndex]);

            }

            PokemonMove.UsageResults usageResults = move.CalculateEffect(
                action.user.ActivePokemon,
                action.fightMoveTarget.ActivePokemon,
                battleData
            );

            string moveUsageMessage = action.user.ActivePokemon.GetDisplayName() + " used " + move.name;
            battleAnimationSequencer.EnqueueSingleText(moveUsageMessage);

            //TODO - animation for move usage

            battleAnimationSequencer.PlayAll();
            yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

            #region Effects

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
                    targetPokemon.battleProperties.volatileStatusConditions.confusion = UnityEngine.Random.Range(0, 5);

                    battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " became confused!");
                    //TODO - enqueue confusion animation

                }

                #endregion

                #region Thawing

                if (usageResults.thawTarget)
                {
                    targetPokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;
                    battleAnimationSequencer.EnqueueSingleText(
                        targetPokemon.GetDisplayName()
                        + " was thawed out"
                        );
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
                yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

            }
            else if (usageResults.failed)
            {

                battleAnimationSequencer.EnqueueSingleText("It failed!");
                battleAnimationSequencer.PlayAll();
                yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

            }
            else if (usageResults.missed)
            {

                action.user.ActivePokemon.movePPs[action.fightMoveIndex]--;

                battleAnimationSequencer.EnqueueSingleText("It missed!");
                battleAnimationSequencer.PlayAll();
                yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

            }

            #endregion

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
        private IEnumerator ExecuteAction_Flee(BattleParticipant.Action action)
        {

            if (!battleData.playerCanFlee)
            {
                Debug.LogError("Player attempted to flee but shouldn't be allowed to");
                yield break;
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

            bool escapeSuccess = UnityEngine.Random.Range(0, 256) < escapeChance;

            if (escapeSuccess)
            {

                battleAnimationSequencer.EnqueueSingleText(battleData.participantPlayer.ActivePokemon.GetDisplayName() + " escaped successfully");

                battleData.battleRunning = false;

            }
            else
            {

                battleAnimationSequencer.EnqueueSingleText(battleData.participantPlayer.ActivePokemon.GetDisplayName() + " couldn't escape!");

            }

            battleAnimationSequencer.PlayAll();
            yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

        }

        /// <summary>
        /// Execute a switch pokemon action
        /// </summary>
        private IEnumerator ExecuteAction_SwitchPokemon(BattleParticipant.Action action)
        {

            action.user.ActivePokemon.badlyPoisonedCounter = 1;

            action.user.activePokemonIndex = action.switchPokemonIndex;

            battleAnimationSequencer.EnqueueSingleText(action.user.GetName() + " switched in " + action.user.ActivePokemon.GetDisplayName());
            //TODO - add animation of participant switching pokemon

            //TODO - apply effects for newly-switched in pokemon's ability (if abilities included)

            battleAnimationSequencer.PlayAll();
            yield return new WaitUntil(() => battleAnimationSequencer.queueEmptied);

        }

        //TODO - have execution function for item actions

    }

}