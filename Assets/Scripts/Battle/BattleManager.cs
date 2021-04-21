using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pokemon;
using Pokemon.Moves;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;
using Audio;

namespace Battle
{

    public partial class BattleManager : GeneralSceneManager
    {

        public static BattleManager GetBattleSceneBattleManager(Scene scene)
        {

            BattleManager[] managers = FindObjectsOfType<BattleManager>()
                .Where(x => x.gameObject.scene == scene)
                .ToArray();

            switch (managers.Length)
            {

                case 0:
                    return null;

                case 1:
                    return managers[0];

                default:
                    Debug.LogError("Multiple BattleManager found");
                    return managers[0];

            }

        }

        public BattleAnimationSequencer battleAnimationSequencer;

        public PlayerUI.PlayerBattleUIController playerBattleUIController;
        public PlayerUI.PlayerPokemonSelectUIController playerPokemonSelectUIController;
        public PlayerUI.PlayerMoveSelectUIController playerMoveSelectUIController;
        public PlayerUI.LearnMoveUI.LearnMoveUIController learnMoveUIController;

        [HideInInspector]
        public Coroutine mainBattleCoroutine;

        [HideInInspector]
        public BattleData battleData;

        public BattleLayout.BattleLayoutController battleLayoutController;

        private TextBoxController textBoxController;

        /// <summary>
        /// A list of battle participants who shouldn't have their actions executed.
        /// For example if their pokemon has fainted and they have just changed pokemon
        /// </summary>
        private List<BattleParticipant> battleParticipantsToCancelActionsOf;
        
        private void Start()
        {

            textBoxController = TextBoxController.GetTextBoxController(Scene);

            SetUpScene();

        }

        private void SetUpScene()
        {

            battleLayoutController.HidePokemonAndPanes();
            learnMoveUIController.menuConfirmLearnMoveSelectionController.Hide();
            learnMoveUIController.menuLearnMoveController.Hide();
            battleLayoutController.SetUpBackground(BattleEntranceArguments.battleBackgroundResourceName);

        }

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

                    participantOpponent = BattleParticipantNPC.modeInitialisers[BattleParticipantNPC.Mode.WildPokemon](
                        null,
                        new PokemonInstance[] {
                            BattleEntranceArguments.wildPokemonBattleArguments.opponentInstance
                        },
                        0
                    );

                    break;

                case BattleType.NPCTrainer:

                    participantOpponent = BattleParticipantNPC.modeInitialisers[BattleEntranceArguments.npcTrainerBattleArguments.mode](
                        BattleEntranceArguments.npcTrainerBattleArguments.opponentFullName,
                        BattleEntranceArguments.npcTrainerBattleArguments.opponentPokemon,
                        BattleEntranceArguments.npcTrainerBattleArguments.opponentBasePayout
                    );

                    break;

                default:

                    Debug.LogError("Unknown battle type");

                    //Generate generic opponent instead of crashing or breaking scene/game
                    participantOpponent = BattleParticipantNPC.modeInitialisers[BattleParticipantNPC.Mode.RandomAttack](
                        "Erroneous Opponent",
                        new PokemonInstance[]
                        {
                            PokemonFactory.GenerateWild(
                                new int[] { 1 },
                                1,
                                1
                            )
                        },
                        0
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

            foreach (PokemonInstance pokemon in battleData.participantPlayer.GetPokemon().Where(x => x != null))
                pokemon.ResetBattleProperties();

            foreach (PokemonInstance pokemon in battleData.participantOpponent.GetPokemon().Where(x => x != null))
                pokemon.ResetBattleProperties();

            battleData.participantPlayer.playerBattleUIController = playerBattleUIController;
            battleData.participantPlayer.playerPokemonSelectUIController = playerPokemonSelectUIController;
            battleData.participantPlayer.playerMoveSelectUIController = playerMoveSelectUIController;

            #region Choose Starting Pokemon

            #region Player

            int playerStartingPokemonIndex = -1;

            for (int i = 0; i < battleData.participantPlayer.GetPokemon().Length; i++)
                if (battleData.participantPlayer.GetPokemon()[i] != null && !battleData.participantPlayer.GetPokemon()[i].IsFainted)
                {
                    playerStartingPokemonIndex = i;
                    break;
                }

            if (playerStartingPokemonIndex < 0)
            {
                Debug.LogError("Unable to find pokemon for player to start battle with");
                playerStartingPokemonIndex = 0;
            }

            battleData.participantPlayer.activePokemonIndex = playerStartingPokemonIndex;

            #endregion

            #region Opponent

            int opponentStartingPokemonIndex = -1;

            for (int i = 0; i < battleData.participantOpponent.GetPokemon().Length; i++)
                if (!battleData.participantOpponent.GetPokemon()[i].IsFainted)
                {
                    opponentStartingPokemonIndex = i;
                    break;
                }

            if (opponentStartingPokemonIndex < 0)
            {
                Debug.LogError("Unable to find pokemon for opponent to start battle with");
                opponentStartingPokemonIndex = 0;
            }

            battleData.participantOpponent.activePokemonIndex = opponentStartingPokemonIndex;

            #endregion

            #endregion

            battleData.participantPlayer.SetUp();

            #region Setting Battle Type-Dependant BattleData Settings

            switch (BattleEntranceArguments.battleType)
            {

                case BattleType.WildPokemon:
                    battleData.SetPlayerCanFlee(true);
                    battleData.itemUsagePermissions = new BattleData.ItemUsagePermissions();
                    break;

                case BattleType.NPCTrainer:
                    battleData.SetPlayerCanFlee(false);
                    battleData.itemUsagePermissions = new BattleData.ItemUsagePermissions() { pokeBalls = false };
                    break;

                default:
                    Debug.LogWarning("Unknown battle type");
                    battleData.SetPlayerCanFlee(true);
                    break;

            }

            #endregion

            #region Pokemon and Trainer Announcements

            #region Announce Opponent Pokemon

            //If the opponent is a wild pokemon, they should appear before their announcement message
            //If the opponent is a trainer, the pokemon should appear after the announcement message

            if (battleData.isWildBattle)
            {

                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = BattleAnimationSequencer.Animation.Type.OpponentSendOutWild,
                    sendOutPokemon = battleData.participantOpponent.ActivePokemon
                });

                battleAnimationSequencer.EnqueueSingleText("A wild "
                    + battleData.participantOpponent.ActivePokemon.GetDisplayName()
                    + " appeared!");

            }
            else
            {

                Sprite opponentTrainerBattleSprite = SpriteStorage.GetCharacterBattleSprite(
                    BattleEntranceArguments.npcTrainerBattleArguments.opponentSpriteResourceName
                );

                if (opponentTrainerBattleSprite != null)
                {
                    battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                    {
                        type = BattleAnimationSequencer.Animation.Type.OpponentTrainerShowcaseStart,
                        opponentTrainerShowcaseSprite = opponentTrainerBattleSprite
                    });
                }

                battleAnimationSequencer.EnqueueSingleText(battleData.participantOpponent.GetName()
                    + " challenged you!");
                battleAnimationSequencer.EnqueueSingleText(battleData.participantOpponent.GetName()
                    + " sent out "
                    + battleData.participantOpponent.ActivePokemon.GetDisplayName()
                    + '!');

                if (opponentTrainerBattleSprite != null)
                {
                    battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                    {
                        type = BattleAnimationSequencer.Animation.Type.OpponentTrainerShowcaseStop
                    });
                }

                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = BattleAnimationSequencer.Animation.Type.OpponentSendOutTrainer,
                    sendOutPokemon = battleData.participantOpponent.ActivePokemon,
                    participantPokemonStates = battleData.participantOpponent.GetPokemon().Select(x => BattleLayout.PokeBallLineController.GetPokemonInstanceBallState(x)).ToArray()
                });

            }

            #endregion

            #region Announce Player Pokemon

            battleAnimationSequencer.EnqueueSingleText("Go, " + battleData.participantPlayer.ActivePokemon.GetDisplayName() + '!');

            battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
            {
                type = BattleAnimationSequencer.Animation.Type.PlayerSendOut,
                sendOutPokemon = battleData.participantPlayer.ActivePokemon,
                participantPokemonStates = battleData.participantPlayer.GetPokemon().Select(x => BattleLayout.PokeBallLineController.GetPokemonInstanceBallState(x)).ToArray()
            });

            #endregion

            #endregion

            //TODO - when and if abilities made, apply them and announce them if needed

            BattleEntranceArguments.argumentsSet = false;

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            battleParticipantsToCancelActionsOf = new List<BattleParticipant>();

            battleData.battleTurnNumber = 0;

            #endregion

            #region Main Loop

            while (battleData.battleRunning)
            {

                #region Weather Announcement

                if (battleData.CurrentWeather.announcement != null)
                {
                    battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                    {
                        type = BattleAnimationSequencer.Animation.Type.Text,
                        messages = new string[] { Weather.GetWeatherById(battleData.currentWeatherId).announcement },
                        requireUserContinue = false
                    });
                }

                //TODO - when ready (if weather has one) show weather display animation

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                #endregion

                #region Action Choosing

                battleData.participantPlayer.StartChoosingAction(battleData);
                battleData.participantOpponent.StartChoosingAction(battleData);

                SetPlayerPokemonBobbingState(true);
                SetTextBoxTextToPlayerActionPrompt();

                yield return new WaitUntil(() =>
                {
                    return battleData.participantPlayer.actionHasBeenChosen
                    && battleData.participantOpponent.actionHasBeenChosen;
                });

                SetPlayerPokemonBobbingState(false);
                textBoxController.SetTextInstant("");

                #endregion

                #region Action Order Deciding

                BattleParticipant.Action[] actionsUnsortedArray = new BattleParticipant.Action[]
                {
                    battleData.participantPlayer.chosenAction,
                    battleData.participantOpponent.chosenAction
                };

                Queue<BattleParticipant.Action> actionQueue = new Queue<BattleParticipant.Action>(
                    actionsUnsortedArray.OrderByDescending(
                        x => x,
                        new BattleParticipant.Action.PriorityComparer()
                    )
                );

                #endregion

                #region Action Execution

                while (actionQueue.Count > 0)
                {

                    BattleParticipant.Action nextAction = actionQueue.Dequeue();

                    if (!battleParticipantsToCancelActionsOf.Contains(nextAction.user))
                    {
                        yield return StartCoroutine(ExecuteAction(nextAction));
                    }

                    RefreshUsedPokemonPerOpposingPokemonRecord();

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

                //At the end of each turn, the overview panes should be refreshed in case anything was missed during the turn execution
                battleLayoutController.overviewPaneManager.playerPokemonOverviewPaneController.FullUpdate(battleData.participantPlayer.ActivePokemon);
                battleLayoutController.overviewPaneManager.opponentPokemonOverviewPaneController.FullUpdate(battleData.participantOpponent.ActivePokemon);

                battleParticipantsToCancelActionsOf.Clear();

                battleData.battleTurnNumber++;

                #endregion

            }

            #endregion

            #region Ending Battle

            #region Convert Bad Poisons to Poisons

            foreach (PokemonInstance pokemon in battleData.participantPlayer.GetPokemon().Where(x => x != null))
                if (pokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned)
                    pokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.Poisoned;

            foreach (PokemonInstance pokemon in battleData.participantOpponent.GetPokemon().Where(x => x != null))
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

                battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name + ' ' + moneyDropMessage + " ₽" + moneyToDrop + "...");

                #endregion

                #region Blacked Out

                battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name + " blacked out!");

                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = BattleAnimationSequencer.Animation.Type.Blackout
                });

                #endregion

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                //TODO - return player to last place they healed

            }
            else
            {

                if (battleData.isWildBattle)
                    MusicSourceController.singleton.SetTrack("victory_wild", true);
                else
                    MusicSourceController.singleton.SetTrack("victory_trainer", true);

                if (battleData.participantOpponent is BattleParticipantNPC opponentNPCParticipant)
                {

                    if (opponentNPCParticipant.basePayout > 0)
                    {

                        int playerPrizeMoney = CalculateTrainerOpponentPrizeMoney(opponentNPCParticipant);

                        PlayerData.singleton.AddMoney(playerPrizeMoney);

                        battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name
                            + " defeated "
                            + opponentNPCParticipant.GetName());

                        battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name
                            + " was given ₽"
                            + playerPrizeMoney.ToString()
                            + " for winning",
                            true);

                        yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                    }

                }

                MusicSourceController.singleton.StopMusic();
                GameSceneManager.CloseBattleScene();

            }

            #endregion

        }

        private IEnumerator MainBattleCoroutine_CheckPokemonFainted()
        {

            #region Pokemon Fainting Animation and Fainting Management

            PokemonInstance playerActivePokemon = battleData.participantPlayer.ActivePokemon;
            if (playerActivePokemon.IsFainted)
            {

                playerActivePokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;

                #region Remove from Player Pokemon Contributions

                if (battleData
                    .playerUsedPokemonPerOpponentPokemon[battleData.participantOpponent.activePokemonIndex]
                    .Contains(
                        battleData.participantPlayer.activePokemonIndex)
                    )
                {

                    battleData.playerUsedPokemonPerOpponentPokemon[battleData.participantOpponent.activePokemonIndex]
                        .Remove(
                            battleData.participantPlayer.activePokemonIndex
                        );

                }

                #endregion

                #region Cancelling Participant's Action

                battleParticipantsToCancelActionsOf.Add(battleData.participantPlayer);

                #endregion

                battleAnimationSequencer.EnqueueSingleText(GetActivePokemonFaintMessage(
                    battleData.participantPlayer,
                    playerActivePokemon
                    ));
                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = BattleAnimationSequencer.Animation.Type.PlayerRetract
                });

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

            PokemonInstance opponentActivePokemon = battleData.participantOpponent.ActivePokemon;
            if (opponentActivePokemon.IsFainted)
            {

                opponentActivePokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;

                #region Cancelling Participant's Action

                battleParticipantsToCancelActionsOf.Add(battleData.participantOpponent);

                #endregion

                battleAnimationSequencer.EnqueueSingleText(GetActivePokemonFaintMessage(
                    battleData.participantOpponent,
                    opponentActivePokemon
                    ));
                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = BattleAnimationSequencer.Animation.Type.OpponentRetract
                });

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                #region Experience and EV Yielding

                yield return StartCoroutine(DistributeExperienceAndEVsForCurrentOpponentPokemon());

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

                textBoxController.SetTextInstant("Select your next pokemon");

                yield return new WaitUntil(() => battleData.participantPlayer.nextPokemonHasBeenChosen);

                textBoxController.SetTextInstant("");

                battleData.participantPlayer.activePokemonIndex = battleData.participantPlayer.chosenNextPokemonIndex;

                battleAnimationSequencer.EnqueueSingleText(GetReplacedPokemonMessage(battleData.participantPlayer));
                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = BattleAnimationSequencer.Animation.Type.PlayerSendOut,
                    sendOutPokemon = battleData.participantPlayer.ActivePokemon,
                    participantPokemonStates = battleData.participantPlayer.GetPokemon().Select(x => BattleLayout.PokeBallLineController.GetPokemonInstanceBallState(x)).ToArray()
                });

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

            if (battleData.participantOpponent.ActivePokemon.IsFainted)
            {

                battleData.participantOpponent.StartChoosingNextPokemon();

                yield return new WaitUntil(() => battleData.participantOpponent.nextPokemonHasBeenChosen);

                battleData.participantOpponent.activePokemonIndex = battleData.participantOpponent.chosenNextPokemonIndex;

                battleAnimationSequencer.EnqueueSingleText(GetReplacedPokemonMessage(battleData.participantOpponent));
                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = BattleAnimationSequencer.Animation.Type.OpponentSendOutWild,
                    sendOutPokemon = battleData.participantOpponent.ActivePokemon
                });

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

            #endregion

        }

        public static int CalculateTrainerOpponentPrizeMoney(BattleParticipantNPC opponent)
        {
            PokemonInstance[] opponentPokemon = opponent.GetPokemon();
            return opponent.basePayout * (opponentPokemon[opponentPokemon.Length - 1].GetLevel());
        }

        private IEnumerator MainBattleCoroutine_CheckPokemonFainted_LevelUpMoveLearning(PokemonInstance pokemon,
            byte startLevel)
        {

            Dictionary<byte, int[]> levelUpMoves = pokemon.species.levelUpMoves;

            #region Determine Moves To Learn

            //I have used a queue as I will use this to iterate over in order so the player can choose whether to learn a move or not and the options should be provided in order
            Queue<int> moveIdsToLearn = new Queue<int>();

            for (byte level = (byte)(startLevel + 1); level <= pokemon.GetLevel(); level++)
            {

                if (levelUpMoves.ContainsKey(level))
                    foreach (int moveId in levelUpMoves[level])
                        moveIdsToLearn.Enqueue(moveId);

            }

            #endregion

            if (moveIdsToLearn.Count <= 0)
                yield break;

            #region Learning Moves

            while (moveIdsToLearn.Count > 0)
            {

                int moveIdToLearn = moveIdsToLearn.Dequeue();
                PokemonMove moveToLearn = PokemonMove.GetPokemonMoveById(moveIdToLearn);

                if (!pokemon.moveIds.Any(x => PokemonMove.MoveIdIsUnset(x))) //If all move slots taken
                {

                    battleAnimationSequencer.EnqueueSingleText(
                        pokemon.GetDisplayName()
                        + " want to learn "
                        + moveToLearn.name
                        + " but it already knows 4 moves");
                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                    learnMoveUIController.StartUI(pokemon, moveIdToLearn);
                    yield return new WaitUntil(() => !learnMoveUIController.uiRunning);

                    PlayerUI.LearnMoveUI.LearnMoveUIController.UIPlayerDecision playerDecision = learnMoveUIController.uiPlayerDecision;

                    if (playerDecision.learnMove)
                    {

                        PokemonMove forgottenMove = PokemonMove.GetPokemonMoveById(pokemon.moveIds[playerDecision.replacedMoveIndex]);

                        #region Setting New Move

                        pokemon.moveIds[playerDecision.replacedMoveIndex] = moveIdToLearn;
                        pokemon.movePPs[playerDecision.replacedMoveIndex] = moveToLearn.maxPP;

                        #endregion

                        #region Announcement Message

                        battleAnimationSequencer.EnqueueSingleText(
                            pokemon.GetDisplayName()
                            + " forgot "
                            + forgottenMove.name
                            + ".....");
                        battleAnimationSequencer.EnqueueSingleText(
                            pokemon.GetDisplayName()
                            + " learnt "
                            + moveToLearn.name
                            + '!');
                        yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                        #endregion

                    }
                    else
                    {

                        battleAnimationSequencer.EnqueueSingleText(
                            pokemon.GetDisplayName()
                            + " didn't learn "
                            + moveToLearn.name);
                        yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                    }

                }
                else //If move slot available
                {

                    bool moveSet = false;

                    for (int moveIdIndex = 0; moveIdIndex < pokemon.moveIds.Length; moveIdIndex++)
                    {

                        #region Setting New Move

                        if (PokemonMove.MoveIdIsUnset(pokemon.moveIds[moveIdIndex]))
                        {
                            pokemon.moveIds[moveIdIndex] = moveIdToLearn;
                            pokemon.movePPs[moveIdIndex] = moveToLearn.maxPP;
                            moveSet = true;
                            break;
                        }

                        #endregion

                    }

                    if (!moveSet)
                        Debug.LogError("Failed to set move even with available place");

                    battleAnimationSequencer.EnqueueSingleText(
                        pokemon.GetDisplayName()
                        + " learnt "
                        + moveToLearn.name
                        + '!');

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                }

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

                int initialHealth = participant.ActivePokemon.health;

                participant.ActivePokemon.TakeDamage(
                    Mathf.RoundToInt(
                        participant.ActivePokemon.GetStats().health
                        * Weather.damageMaxHealthProportion
                    )
                );

                if (battleData.CurrentWeather.damageMessage != null && battleData.CurrentWeather.damageMessage != "")
                    battleAnimationSequencer.EnqueueSingleText(
                        participant.ActivePokemon.GetDisplayName()
                        + ' '
                        + battleData.CurrentWeather.damageMessage);
                else
                    battleAnimationSequencer.EnqueueSingleText(
                        participant.ActivePokemon.GetDisplayName()
                        + " was damaged from the weather");

                battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(participant.ActivePokemon, initialHealth, participant is BattleParticipantPlayer));

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    yield break;

            }

        }

        private IEnumerator MainBattleCoroutine_ApplyNonVolatileStatusConditionDamageUsingParticipant(BattleParticipant participant)
        {

            int initialHealth = participant.ActivePokemon.health;

            switch (participant.ActivePokemon.nonVolatileStatusCondition)
            {

                case PokemonInstance.NonVolatileStatusCondition.Burn:

                    int burnDamageToDeal = Mathf.RoundToInt(participant.ActivePokemon.GetStats().health * 0.125F);

                    participant.ActivePokemon.TakeDamage(burnDamageToDeal);

                    battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName() + " was hurt by its burn");
                    battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(participant.ActivePokemon, initialHealth, participant is BattleParticipantPlayer));

                    break;

                case PokemonInstance.NonVolatileStatusCondition.Poisoned:

                    int poisonDamageToDeal = Mathf.RoundToInt(participant.ActivePokemon.GetStats().health * 0.125F);

                    participant.ActivePokemon.TakeDamage(poisonDamageToDeal);

                    battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName() + " was hurt by its poison");
                    battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(participant.ActivePokemon, initialHealth, participant is BattleParticipantPlayer));

                    break;

                case PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned:

                    int badlyPoisonedDamageToDeal = Mathf.RoundToInt(participant.ActivePokemon.badlyPoisonedCounter * 0.0625F);

                    participant.ActivePokemon.TakeDamage(badlyPoisonedDamageToDeal);
                    participant.ActivePokemon.badlyPoisonedCounter++;

                    battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName() + " was hurt by its bad poison");
                    battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(participant.ActivePokemon, initialHealth, participant is BattleParticipantPlayer));

                    break;

                default:
                    yield break;

            }

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

        }

        public void SetTextBoxTextToPlayerActionPrompt()
            => textBoxController.SetTextInstant("What will "
                + battleData.participantPlayer.ActivePokemon.GetDisplayName()
                + " do?");

        #region Player Invalid Selection Messages

        private Coroutine displayPlayerInvalidSelectionMessageCouroutine;

        private IEnumerator _DisplayPlayerInvalidSelectionMessage(string message)
        {
            battleAnimationSequencer.EnqueueSingleText(message);
            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            SetTextBoxTextToPlayerActionPrompt();
        }

        public void DisplayPlayerInvalidSelectionMessage(string message)
        {

            if (displayPlayerInvalidSelectionMessageCouroutine != null)
                StopCoroutine(displayPlayerInvalidSelectionMessageCouroutine);

            displayPlayerInvalidSelectionMessageCouroutine = StartCoroutine(_DisplayPlayerInvalidSelectionMessage(message));

        }

        #endregion

        private void SetPlayerPokemonBobbingState(bool state)
        {
            if (state)
                battleLayoutController.StartPlayerPokemonBobbing(battleData.participantPlayer.ActivePokemon.nonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.None);
            else
                battleLayoutController.StopPlayerPokemonBobbing();
        }

        /// <summary>
        /// Generate an animation for the specified pokemon changing health assuming that the PokemonInstance has already lost the health
        /// </summary>
        /// <param name="pokemon">The pokemon to consider</param>
        /// <param name="startingHealth">The health the pokemon started on</param>
        /// <param name="isPlayer">Whether the animation is being generated for the player (not the opponent)</param>
        /// <returns>The animation generated</returns>
        private BattleAnimationSequencer.Animation GenerateDamageAnimation(PokemonInstance pokemon,
            int startingHealth,
            bool isPlayer)
            => new BattleAnimationSequencer.Animation()
            {
                type = isPlayer ? BattleAnimationSequencer.Animation.Type.PlayerTakeDamage : BattleAnimationSequencer.Animation.Type.OpponentTakeDamage,
                takeDamageMaxHealth = pokemon.GetStats().health,
                takeDamageNewHealth = pokemon.health,
                takeDamageOldHealth = startingHealth
            };

        private void RefreshUsedPokemonPerOpposingPokemonRecord()
        {

            int opponentPokemonIndex = battleData.participantOpponent.activePokemonIndex;
            int playerPokemonIndex = battleData.participantPlayer.activePokemonIndex;

            if (!battleData.playerUsedPokemonPerOpponentPokemon[opponentPokemonIndex].Contains(playerPokemonIndex))
                battleData.playerUsedPokemonPerOpponentPokemon[opponentPokemonIndex].Add(playerPokemonIndex);

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

        private IEnumerator RefreshParticipantNVSC(BattleParticipant participant)
        {

            if (participant.ActivePokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Asleep)
            {

                participant.ActivePokemon.remainingSleepTurns--;

                if (participant.ActivePokemon.remainingSleepTurns <= 0)
                {

                    battleAnimationSequencer.EnqueueSingleText(
                        participant.ActivePokemon.GetDisplayName()
                        + " woke up!"
                        );
                    participant.ActivePokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;

                    if (participant is BattleParticipantPlayer)
                        battleLayoutController.overviewPaneManager.playerPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(participant.ActivePokemon.nonVolatileStatusCondition);
                    else
                        battleLayoutController.overviewPaneManager.opponentPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(participant.ActivePokemon.nonVolatileStatusCondition);

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                }

            }

            if (participant.ActivePokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Frozen)
            {

                if (UnityEngine.Random.Range(0F, 1F) < PokemonInstance.thawChancePerTurn)
                {

                    battleAnimationSequencer.EnqueueSingleText(
                        participant.ActivePokemon.GetDisplayName()
                        + " thawed out!"
                        );
                    participant.ActivePokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;

                    if (participant is BattleParticipantPlayer)
                        battleLayoutController.overviewPaneManager.playerPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(participant.ActivePokemon.nonVolatileStatusCondition);
                    else
                        battleLayoutController.overviewPaneManager.opponentPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(participant.ActivePokemon.nonVolatileStatusCondition);

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                }

            }

        }

        private IEnumerator RefreshParticipantConfusion(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileStatusConditions.confusion > 0)
            {

                participantPokemon.battleProperties.volatileStatusConditions.confusion--;

                if (participantPokemon.battleProperties.volatileStatusConditions.confusion <= 0)
                {

                    battleAnimationSequencer.EnqueueSingleText(
                        participantPokemon.GetDisplayName()
                        + " snapped out of its confusion!"
                        );

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                }

            }

        }

        private IEnumerator DistributeExperienceAndEVsForCurrentOpponentPokemon()
        {

            PokemonInstance opponentActivePokemon = battleData.participantOpponent.ActivePokemon;

            List<int> pokemonUsedIndexes = battleData
                    .playerUsedPokemonPerOpponentPokemon[battleData.participantOpponent.activePokemonIndex];

            ushort opponentPokemonBaseExperienceYield = opponentActivePokemon.species.baseExperienceYield;
            byte opponentPokemonLevel = opponentActivePokemon.GetLevel();

            foreach (int playerPokemonIndex in pokemonUsedIndexes)
            {

                PokemonInstance playerPokemonInstance = battleData.participantPlayer.GetPokemon()[playerPokemonIndex];

                #region Experience Yielding

                //Don't try to give the player's pokemon experience if they are at level 100
                if (playerPokemonInstance.GetLevel() < 100)
                {

                    int experienceToAdd = (opponentPokemonBaseExperienceYield * opponentPokemonLevel) / (7 * pokemonUsedIndexes.Count);

                    if (!battleData.isWildBattle)
                        experienceToAdd = Mathf.FloorToInt(experienceToAdd * 1.5F);

                    //TODO - if playerPokemonInstance holding lucky egg, multiply by 1.5

                    //TODO - if playerPokemonInstance was traded (aka isn't with original trainer), multiply by 1.5

                    byte previousPlayerPokemonLevel = playerPokemonInstance.GetLevel();
                    int previousPlayerPokemonExperience = playerPokemonInstance.experience;

                    playerPokemonInstance.AddMaxExperience(experienceToAdd);

                    battleAnimationSequencer.EnqueueSingleText(
                        playerPokemonInstance.GetDisplayName()
                        + " gained "
                        + experienceToAdd.ToString()
                        + " experience",
                        true);

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                    //If the current pokemon is the active pokemon, show the experience gain in the battle layout
                    if (playerPokemonIndex == battleData.participantPlayer.activePokemonIndex)
                    {

                        battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                        {
                            type = BattleAnimationSequencer.Animation.Type.PlayerPokemonExperienceGain,
                            experienceGainGrowthType = playerPokemonInstance.growthType,
                            experienceGainInitialExperience = previousPlayerPokemonExperience,
                            experienceGainNewExperience = battleData.participantPlayer.ActivePokemon.experience
                        });

                        yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                    }

                    if (previousPlayerPokemonLevel != playerPokemonInstance.GetLevel())
                    {

                        //TODO - level up animation
                        battleAnimationSequencer.EnqueueSingleText(
                            playerPokemonInstance.GetDisplayName()
                            + " levelled up to level "
                            + playerPokemonInstance.GetLevel().ToString()
                            + '!');

                        SoundFXController.singleton.PlaySound("level_up");
                        yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                        if (playerPokemonIndex == battleData.participantPlayer.activePokemonIndex)
                            battleLayoutController.overviewPaneManager.playerPokemonOverviewPaneController.UpdateLevel(playerPokemonInstance.GetLevel());

                        #region Evolution

                        PokemonSpecies.Evolution evolution = playerPokemonInstance.TryFindEvolution();

                        if (evolution != null)
                        {
                            yield return StartCoroutine(EvolvePokemon(playerPokemonInstance,
                                evolution.targetId,
                                playerPokemonIndex == battleData.participantPlayer.activePokemonIndex));
                        }

                        #endregion

                        yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted_LevelUpMoveLearning(playerPokemonInstance, previousPlayerPokemonLevel));

                    }

                }

                #endregion

                #region EV Yielding

                Stats<byte> opponentPokemonEVYield = opponentActivePokemon.species.evYield;

                playerPokemonInstance.AddEffortValuePoints(opponentPokemonEVYield);

                #endregion

            }

        }

        private bool readyToCarryOnAfterEvolution;

        private IEnumerator EvolvePokemon(PokemonInstance pokemon,
            int newSpeciesId,
            bool isActivePokemon)
        {

            EvolutionScene.EvolutionSceneController.entranceArguments = new EvolutionScene.EvolutionSceneController.EntranceArguments()
            {
                displayName = pokemon.GetDisplayName(),
                startSpeciesId = pokemon.speciesId,
                endSpeciesId = newSpeciesId,
                useFemaleSprite = pokemon.gender == false
            };

            pokemon.Evolve(newSpeciesId);

            if (isActivePokemon)
                battleLayoutController.UpdatePlayerPokemon(pokemon);

            DisableScene();

            GameSceneManager.LaunchEvolutionScene();

            readyToCarryOnAfterEvolution = false;
            GameSceneManager.EvolutionSceneClosed += () =>
            {
                readyToCarryOnAfterEvolution = true;
            };

            yield return new WaitUntil(() => readyToCarryOnAfterEvolution);

            EnableScene();
            
        }

        /// <summary>
        /// Select the action execution method for the provided action and run it using the action. This includes adding and running animations
        /// </summary>
        /// <param name="action">The action to execute</param>
        private IEnumerator ExecuteAction(BattleParticipant.Action action)
        {

            switch (action.type)
            {

                case BattleParticipant.Action.Type.Fight:
                    yield return StartCoroutine(RefreshParticipantNVSC(action.user));
                    yield return StartCoroutine(RefreshParticipantConfusion(action.user));
                    yield return StartCoroutine(ExecuteAction_Fight(action));
                    break;

                case BattleParticipant.Action.Type.Flee:

                    yield return StartCoroutine(ExecuteAction_Flee(action));

                    //If participant fails to flee, refresh their active pokemon's non-volatile status conditions
                    if (CheckIfBattleRunning())
                    {
                        yield return StartCoroutine(RefreshParticipantNVSC(action.user));
                    }

                    break;

                case BattleParticipant.Action.Type.SwitchPokemon:
                    yield return StartCoroutine(ExecuteAction_SwitchPokemon(action));
                    break;

                case BattleParticipant.Action.Type.UseItem:
                    yield return StartCoroutine(ExecuteAction_UseItem(action));
                    break;

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

            bool userIsPlayer = action.user is BattleParticipantPlayer;

            if (action.user.ActivePokemon.battleProperties.volatileStatusConditions.flinch)
                yield break;

            #region NVSC Move Failures

            if (action.user.ActivePokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Asleep)
            {

                battleAnimationSequencer.EnqueueSingleText(
                    action.user.ActivePokemon.GetDisplayName()
                    + " is fast asleep");

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                yield break;

            }

            if (action.user.ActivePokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Paralysed)
            {

                if (UnityEngine.Random.Range(0F, 1F) < PokemonInstance.paralysisFightFailChance)
                {

                    battleAnimationSequencer.EnqueueSingleText(
                        action.user.ActivePokemon.GetDisplayName()
                        + " is paralysed and couldn't move!"
                        );

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                    yield break;

                }

            }

            if (action.user.ActivePokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Frozen)
            {

                battleAnimationSequencer.EnqueueSingleText(
                        action.user.ActivePokemon.GetDisplayName()
                        + " is frozen solid!"
                        );

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                yield break;

            }

            #endregion

            #region Confusion Move Failure

            if (action.user.ActivePokemon.battleProperties.volatileStatusConditions.confusion > 0)
            {

                battleAnimationSequencer.EnqueueSingleText(
                    action.user.ActivePokemon.GetDisplayName()
                    + " is confused"
                    );

                if (UnityEngine.Random.Range(0F, 1F) <= PokemonInstance.BattleProperties.VolatileStatusConditions.confusionPokemonDamageChance)
                {

                    battleAnimationSequencer.EnqueueSingleText(
                    action.user.ActivePokemon.GetDisplayName()
                    + " hurt itself in its confusion!"
                    );

                    int previousHealth = action.user.ActivePokemon.health;
                    Stats<int> userBattleStats = action.user.ActivePokemon.GetBattleStats();

                    action.user.ActivePokemon.TakeDamage(PokemonMove.CalculateNormalDamageToDeal(
                        action.user.ActivePokemon.GetLevel(),
                        PokemonInstance.BattleProperties.VolatileStatusConditions.confusionUserHarmPower,
                        ((float)userBattleStats.attack) / userBattleStats.defense,
                        1
                    ));

                    battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                    {
                        type = userIsPlayer ? BattleAnimationSequencer.Animation.Type.PlayerTakeDamage : BattleAnimationSequencer.Animation.Type.OpponentTakeDamage,
                        takeDamageMaxHealth = action.user.ActivePokemon.GetStats().health,
                        takeDamageNewHealth = action.user.ActivePokemon.health,
                        takeDamageOldHealth = previousHealth
                    });

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                    yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());
                    yield break;

                }

            }

            #endregion

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

            string moveUsageMessage = action.user.ActivePokemon.GetDisplayName() + " used " + move.name;
            battleAnimationSequencer.EnqueueSingleText(moveUsageMessage);

            PokemonMove.UsageResults usageResults = move.CalculateEffect(
                action.user.ActivePokemon,
                action.fightMoveTarget.ActivePokemon,
                battleData
            );

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            #region Effects

            //This may not be the same as the moveHitCount calculated since the target or user may faint during the attacking
            byte multiHitsLanded = 0;

            if (usageResults.Succeeded)
            {

                byte moveHitCount = (byte)UnityEngine.Random.Range(move.minimumMultiHitAmount, move.maximumMultiHitAmount + 1);

                for (int i = 0; i < moveHitCount; i++)
                {

                    multiHitsLanded++;

                    battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation
                    {
                        type = BattleAnimationSequencer.Animation.Type.PokemonMove,
                        pokemonMoveId = move.id,
                        pokemonMovePlayerIsUser = userIsPlayer
                    });

                    #region Target Effects

                    PokemonInstance targetPokemon = action.fightMoveTarget.ActivePokemon;

                    #region Target Damage

                    //No need to animate damage dealing or try take damage is no damage is dealt
                    if (usageResults.targetDamageDealt > 0)
                    {

                        int targetInitialHealth = targetPokemon.health;

                        targetPokemon.TakeDamage(usageResults.targetDamageDealt);

                        battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(targetPokemon, targetInitialHealth, !userIsPlayer));

                    }

                    //Stop loop if target fainted
                    if (targetPokemon.IsFainted)
                        break;

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

                        if (userIsPlayer)
                            battleLayoutController.overviewPaneManager.opponentPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(targetPokemon.nonVolatileStatusCondition);
                        else
                            battleLayoutController.overviewPaneManager.playerPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(targetPokemon.nonVolatileStatusCondition);

                    }

                    #endregion

                    #region Non-Volatile Status Conditions

                    if (usageResults.targetNonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.None
                        && targetPokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.None)
                    {

                        targetPokemon.nonVolatileStatusCondition = usageResults.targetNonVolatileStatusCondition;

                        if (usageResults.targetNonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Asleep)
                            targetPokemon.remainingSleepTurns = usageResults.targetAsleepInflictionDuration;

                        string nvscInflictionMessage = targetPokemon.GetDisplayName()
                            + ' '
                            + PokemonInstance.nonVolatileStatusConditionMessages[
                                usageResults.targetNonVolatileStatusCondition
                                ];

                        battleAnimationSequencer.EnqueueSingleText(nvscInflictionMessage);

                        //TODO - enqueue NVSC animation

                        if (userIsPlayer)
                            battleLayoutController.overviewPaneManager.opponentPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(targetPokemon.nonVolatileStatusCondition);
                        else
                            battleLayoutController.overviewPaneManager.playerPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(targetPokemon.nonVolatileStatusCondition);

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

                        if (targetPokemon.IsFainted)
                        {
                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " flinched!");
                        }

                    }

                    #endregion

                    #endregion

                    #region User Effects

                    PokemonInstance userPokemon = action.user.ActivePokemon;

                    #region Move PP down

                    if (!action.fightUsingStruggle)
                        userPokemon.movePPs[action.fightMoveIndex]--;

                    #endregion

                    #region User Damage

                    if (usageResults.userDamageDealt > 0)
                    {

                        int userInitialHealth = action.user.ActivePokemon.health;

                        action.user.ActivePokemon.TakeDamage(usageResults.userDamageDealt);

                        battleAnimationSequencer.EnqueueSingleText(action.user.ActivePokemon.GetDisplayName() + " was hurt from the recoil");
                        battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(userPokemon, userInitialHealth, action.user is BattleParticipantPlayer));

                    }

                    //Stop loop if user fainted
                    if (userPokemon.IsFainted)
                        break;

                    #endregion

                    #region User Healing

                    if (usageResults.userHealthHealed > 0)
                    {

                        int userInitialHealth = action.user.ActivePokemon.health;

                        action.user.ActivePokemon.HealHealth(usageResults.userHealthHealed);

                        battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                        {
                            type = action.user is BattleParticipantPlayer
                                ? BattleAnimationSequencer.Animation.Type.PlayerHealHealth
                                : BattleAnimationSequencer.Animation.Type.OpponentHealHealth,
                            takeDamageOldHealth = userInitialHealth,
                            takeDamageNewHealth = userPokemon.health,
                            takeDamageMaxHealth = userPokemon.GetStats().health
                        });
                        battleAnimationSequencer.EnqueueSingleText(action.user.ActivePokemon.GetDisplayName() + " recovered some health");

                    }

                    #endregion

                    if (usageResults.userHealthHealed > 0 && usageResults.userDamageDealt > 0)
                        Debug.LogError("Usage results contained health reduction and health increase for user pokemon");

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

                    //Prepare usage results (not allowing misses) for next hit
                    if (i < moveHitCount - 1)
                    {
                        usageResults = move.CalculateEffect(
                            action.user.ActivePokemon,
                            action.fightMoveTarget.ActivePokemon,
                            battleData,
                            false
                        );
                    }

                }

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }
            else if (usageResults.failed)
            {

                battleAnimationSequencer.EnqueueSingleText("It failed!");
                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }
            else if (usageResults.missed)
            {

                if (!action.fightUsingStruggle)
                    action.user.ActivePokemon.movePPs[action.fightMoveIndex]--;

                battleAnimationSequencer.EnqueueSingleText("It missed!");
                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

            #endregion

            if (move.IsMultiHit && usageResults.Succeeded)
            {
                battleAnimationSequencer.EnqueueSingleText("Hit " + multiHitsLanded.ToString() + " times");
                yield return StartCoroutine(battleAnimationSequencer.PlayAll());
            }

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
                animationsList.Add(new BattleAnimationSequencer.Animation()
                {
                    type = pokemonIsOpponent
                       ? BattleAnimationSequencer.Animation.Type.OpponentStatModifierUp
                       : BattleAnimationSequencer.Animation.Type.PlayerStatModifierUp
                });
                animationsList.Add(BattleAnimationSequencer.CreateSingleTextAnimation(pokemon.GetDisplayName() + "'s " + statName + " rose"));
            }
            else if (statModifierStageChange == -1)
            {
                animationsList.Add(new BattleAnimationSequencer.Animation()
                {
                    type = pokemonIsOpponent
                    ? BattleAnimationSequencer.Animation.Type.OpponentStatModifierDown
                    : BattleAnimationSequencer.Animation.Type.PlayerStatModifierDown
                });
                animationsList.Add(BattleAnimationSequencer.CreateSingleTextAnimation(pokemon.GetDisplayName() + "'s " + statName + " fell"));
            }
            else if (statModifierStageChange > 1)
            {
                animationsList.Add(new BattleAnimationSequencer.Animation()
                {
                    type = pokemonIsOpponent
                    ? BattleAnimationSequencer.Animation.Type.OpponentStatModifierUp
                    : BattleAnimationSequencer.Animation.Type.PlayerStatModifierUp
                });
                animationsList.Add(BattleAnimationSequencer.CreateSingleTextAnimation(pokemon.GetDisplayName() + "'s " + statName + " rose sharply"));
            }
            else if (statModifierStageChange < -1)
            {
                animationsList.Add(new BattleAnimationSequencer.Animation()
                {
                    type = pokemonIsOpponent
                    ? BattleAnimationSequencer.Animation.Type.OpponentStatModifierDown
                    : BattleAnimationSequencer.Animation.Type.PlayerStatModifierDown
                });
                animationsList.Add(BattleAnimationSequencer.CreateSingleTextAnimation(pokemon.GetDisplayName() + "'s " + statName + " fell harshly"));
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

            if (escaperSpeed > opponentSpeed)
                return byte.MaxValue;

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

            bool escapeSuccess = UnityEngine.Random.Range(0, 256) <= escapeChance;

            if (escapeSuccess)
            {

                battleAnimationSequencer.EnqueueSingleText(battleData.participantPlayer.ActivePokemon.GetDisplayName() + " escaped successfully");

                battleData.battleRunning = false;

            }
            else
            {

                battleAnimationSequencer.EnqueueSingleText(battleData.participantPlayer.ActivePokemon.GetDisplayName() + " couldn't escape!");

            }

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

        }

        /// <summary>
        /// Execute a switch pokemon action
        /// </summary>
        private IEnumerator ExecuteAction_SwitchPokemon(BattleParticipant.Action action)
        {

            action.user.ActivePokemon.badlyPoisonedCounter = 1;
            action.user.ActivePokemon.battleProperties.ResetVolatileProperties();

            action.user.activePokemonIndex = action.switchPokemonIndex;

            battleAnimationSequencer.EnqueueSingleText(action.user.GetName() + " switched in " + action.user.ActivePokemon.GetDisplayName());

            battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
            {
                type = action.user == battleData.participantPlayer
                ? BattleAnimationSequencer.Animation.Type.PlayerRetract
                : BattleAnimationSequencer.Animation.Type.OpponentRetract
            });

            battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
            {
                type = action.user == battleData.participantPlayer
                ? BattleAnimationSequencer.Animation.Type.PlayerSendOut
                : BattleAnimationSequencer.Animation.Type.OpponentSendOutTrainer,
                sendOutPokemon = action.user.ActivePokemon,
                participantPokemonStates = action.user.GetPokemon().Select(x => BattleLayout.PokeBallLineController.GetPokemonInstanceBallState(x)).ToArray()
            });

            //TODO - apply effects for newly-switched in pokemon's ability (if abilities included)

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

        }

        /// <summary>
        /// Execute a use item action
        /// </summary>
        private IEnumerator ExecuteAction_UseItem(BattleParticipant.Action action)
        {

            if (action.useItemItemToUse is PokeBall)
                yield return StartCoroutine(ExecuteAction_UseItem_PokeBall(action));
            else
                yield return StartCoroutine(ExecuteAction_UseItem_UsageEffectsItem(action));

        }

        private IEnumerator ExecuteAction_UseItem_PokeBall(BattleParticipant.Action action)
        {

            if (!(action.user is BattleParticipantPlayer))
            {
                Debug.LogError("Opponent used poke ball");
                yield break;
            }

            PokemonInstance targetPokemon = action.useItemPokeBallTarget.ActivePokemon;

            char[] vowels = new char[] { 'a', 'e', 'i', 'o', 'u' };
            string itemName = action.useItemItemToUse.itemName;
            battleAnimationSequencer.EnqueueSingleText(action.user.GetName()
                + " used "
                + (vowels.Contains(char.ToLower(itemName[0])) ? "an" : "a")
                + ' '
                + itemName);

            #region Inventory Reduction

            if (!action.useItemDontConsumeItem)
                PlayerData.singleton.inventory.RemoveItem(action.useItemItemToUse, 1);

            #endregion

            PokeBall pokeBall = (PokeBall)action.useItemItemToUse;

            #region Result Calculation

            bool[] shakeResults = new bool[PokeBall.shakeTrialsRequired];

            shakeResults = new bool[PokeBall.shakeTrialsRequired];

            for (int i = 0; i < PokeBall.shakeTrialsRequired; i++)
            {

                bool result = PokeBall.CalculateIfShake(targetPokemon, battleData, pokeBall);
                shakeResults[i] = result;

                //If a test fails, the tests after it are irrelevant and should be considered as failing
                if (!result)
                    break;

            }

            #endregion

            #region Get Wobble Count

            byte wobbleCount = 0;
            foreach (bool result in shakeResults)
                if (result)
                    wobbleCount++;
                else
                    break;

            #endregion

            #region Shake/Catch Animation

            battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
            {
                type = BattleAnimationSequencer.Animation.Type.PokeBallUse,
                pokeBallUsePokeBall = pokeBall,
                pokeBallUseWobbleCount = wobbleCount
            });

            if (shakeResults.All(x => x))
                battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " was caught!");
            else
                battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " broke out!");

            #endregion

            #region Successful Catch

            if (shakeResults.All(x => x)) //If the catch is successful
            {

                #region Experience and EV Distribution

                yield return StartCoroutine(DistributeExperienceAndEVsForCurrentOpponentPokemon());

                #endregion

                #region Target Pokemon Catch Details Setting

                targetPokemon.pokeBallId = pokeBall.id;
                targetPokemon.originalTrainerName = PlayerData.singleton.profile.name;
                targetPokemon.catchTime = PokemonInstance.GetCurrentEpochTime();

                #endregion

                #region Add Pokemon to Player Pokemon

                if (PlayerData.singleton.partyPokemon.Any(x => x == null)) //If the player has space for the new pokemon in their party
                {

                    PlayerData.singleton.AddNewPartyPokemon(targetPokemon);
                    battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " was added to your party");

                }
                else //Otherwise, send the new pokemon to the player's boxes
                {

                    PlayerData.singleton.AddBoxPokemon(targetPokemon);
                    battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " was sent to your storage system");

                }

                #endregion

                //End the battle
                battleData.battleRunning = false;

            }

            #endregion

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

        }

        private IEnumerator ExecuteAction_UseItem_UsageEffectsItem(BattleParticipant.Action action)
        {

            bool playerIsUser = action.user is BattleParticipantPlayer;

            #region Inventory Reduction

            if (playerIsUser && !action.useItemDontConsumeItem)
                PlayerData.singleton.inventory.RemoveItem(action.useItemItemToUse, 1);

            #endregion

            bool definetlyApplyToActivePokemon;

            if (action.useItemItemToUse is BattleItem)
                definetlyApplyToActivePokemon = true;
            else if (action.useItemItemToUse is MedicineItem)
                definetlyApplyToActivePokemon = false;
            else
            {
                Debug.LogError("Couldn't choose whether to apply item to active pokemon");
                definetlyApplyToActivePokemon = true;
            }

            PokemonInstance affectedPokemon = definetlyApplyToActivePokemon ? action.user.ActivePokemon : action.user.GetPokemon()[action.useItemTargetPartyIndex];

            bool activePokemonIsItemTarget = definetlyApplyToActivePokemon
                || action.useItemTargetPartyIndex == action.user.activePokemonIndex;

            char[] vowels = new char[] { 'a','e','i','o','u' };
            string itemName = action.useItemItemToUse.itemName;
            battleAnimationSequencer.EnqueueSingleText(action.user.GetName()
                + " used "
                + (vowels.Contains(char.ToLower(itemName[0])) ? "an" : "a")
                + ' '
                + itemName);

            #region Usage Effects

            if (action.useItemTargetMoveIndex >= 0 && action.useItemItemToUse is PPRestoreMedicineItem)
                PPRestoreMedicineItem.singleMoveIndexToRecoverPP = action.useItemTargetMoveIndex;

            Item.ItemUsageEffects itemUsageEffects = action.useItemItemToUse.GetUsageEffects(affectedPokemon);

            if (itemUsageEffects.healthRecovered != 0)
            {

                bool pokemonWasFainted = affectedPokemon.IsFainted;
                int pokemonPrevHealth = affectedPokemon.health;

                affectedPokemon.HealHealth(itemUsageEffects.healthRecovered);

                if (activePokemonIsItemTarget)
                {
                    battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                    {
                        type = playerIsUser ? BattleAnimationSequencer.Animation.Type.PlayerHealHealth : BattleAnimationSequencer.Animation.Type.OpponentHealHealth,
                        takeDamageOldHealth = pokemonPrevHealth,
                        takeDamageNewHealth = affectedPokemon.health,
                        takeDamageMaxHealth = affectedPokemon.GetStats().health
                    });
                }

                if (pokemonWasFainted)
                    battleAnimationSequencer.EnqueueSingleText(affectedPokemon.GetDisplayName()
                    + " was revived!");

                else
                    battleAnimationSequencer.EnqueueSingleText(affectedPokemon.GetDisplayName()
                        + " recovered some health");

            }

            if (itemUsageEffects.statModifierChanges.GetEnumerator(false).Any(x => x != 0)
                || itemUsageEffects.evasionModifierChange != 0
                || itemUsageEffects.accuracyModifierChange != 0)
            {

                BattleAnimationSequencer.Animation[] statChangeAnimations = ExecuteAction_Fight_StatModifiers(
                    affectedPokemon,
                    !playerIsUser,
                    itemUsageEffects.statModifierChanges,
                    itemUsageEffects.evasionModifierChange,
                    itemUsageEffects.accuracyModifierChange);

                if (activePokemonIsItemTarget)
                    foreach (BattleAnimationSequencer.Animation animation in statChangeAnimations)
                        battleAnimationSequencer.EnqueueAnimation(animation);

                else
                    //Only take the animations that don't affect the current battle layout
                    foreach (BattleAnimationSequencer.Animation animation in statChangeAnimations.Where(x => x.type == BattleAnimationSequencer.Animation.Type.Text))
                        battleAnimationSequencer.EnqueueAnimation(animation);

            }

            if (itemUsageEffects.increaseCritChance)
            {

                affectedPokemon.battleProperties.criticalHitChanceBoosted = true;

                battleAnimationSequencer.EnqueueSingleText(affectedPokemon.GetDisplayName()
                    + " focused intensely");

            }

            if (itemUsageEffects.nvscCured)
            {

                affectedPokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;

                battleAnimationSequencer.EnqueueSingleText(affectedPokemon.GetDisplayName() + " was cured of its status condition");

                if (activePokemonIsItemTarget)
                {

                    if (playerIsUser)
                        battleLayoutController.overviewPaneManager.playerPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(affectedPokemon.nonVolatileStatusCondition);
                    else
                        battleLayoutController.overviewPaneManager.opponentPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(affectedPokemon.nonVolatileStatusCondition);

                }

            }

            if (itemUsageEffects.ppIncreases.Any(x => x != 0))
            {

                for (int i = 0; i < affectedPokemon.movePPs.Length; i++)
                    affectedPokemon.movePPs[i] += itemUsageEffects.ppIncreases[i];

                battleAnimationSequencer.EnqueueSingleText(affectedPokemon.GetDisplayName() + " recovered some PP");

            }

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            #endregion

        }

    }

}
