using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using FreeRoaming;
using Pokemon;
using Pokemon.Moves;
using Items;
using Items.MedicineItems;
using Items.PokeBalls;
using Audio;
using Serialization;
using Networking;

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

        /// <summary>
        /// Event that triggers when the player completes their battle. When a battle is ended, all listeners are cleared whether or not the player wins.
        /// Event listeners should be added when a battle is just about to be launched (for example to mark a trainer NPC as battled if the player defeats them)
        /// </summary>
        public static UnityEvent OnBattleVictory = new UnityEvent();

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
        /// The comms manager to manage network communications if this is a network battle. Not used if the battle isn't a network battle
        /// </summary>
        public Connection.NetworkBattleCommsManager commsManager
        {
            get;
            private set;
        }

        /// <summary>
        /// A list of battle participants who shouldn't have their actions executed.
        /// For example if their pokemon has fainted and they have just changed pokemon
        /// </summary>
        private List<BattleParticipant> battleParticipantsToCancelActionsOf;

        /// <summary>
        /// Whether the victory music has already been started. If the music is triggered early, this will prevent it from being re-triggered
        /// </summary>
        private bool victoryMusicStarted = false;
        
        private void Start()
        {

            textBoxController = TextBoxController.GetTextBoxController(Scene);

            SetUpScene();

        }

        private void OnDestroy()
        {

            //Just to make sure (eg when stopping Play mode on the editor)
            if (battleData.isNetworkBattle)
                StopNetworkBattleNetworking();

            StopCoroutine(mainBattleCoroutine);

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

            //If the battle is a network battle then set up the comms manager
            if (BattleEntranceArguments.battleType == BattleType.Network)
            {
                commsManager = new Connection.NetworkBattleCommsManager(
                    stream: BattleEntranceArguments.networkBattleArguments.stream,
                    serializer: Serialize.DefaultSerializer);
            }

            #region Opponent Participant

            BattleParticipant participantOpponent;

            switch (BattleEntranceArguments.battleType)
            {

                case BattleType.WildPokemon:

                    participantOpponent = new NPCBattleParticipantModes.WildPokemon(null,
                        new PokemonInstance[1] { BattleEntranceArguments.wildPokemonBattleArguments.opponentInstance }
                    );

                    break;

                case BattleType.NPCTrainer:

                    participantOpponent = BattleParticipantNPC.modeInitialisers[BattleEntranceArguments.npcTrainerBattleArguments.trainerDetails.mode](
                        BattleEntranceArguments.npcTrainerBattleArguments.trainerDetails
                    );

                    break;

                case BattleType.Network:

                    participantOpponent = new BattleParticipantNetwork(
                        commsManager,
                        BattleEntranceArguments.networkBattleArguments.opponentName,
                        BattleEntranceArguments.networkBattleArguments.opponentPokemon,
                        BattleEntranceArguments.networkBattleArguments.opponentSpriteResourceName);
                    
                    break;

                default:

                    Debug.LogError("Unknown battle type");

                    //Generate generic opponent instead of crashing or breaking scene/game
                    participantOpponent = BattleParticipantNPC.modeInitialisers[BattleParticipantNPC.Mode.RandomAttack](
                        new TrainersData.TrainerDetails()
                        {
                            id = 0,
                            name = "Erroneous Opponent",
                            pokemonSpecifications = new PokemonInstance.BasicSpecification[1] { new PokemonInstance.BasicSpecification() { speciesId = 1, level = 1 } },
                            defeatMessages = new string[0]
                        }
                    );

                    break;

            }

            #endregion

            #region Player Participant

            BattleParticipantPlayer participantPlayer;

            if (BattleEntranceArguments.battleType == BattleType.Network)
            {
                participantPlayer = new BattleParticipantNetworkedPlayer(commsManager);
            }
            else
            {
                participantPlayer = new BattleParticipantPlayer();
            }

            #endregion

            //If the battle is a network battle then, now that the player and opponent are created, set the comms manager's battle action participants for receiving actions
            if (BattleEntranceArguments.battleType == BattleType.Network)
            {
                commsManager.SetRecvActionParticipants(participantOpponent, participantPlayer);
            }

            #region Battle Data

            battleData = new BattleData()
            {
                participantPlayer = participantPlayer,
                participantOpponent = participantOpponent,
                isWildBattle = BattleEntranceArguments.battleType == BattleType.WildPokemon,
                currentWeatherId = BattleEntranceArguments.initialWeatherId,
                weatherHasBeenChanged = false,
                turnsUntilWeatherFade = 0,
                initialWeatherId = BattleEntranceArguments.initialWeatherId,
                random = BattleEntranceArguments.randomSeed == null ? new System.Random() : new System.Random((int)BattleEntranceArguments.randomSeed),
                isNetworkBattle = BattleEntranceArguments.battleType == BattleType.Network,
                networkStream = BattleEntranceArguments.networkBattleArguments.stream,
                isNetworkClient = !BattleEntranceArguments.networkBattleArguments.isServer
            };

            battleData.participantPlayer.battleManager = this;
            battleData.participantOpponent.battleManager = this;

            if (participantOpponent is BattleParticipantNetwork participantOpponentNetwork)
                participantOpponentNetwork.StartListeningForNetworkComms(battleData.participantPlayer);

            foreach (PokemonInstance pokemon in battleData.participantPlayer.GetPokemon().Where(x => x != null))
                pokemon.ResetBattleProperties();

            foreach (PokemonInstance pokemon in battleData.participantOpponent.GetPokemon().Where(x => x != null))
                pokemon.ResetBattleProperties();

            battleData.participantPlayer.playerBattleUIController = playerBattleUIController;
            battleData.participantPlayer.playerPokemonSelectUIController = playerPokemonSelectUIController;
            battleData.participantPlayer.playerMoveSelectUIController = playerMoveSelectUIController;

            #endregion

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
                    battleData.SetCheatsAllowed(true);
                    battleData.SetExpEVGainEnabled(true);
                    battleData.itemUsagePermissions = BattleData.wildBattleItemUsagePermissions;
                    break;

                case BattleType.NPCTrainer:
                    battleData.SetPlayerCanFlee(false);
                    battleData.SetCheatsAllowed(true);
                    battleData.SetExpEVGainEnabled(true);
                    battleData.itemUsagePermissions = BattleData.trainerBattleItemUsagePermissions;
                    break;

                case BattleType.Network:
                    battleData.SetPlayerCanFlee(false);
                    battleData.SetCheatsAllowed(false);
                    battleData.SetExpEVGainEnabled(false);
                    battleData.itemUsagePermissions = BattleData.networkBattleItemUsagePermissions;
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
                    BattleEntranceArguments.GetOpponentSpriteResourceName()
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

            TryAddSeenOpponentCurrentPokemon();

            //TODO - when and if abilities made, apply them and announce them if needed

            BattleEntranceArguments.argumentsSet = false;

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            battleParticipantsToCancelActionsOf = new List<BattleParticipant>();

            victoryMusicStarted = false;

            battleData.battleTurnNumber = 0;

            #endregion

            #region Main Loop

            RefreshUsedPokemonPerOpposingPokemonRecord(); //In case initial pokemon is instantly switched out

            while (battleData.battleRunning)
            {

                #region Weather Announcement

                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = BattleAnimationSequencer.Animation.Type.WeatherDisplay,
                    weatherDisplayTargetWeatherId = battleData.currentWeatherId
                });

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                #endregion

                #region Action Choosing

                battleData.participantPlayer.StartChoosingAction(battleData);
                battleData.participantOpponent.StartChoosingAction(battleData);

                SetPlayerPokemonBobbingState(true);
                SetTextBoxTextToPlayerActionPrompt();

                yield return new WaitUntil(() =>

                    (battleData.participantPlayer.GetActionHasBeenChosen()
                    && battleData.participantOpponent.GetActionHasBeenChosen())

                    ||

                    (battleData.isNetworkBattle
                    && commsManager.CommsConnErrorOccured)

                );

                SetPlayerPokemonBobbingState(false);
                textBoxController.SetTextInstant("");

                #endregion

                #region Network Connection Error Handling

                if (battleData.isNetworkBattle && commsManager.CommsConnErrorOccured)
                {
                    
                    battleAnimationSequencer.EnqueueSingleText("Connection error occured, ending battle...", true);
                    yield return battleAnimationSequencer.PlayAll();

                    break;

                }

                #endregion

                #region Action Order Deciding

                BattleParticipant.Action[] actionsUnsortedArray = new BattleParticipant.Action[]
                {
                    battleData.participantPlayer.GetChosenAction(),
                    battleData.participantOpponent.GetChosenAction()
                };

                //So that the instances stay in sync in network battles
                if (battleData.isNetworkBattle && battleData.isNetworkClient)
                    actionsUnsortedArray = actionsUnsortedArray.Reverse().ToArray();

                BattleParticipant.Action.PriorityComparer actionComparer = new BattleParticipant.Action.PriorityComparer();

                actionComparer.battleData = battleData;

                var actionsSorted = actionsUnsortedArray.OrderByDescending(
                    x => x,
                    actionComparer
                );

                Queue<BattleParticipant.Action> actionQueue = new Queue<BattleParticipant.Action>(
                    actionsSorted
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
                
                #region Weather Refreshing

                yield return StartCoroutine(RefreshCurrentWeather());

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion
                
                #region Weather Damage

                yield return StartCoroutine(MainBattleCoroutine_ApplyWeatherDamageToParticipant(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(MainBattleCoroutine_ApplyWeatherDamageToParticipant(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion
                
                #region Stage Modifiers

                StartCoroutine(RefreshStageTrickRoom());

                #endregion
                
                #region Non-Volatile Status Conditions

                yield return StartCoroutine(MainBattleCoroutine_ApplyNonVolatileStatusConditionDamageUsingParticipant(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(MainBattleCoroutine_ApplyNonVolatileStatusConditionDamageUsingParticipant(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion
                
                #region Volatile Status Conditions

                #region Bound

                yield return StartCoroutine(RefreshParticipantBound(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantBound(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Curse

                yield return StartCoroutine(RefreshParticipantCurse(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantCurse(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Drowsy

                yield return StartCoroutine(RefreshParticipantDrowsy(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantDrowsy(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Embargo

                yield return StartCoroutine(RefreshParticipantEmbargo(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantEmbargo(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Encore

                yield return StartCoroutine(RefreshParticipantEncore(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantEncore(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Heal Block

                yield return StartCoroutine(RefreshParticipantHealBlock(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantHealBlock(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Leech Seed

                yield return StartCoroutine(RefreshParticipantLeechSeed(battleData.participantPlayer, battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantLeechSeed(battleData.participantOpponent, battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Nightmare

                yield return StartCoroutine(RefreshParticipantNightmare(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantNightmare(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Perish Song

                yield return StartCoroutine(RefreshParticipantPerishSong(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantPerishSong(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Taunt

                yield return StartCoroutine(RefreshParticipantTaunt(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantTaunt(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #endregion
                
                #region Volatile Battle Statuses

                #region Aqua Ring

                yield return StartCoroutine(RefreshParticipantAquaRing(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantAquaRing(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Bracing

                yield return StartCoroutine(RefreshParticipantBracing(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantBracing(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Rooting

                yield return StartCoroutine(RefreshParticipantRooting(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantRooting(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Protection

                yield return StartCoroutine(RefreshParticipantProtection(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantProtection(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Recharging

                yield return StartCoroutine(RefreshParticipantRecharging(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantRecharging(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Thrashing

                yield return StartCoroutine(RefreshParticipantThrashing(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantThrashing(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #region Charging

                yield return StartCoroutine(RefreshParticipantCharging(battleData.participantPlayer));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                yield return StartCoroutine(RefreshParticipantCharging(battleData.participantOpponent));

                yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());

                if (!CheckIfBattleRunning())
                    break;

                #endregion

                #endregion
                
                battleData.participantPlayer.ActivePokemon.battleProperties.volatileStatusConditions.flinch = false;
                battleData.participantOpponent.ActivePokemon.battleProperties.volatileStatusConditions.flinch = false;

                // Reset damage this turn
                battleData.participantPlayer.ActivePokemon.battleProperties.ResetDamageThisTurn();
                battleData.participantOpponent.ActivePokemon.battleProperties.ResetDamageThisTurn();

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

            if (battleData.isNetworkBattle)
            {

                #region Battle End Message

                if (!commsManager.CommsConnErrorOccured)
                {

                    if (battleData.participantPlayer.CheckIfDefeated()) //If player is defeated, it counts as a loss
                    {

                        battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name
                            + " was defeated by "
                            + battleData.participantOpponent.GetName(), true);

                    }
                    else
                    {

                        TryTriggerVictoryMusic();

                        battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name
                            + " defeated "
                            + battleData.participantOpponent.GetName(), true);

                        OnBattleVictory.Invoke();
                        OnBattleVictory.RemoveAllListeners();

                    }

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                }

                #endregion

                CloseNetworkBattle();

            }
            else if (battleData.participantPlayer.CheckIfDefeated()) //If player is defeated, it counts as a loss
            {

                OnBattleVictory.RemoveAllListeners();

                //If player lost (a draw counts as the player losing)

                #region Out of Usable Pokemon Message

                battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name + " ran out of usable pokemon...", true);

                #endregion

                #region Dropping Money

                int moneyToDrop = PlayerData.singleton.profile.money / 2;

                PlayerData.singleton.AddMoney(-moneyToDrop);

                string moneyDropMessage = battleData.isWildBattle ? "dropped" : "handed over";

                battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name + ' ' + moneyDropMessage + " " + PlayerData.currencySymbol + moneyToDrop + "...");

                #endregion

                #region Blacked Out

                battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name + " blacked out!");

                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = BattleAnimationSequencer.Animation.Type.Blackout
                });

                #endregion

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                MusicSourceController.singleton.StopMusic();

                PlayerController.singleton.Respawn();

            }
            else
            {

                if (!battleData.playerFled)
                {

                    TryTriggerVictoryMusic();

                    if (battleData.isWildBattle)
                    {

                        if (!battleData.opponentWasCaptured)
                        {

                            battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name
                                + " defeated the wild "
                                + battleData.participantOpponent.ActivePokemon.GetDisplayName());

                            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                        }

                    }
                    else if (battleData.participantOpponent is BattleParticipantNPC opponentNPCParticipant)
                    {

                        //Opponent defeat messages
                        if (opponentNPCParticipant.defeatMessages.Length > 0)
                        {

                            Sprite opponentTrainerBattleSprite = SpriteStorage.GetCharacterBattleSprite(
                                BattleEntranceArguments.npcTrainerBattleArguments.trainerDetails.GetBattleSpriteResourceName()
                            );

                            //Sprite showcase start
                            if (opponentTrainerBattleSprite != null)
                            {
                                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                                {
                                    type = BattleAnimationSequencer.Animation.Type.OpponentTrainerShowcaseStart,
                                    opponentTrainerShowcaseSprite = opponentTrainerBattleSprite
                                });
                            }

                            //Messages
                            foreach (string message in opponentNPCParticipant.defeatMessages)
                                battleAnimationSequencer.EnqueueSingleText(message, true);

                            //Sprite showcase end
                            if (opponentTrainerBattleSprite != null)
                            {
                                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                                {
                                    type = BattleAnimationSequencer.Animation.Type.OpponentTrainerShowcaseStop
                                });
                            }

                            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                        }

                        //Payout
                        if (opponentNPCParticipant.basePayout > 0)
                        {

                            int playerPrizeMoney = CalculateTrainerOpponentPrizeMoney(opponentNPCParticipant);

                            PlayerData.singleton.AddMoney(playerPrizeMoney);

                            battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name
                                + " defeated "
                                + opponentNPCParticipant.GetName());

                            battleAnimationSequencer.EnqueueSingleText(PlayerData.singleton.profile.name
                                + " was given "
                                + PlayerData.currencySymbol
                                + playerPrizeMoney.ToString()
                                + " for winning",
                                true);

                            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                        }

                    }

                    OnBattleVictory.Invoke();
                    OnBattleVictory.RemoveAllListeners();

                }

                MusicSourceController.singleton.StopMusic();
                GameSceneManager.CloseBattleScene();

            }

            #endregion

        }

        /// <summary>
        /// If the victory music hasn't already been triggered, triggers it
        /// </summary>
        private void TryTriggerVictoryMusic()
        {

            if (!victoryMusicStarted)
            {

                if (battleData.isWildBattle)
                    MusicSourceController.singleton.SetTrack("victory_wild", true);
                else
                    MusicSourceController.singleton.SetTrack("victory_trainer", true);

                victoryMusicStarted = true;

            }

        }

        /// <summary>
        /// Stops networking fuctionality for a network battle (eg closing the network stream, stopping refreshing threads etc)
        /// </summary>
        private void StopNetworkBattleNetworking()
        {

            battleData?.networkStream?.Close();
            commsManager.StopListening();
            if (battleData?.participantOpponent is BattleParticipantNetwork netOpp)
            {
                netOpp.StopRefreshingForNetworkComms();
            }

        }

        /// <summary>
        /// Close sockets/network streams and stop a network battle by stopping music and returning the player to where they launched the battle
        /// </summary>
        private void CloseNetworkBattle()
        {

            //After a network battle, the player's pokemon should be fully restored
            PlayerData.singleton.HealPartyPokemon();

            StopNetworkBattleNetworking();

            MusicSourceController.singleton.StopMusic();
            GameSceneManager.CloseBattleScene();

        }

        private IEnumerator MainBattleCoroutine_CheckPokemonFainted()
        {

            #region Pokemon Fainting Animation and Fainting Management

            PokemonInstance playerActivePokemon = battleData.participantPlayer.ActivePokemon;
            if (playerActivePokemon.IsFainted)
            {

                playerActivePokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;

                #region Reduce Friendship

                playerActivePokemon.RemoveFriendshipForFaint(battleData.participantOpponent.ActivePokemon);

                #endregion

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

                if (battleData.expEvGainEnabled)
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

                yield return new WaitUntil(() => battleData.participantPlayer.GetNextPokemonHasBeenChosen());

                textBoxController.SetTextInstant("");

                battleData.participantPlayer.activePokemonIndex = battleData.participantPlayer.GetChosenNextPokemonIndex();

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

                yield return new WaitUntil(() => battleData.participantOpponent.GetNextPokemonHasBeenChosen());

                battleData.participantOpponent.activePokemonIndex = battleData.participantOpponent.GetChosenNextPokemonIndex();

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
                        if (!pokemon.moveIds.Contains(moveId))
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

        public void SetTextBoxTextToWaitingForOpponent()
            => textBoxController.SetTextInstant("Waiting for opponent...");

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

        private void TryAddSeenOpponentCurrentPokemon()
        {
            if (!battleData.opponentPokemonSeenRecorded[battleData.participantOpponent.activePokemonIndex])
            {

                PlayerData.singleton.pokedex.AddPokemonSeen(battleData.participantOpponent.ActivePokemon);
                battleData.opponentPokemonSeenRecorded[battleData.participantOpponent.activePokemonIndex] = true;

            }
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

                if (battleData.RandomRange(0F, 1F) < PokemonInstance.thawChancePerTurn)
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

        #region Volatile Status Conditions

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

        private IEnumerator RefreshParticipantBound(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileStatusConditions.bound > 0)
            {

                participantPokemon.battleProperties.volatileStatusConditions.bound--;

                if (participantPokemon.battleProperties.volatileStatusConditions.bound <= 0)
                {

                    battleAnimationSequencer.EnqueueSingleText(
                        participantPokemon.GetDisplayName()
                        + " escaped from its bonds!"
                        );

                }
                else
                {

                    //Being bound deals 1/16 of the target pokemon's max health

                    int damageToDeal = participantPokemon.GetStats().health / 16;

                    int initialHealth = participantPokemon.health;

                    participantPokemon.TakeDamage(damageToDeal);

                    battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName() + " was hurt by being bound");
                    battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(participant.ActivePokemon, initialHealth, participant is BattleParticipantPlayer));

                }

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

        }

        private IEnumerator RefreshParticipantCurse(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileStatusConditions.curse)
            {

                //Being cursed deals 1/4 of the target pokemon's max health

                int damageToDeal = participantPokemon.GetStats().health / 4;

                int initialHealth = participantPokemon.health;

                participantPokemon.TakeDamage(damageToDeal);

                battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName() + " was hurt by its curse");
                battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(participant.ActivePokemon, initialHealth, participant is BattleParticipantPlayer));

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

        }

        private IEnumerator RefreshParticipantDrowsy(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            switch (participantPokemon.battleProperties.volatileStatusConditions.drowsyStage)
            {

                case 2:

                    participantPokemon.battleProperties.volatileStatusConditions.drowsyStage = 1;

                    break;

                case 1:

                    if (participantPokemon.nonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.None)
                    {
                        participantPokemon.battleProperties.volatileStatusConditions.drowsyStage = 0;
                        break;
                    }

                    participantPokemon.battleProperties.volatileStatusConditions.drowsyStage = 0;
                    participantPokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.Asleep;

                    battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName()
                        + ' '
                        + PokemonInstance.nonVolatileStatusConditionMessages[PokemonInstance.NonVolatileStatusCondition.Asleep]);

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                    break;

                case 0:
                    break;

                default:
                    Debug.LogWarning("Invalid drowsy stage");
                    break;

            }

        }

        private IEnumerator RefreshParticipantEmbargo(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileStatusConditions.embargo > 0)
            {

                participantPokemon.battleProperties.volatileStatusConditions.embargo--;

                if (participantPokemon.battleProperties.volatileStatusConditions.embargo <= 0)
                {

                    battleAnimationSequencer.EnqueueSingleText(
                        participantPokemon.GetDisplayName()
                        + " escaped from the embargo!"
                        );

                }

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

        }

        private IEnumerator RefreshParticipantEncore(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileStatusConditions.encoreTurns > 0)
            {

                participantPokemon.battleProperties.volatileStatusConditions.encoreTurns--;

                if (participantPokemon.battleProperties.volatileStatusConditions.encoreTurns <= 0)
                {

                    battleAnimationSequencer.EnqueueSingleText(
                        participantPokemon.GetDisplayName()
                        + " escaped from the encore!"
                        );

                }

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

        }

        private IEnumerator RefreshParticipantHealBlock(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileStatusConditions.healBlock > 0)
            {

                participantPokemon.battleProperties.volatileStatusConditions.healBlock--;

                if (participantPokemon.battleProperties.volatileStatusConditions.healBlock <= 0)
                {

                    battleAnimationSequencer.EnqueueSingleText(
                        participantPokemon.GetDisplayName()
                        + " escaped from the heal block!"
                        );

                }

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

        }

        private IEnumerator RefreshParticipantLeechSeed(BattleParticipant participant, BattleParticipant opponent)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;
            PokemonInstance opponentPokemon = opponent.ActivePokemon;

            if (participantPokemon.battleProperties.volatileStatusConditions.leechSeed)
            {

                // Leech seed leeches 1/8 of a pokemon's maximum health
                int damageToDeal = participantPokemon.GetStats().health / 8;

                int initialHealth = participantPokemon.health;

                // Damage pokemon
                participantPokemon.TakeDamage(damageToDeal);

                int damageDone = initialHealth - participantPokemon.health;

                // Heal opponent
                int opponentInitialHealth = opponentPokemon.health;
                opponentPokemon.HealHealth(damageDone);

                battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName() + " had its health leeched");

                battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(participant.ActivePokemon, initialHealth, participant is BattleParticipantPlayer));

                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = opponent is BattleParticipantPlayer
                        ? BattleAnimationSequencer.Animation.Type.PlayerHealHealth
                        : BattleAnimationSequencer.Animation.Type.OpponentHealHealth,
                    takeDamageOldHealth = opponentInitialHealth,
                    takeDamageNewHealth = opponentPokemon.health,
                    takeDamageMaxHealth = opponentPokemon.GetStats().health
                });

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

        }

        private IEnumerator RefreshParticipantNightmare(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.nonVolatileStatusCondition != PokemonInstance.NonVolatileStatusCondition.Asleep)
                participantPokemon.battleProperties.volatileStatusConditions.nightmare = false;

            if (participantPokemon.battleProperties.volatileStatusConditions.nightmare)
            {

                //Nightmares inflict 1/4 of a pokemon's maximum health each turn

                int damageToDeal = participantPokemon.GetStats().health / 4;

                int initialHealth = participantPokemon.health;

                participantPokemon.TakeDamage(damageToDeal);

                battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName() + " is having nightmares");
                battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(participant.ActivePokemon, initialHealth, participant is BattleParticipantPlayer));

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

        }

        private IEnumerator RefreshParticipantPerishSong(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileStatusConditions.perishSong > 0)
            {

                participantPokemon.battleProperties.volatileStatusConditions.perishSong--;

                if (participantPokemon.battleProperties.volatileStatusConditions.perishSong == 0)
                {

                    participantPokemon.battleProperties.volatileStatusConditions.perishSong = -1;

                    int damageToDeal = participantPokemon.health;

                    int initialHealth = participantPokemon.health;

                    participantPokemon.TakeDamage(damageToDeal);

                    battleAnimationSequencer.EnqueueSingleText(participant.ActivePokemon.GetDisplayName() + " fell to the perish song!");
                    battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(participant.ActivePokemon, initialHealth, participant is BattleParticipantPlayer));

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                }

            }

        }

        private IEnumerator RefreshParticipantTaunt(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileStatusConditions.tauntTurns > 0)
            {

                participantPokemon.battleProperties.volatileStatusConditions.tauntTurns--;

                if (participantPokemon.battleProperties.volatileStatusConditions.tauntTurns <= 0)
                {

                    battleAnimationSequencer.EnqueueSingleText(
                        participantPokemon.GetDisplayName()
                        + " escaped from the taunt!"
                        );

                }

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

        }

        #endregion

        #region Volatile Battle Statuses

        private IEnumerator RefreshParticipantAquaRing(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileBattleStatus.aquaRing && (participantPokemon.health < participantPokemon.GetStats().health))
            {

                int initialHealth = participantPokemon.health;

                //Aqua ring heals 1/16th of pokemon's maximum health each turn
                int healthToHeal = participantPokemon.GetStats().health / 16;

                //Heal pokemon
                participantPokemon.HealHealth(healthToHeal);

                battleAnimationSequencer.EnqueueSingleText(participantPokemon.GetDisplayName() + " was healed by its aqua ring");

                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = participant is BattleParticipantPlayer
                        ? BattleAnimationSequencer.Animation.Type.PlayerHealHealth
                        : BattleAnimationSequencer.Animation.Type.OpponentHealHealth,
                    takeDamageOldHealth = initialHealth,
                    takeDamageNewHealth = participantPokemon.health,
                    takeDamageMaxHealth = participantPokemon.GetStats().health
                });

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

        }

        private IEnumerator RefreshParticipantBracing(BattleParticipant participant)
        {

            //Bracing only lasts for 1 turn

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileBattleStatus.bracing)
                participantPokemon.battleProperties.volatileBattleStatus.bracing = false;

            yield break;

        }

        private IEnumerator RefreshParticipantRooting(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileBattleStatus.rooting && (participantPokemon.health < participantPokemon.GetStats().health))
            {

                int initialHealth = participantPokemon.health;

                //Rooting heals 1/16th of pokemon's maximum health each turn
                int healthToHeal = participantPokemon.GetStats().health / 16;

                //Heal pokemon
                participantPokemon.HealHealth(healthToHeal);

                battleAnimationSequencer.EnqueueSingleText(participantPokemon.GetDisplayName() + " absorbed energy from the ground");

                battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                {
                    type = participant is BattleParticipantPlayer
                        ? BattleAnimationSequencer.Animation.Type.PlayerHealHealth
                        : BattleAnimationSequencer.Animation.Type.OpponentHealHealth,
                    takeDamageOldHealth = initialHealth,
                    takeDamageNewHealth = participantPokemon.health,
                    takeDamageMaxHealth = participantPokemon.GetStats().health
                });

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

        }

        private IEnumerator RefreshParticipantProtection(BattleParticipant participant)
        {

            //Protection only lasts for 1 turn

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileBattleStatus.protection)
                participantPokemon.battleProperties.volatileBattleStatus.protection = false;

            yield break;

        }

        private IEnumerator RefreshParticipantRecharging(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            switch (participantPokemon.battleProperties.volatileBattleStatus.rechargingStage)
            {

                case 2:
                case 1:

                    participantPokemon.battleProperties.volatileBattleStatus.rechargingStage--;

                    break;

                case 0:
                    break;

                default:
                    Debug.LogWarning("Invalid recharging stage");
                    break;

            }

            yield break;

        }

        private IEnumerator RefreshParticipantThrashing(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            if (participantPokemon.battleProperties.volatileBattleStatus.thrashTurns > 0)
            {

                participantPokemon.battleProperties.volatileBattleStatus.thrashTurns--;

                if (participantPokemon.battleProperties.volatileBattleStatus.thrashTurns == 0)
                {

                    battleAnimationSequencer.EnqueueSingleText(participantPokemon.GetDisplayName() + " calmed down...");

                    //Confuse if not already confused
                    if (participantPokemon.battleProperties.volatileStatusConditions.confusion <= 0)
                    {
                        participantPokemon.battleProperties.volatileStatusConditions.confusion = battleData.RandomRange(2, 6);
                        battleAnimationSequencer.EnqueueSingleText(participantPokemon.GetDisplayName() + " became confused from its thrashing!");
                    }

                }

            }

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

        }

        private IEnumerator RefreshParticipantCharging(BattleParticipant participant)
        {

            PokemonInstance participantPokemon = participant.ActivePokemon;

            switch (participantPokemon.battleProperties.volatileBattleStatus.chargingStage)
            {

                case 2:
                case 1:

                    participantPokemon.battleProperties.volatileBattleStatus.chargingStage--;

                    break;

                case 0:
                    break;

                default:
                    Debug.LogWarning("Invalid charging stage");
                    break;

            }

            yield break;

        }

        #endregion

        #region Stage Modifiers

        private IEnumerator RefreshStageTrickRoom()
        {

            if (battleData.stageModifiers.trickRoomRemainingTurns > 0)
            {

                battleData.stageModifiers.trickRoomRemainingTurns--;

                if (battleData.stageModifiers.trickRoomRemainingTurns <= 0)
                {

                    battleAnimationSequencer.EnqueueSingleText("Things started to feel normal again");
                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                }

            }

        }

        #endregion

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

                    //If pokemon holding lucky egg, multiply by 1.5
                    if (playerPokemonInstance.heldItem != null && playerPokemonInstance.heldItem.id == 231)
                        experienceToAdd += Mathf.FloorToInt(experienceToAdd * 0.5F);

                    //If pokemon was traded (aka isn't with original trainer), multiply by 1.5
                    if (playerPokemonInstance.originalTrainerGuid != PlayerData.singleton.profile.guid)
                        experienceToAdd += Mathf.FloorToInt(experienceToAdd * 0.5F);

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

                        if (playerPokemonInstance.heldItem == null || playerPokemonInstance.heldItem.id != 229) //Only consider evolving if not holding everstone
                        {

                            PokemonSpecies.Evolution evolution = playerPokemonInstance.TryFindEvolution();

                            if (evolution != null)
                            {
                                yield return StartCoroutine(EvolvePokemon(playerPokemonInstance,
                                    evolution,
                                    playerPokemonIndex == battleData.participantPlayer.activePokemonIndex));
                            }

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
            PokemonSpecies.Evolution evolution,
            bool isActivePokemon)
        {

            EvolutionScene.EvolutionSceneController.entranceArguments = new EvolutionScene.EvolutionSceneController.EntranceArguments()
            {
                pokemon = pokemon,
                evolution = evolution
            };

            DisableScene();

            GameSceneManager.LaunchEvolutionScene();

            readyToCarryOnAfterEvolution = false;
            GameSceneManager.EvolutionSceneClosed += () =>
            {
                readyToCarryOnAfterEvolution = true;
            };

            yield return new WaitUntil(() => readyToCarryOnAfterEvolution);

            if (isActivePokemon)
                battleLayoutController.UpdatePlayerPokemon(pokemon);

            EnableScene();
            
        }

        #region Weather

        /// <summary>
        /// Sets the current weather of the battle assuming it will be changed back later when its duration is finished. Also performs animation for changing weather
        /// </summary>
        private void SetChangedWeather(int newWeatherId,
            int duration = 5)
        {

            battleData.SetChangedWeather(newWeatherId, duration);

        }

        private IEnumerator RefreshCurrentWeather()
        {

            if (battleData.weatherHasBeenChanged)
            {

                battleData.turnsUntilWeatherFade--;

                if (battleData.turnsUntilWeatherFade <= 0)
                {

                    int newWeatherId = battleData.RevertToInitialWeather();
                    battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                    {
                        type = BattleAnimationSequencer.Animation.Type.WeatherDisplay,
                        weatherDisplayTargetWeatherId = newWeatherId
                    });

                }

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }

        }

        #endregion

        #region Action Execution

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

                case BattleParticipant.Action.Type.Recharge:
                    yield return StartCoroutine(ExecuteAction_Recharging(action));
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

            PokemonInstance userPokemon = action.user.ActivePokemon;

            if (userPokemon.battleProperties.volatileStatusConditions.flinch)
                yield break;

            #region NVSC Move Failures

            if (userPokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Asleep)
            {

                //If move missed, cancel thrashing
                userPokemon.battleProperties.volatileBattleStatus.thrashTurns = 0;

                battleAnimationSequencer.EnqueueSingleText(
                    userPokemon.GetDisplayName()
                    + " is fast asleep");

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                yield break;

            }

            if (userPokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Paralysed)
            {

                if (battleData.RandomRange(0F, 1F) < PokemonInstance.paralysisFightFailChance)
                {

                    //If move missed, cancel thrashing
                    userPokemon.battleProperties.volatileBattleStatus.thrashTurns = 0;

                    battleAnimationSequencer.EnqueueSingleText(
                        userPokemon.GetDisplayName()
                        + " is paralysed and couldn't move!"
                        );

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                    yield break;

                }

            }

            if (userPokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.Frozen)
            {

                //If move missed, cancel thrashing
                userPokemon.battleProperties.volatileBattleStatus.thrashTurns = 0;

                battleAnimationSequencer.EnqueueSingleText(
                        userPokemon.GetDisplayName()
                        + " is frozen solid!"
                        );

                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                yield break;

            }

            #endregion

            #region Confusion Move Failure

            if (userPokemon.battleProperties.volatileStatusConditions.confusion > 0)
            {

                battleAnimationSequencer.EnqueueSingleText(
                    userPokemon.GetDisplayName()
                    + " is confused"
                    );

                if (battleData.RandomValue01() <= PokemonInstance.BattleProperties.VolatileStatusConditions.confusionPokemonDamageChance)
                {

                    //If move missed, cancel thrashing
                    userPokemon.battleProperties.volatileBattleStatus.thrashTurns = 0;

                    battleAnimationSequencer.EnqueueSingleText(
                        userPokemon.GetDisplayName()
                        + " hurt itself in its confusion!"
                    );

                    int previousHealth = userPokemon.health;
                    Stats<int> userBattleStats = userPokemon.GetBattleStats();

                    userPokemon.TakeDamage(PokemonMove.CalculateNormalDamageToDeal(
                        userPokemon.GetLevel(),
                        PokemonInstance.BattleProperties.VolatileStatusConditions.confusionUserHarmPower,
                        ((float)userBattleStats.attack) / userBattleStats.defense,
                        1
                    ));

                    battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                    {
                        type = userIsPlayer ? BattleAnimationSequencer.Animation.Type.PlayerTakeDamage : BattleAnimationSequencer.Animation.Type.OpponentTakeDamage,
                        takeDamageMaxHealth = userPokemon.GetStats().health,
                        takeDamageNewHealth = userPokemon.health,
                        takeDamageOldHealth = previousHealth
                    });

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());

                    yield return StartCoroutine(MainBattleCoroutine_CheckPokemonFainted());
                    yield break;

                }

            }

            #endregion

            #region Infatuated Move Failure

            if (userPokemon.battleProperties.volatileStatusConditions.infatuated)
            {

                battleAnimationSequencer.EnqueueSingleText(
                    userPokemon.GetDisplayName()
                    + " is in love"
                    );

                if (battleData.RandomValue01() <= PokemonInstance.BattleProperties.VolatileStatusConditions.infatuatedMoveFailChance)
                {

                    //If move missed, cancel thrashing
                    userPokemon.battleProperties.volatileBattleStatus.thrashTurns = 0;

                    battleAnimationSequencer.EnqueueSingleText(
                        userPokemon.GetDisplayName()
                        + " couldn't bear to move!"
                    );

                    yield return StartCoroutine(battleAnimationSequencer.PlayAll());
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

                if (userPokemon.movePPs[action.fightMoveIndex] <= 0)
                {
                    throw new ArgumentException("Participant selected move with 0 PP (Move Index " + action.fightMoveIndex + ")");
                }

                move = PokemonMove.GetPokemonMoveById(userPokemon.moveIds[action.fightMoveIndex]);

            }

            #region Move PP down

            if (!action.fightUsingStruggle)
                userPokemon.movePPs[action.fightMoveIndex]--;

            #endregion

            string moveUsageMessage = userPokemon.GetDisplayName() + " used " + move.name;
            battleAnimationSequencer.EnqueueSingleText(moveUsageMessage);

            PokemonMove.UsageResults usageResults = move.CalculateEffect(
                userPokemon,
                action.fightMoveTarget.ActivePokemon,
                battleData
            );

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            #region Effects

            //This may not be the same as the moveHitCount calculated since the target or user may faint during the attacking
            byte multiHitsLanded = 0;

            if (usageResults.Succeeded)
            {

                byte moveHitCount = usageResults.hitCount;

                for (int i = 0; i < moveHitCount; i++)
                {

                    multiHitsLanded++;

                    #region Semi-Invulnerable Animation

                    switch (usageResults.setSemiInvulnerable)
                    {

                        case true:
                            battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation
                            {
                                type = BattleAnimationSequencer.Animation.Type.PokemonSemiInvulnerableStart,
                                pokemonSemiInvulnerableParticipantIsPlayer = userIsPlayer
                            });
                            break;

                        case false:
                            battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation
                            {
                                type = BattleAnimationSequencer.Animation.Type.PokemonSemiInvulnerableStop,
                                pokemonSemiInvulnerableParticipantIsPlayer = userIsPlayer
                            });
                            break;

                    }

                    #endregion

                    if (!usageResults.setCharging) //Don't show move animation if this is the charging stage of the move
                    {
                        battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation
                        {
                            type = BattleAnimationSequencer.Animation.Type.PokemonMove,
                            pokemonMoveId = move.id,
                            pokemonMovePlayerIsUser = userIsPlayer
                        });
                    }

                    #region Target Effects

                    PokemonInstance targetPokemon = action.fightMoveTarget.ActivePokemon;

                    if (targetPokemon.battleProperties.volatileBattleStatus.protection && !move.noOpponentEffects)
                    {

                        battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " protected itself!");

                    }
                    else
                    {

                        #region Target Damage

                        //No need to animate damage dealing or try take damage is no damage is dealt
                        if (usageResults.targetDamageDealt > 0)
                        {

                            int targetInitialHealth = targetPokemon.health;

                            int damageDealt = targetPokemon.TakeDamage(usageResults.targetDamageDealt);
                            targetPokemon.battleProperties.AddDamageThisTurn(damageDealt);

                            battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(targetPokemon, targetInitialHealth, !userIsPlayer));

                        }

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

                        #region User Healing

                        //User healing done here so that it isn't skipped if the move defeats the opponent

                        if (usageResults.userHealthHealed > 0)
                        {

                            int userInitialHealth = userPokemon.health;

                            userPokemon.HealHealth(usageResults.userHealthHealed);

                            battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                            {
                                type = action.user is BattleParticipantPlayer
                                    ? BattleAnimationSequencer.Animation.Type.PlayerHealHealth
                                    : BattleAnimationSequencer.Animation.Type.OpponentHealHealth,
                                takeDamageOldHealth = userInitialHealth,
                                takeDamageNewHealth = userPokemon.health,
                                takeDamageMaxHealth = userPokemon.GetStats().health
                            });
                            battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " recovered some health");

                        }

                        #endregion

                        //Stop loop if target fainted
                        if (targetPokemon.IsFainted)
                            break;

                        #endregion

                        #region Target Healing

                        if (usageResults.targetHealthHealed > 0)
                        {

                            int targetInitialHealth = targetPokemon.health;

                            targetPokemon.HealHealth(usageResults.targetHealthHealed);

                            battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                            {
                                type = action.fightMoveTarget is BattleParticipantPlayer
                                    ? BattleAnimationSequencer.Animation.Type.PlayerHealHealth
                                    : BattleAnimationSequencer.Animation.Type.OpponentHealHealth,
                                takeDamageOldHealth = targetInitialHealth,
                                takeDamageNewHealth = targetPokemon.health,
                                takeDamageMaxHealth = targetPokemon.GetStats().health
                            });
                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " recovered some health");

                        }

                        #endregion

                        #region Volatile Status Conditions

                        #region Target Confusion

                        if (usageResults.targetConfuse)
                        {

                            //Confusion should last between 1-4 turns decided randomly
                            targetPokemon.battleProperties.volatileStatusConditions.confusion = PokemonInstance.BattleProperties.VolatileStatusConditions.GetRandomConfusionDuration(battleData);

                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " became confused!");
                            //TODO - enqueue confusion animation

                        }

                        #endregion

                        #region Bound

                        if (usageResults.boundTurns > 0 && targetPokemon.battleProperties.volatileStatusConditions.bound <= 0)
                        {

                            targetPokemon.battleProperties.volatileStatusConditions.bound = usageResults.boundTurns;

                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " was bound!");

                        }

                        #endregion

                        #region Cant Escape

                        if (usageResults.inflictCantEscape)
                        {

                            targetPokemon.battleProperties.volatileStatusConditions.cantEscape = true;

                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " was locked into the battle!");

                        }

                        #endregion

                        #region Curse

                        if (usageResults.inflictCurse)
                        {

                            targetPokemon.battleProperties.volatileStatusConditions.curse = true;

                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " was cursed!");

                        }

                        #endregion

                        #region Drowsy

                        if (usageResults.inflictDrowsy)
                        {

                            targetPokemon.battleProperties.volatileStatusConditions.drowsyStage = 2;

                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " is feeling sleepy");

                        }

                        #endregion

                        #region Embargo

                        if (usageResults.inflictEmbargo)
                        {

                            targetPokemon.battleProperties.volatileStatusConditions.embargo = 6; //Lasts 5 turns but counter will be reduced at end of this turn which shouldn't count

                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " feels on-edge");

                        }

                        #endregion

                        #region Encore

                        if (usageResults.encoreTurns > 0)
                        {

                            targetPokemon.battleProperties.volatileStatusConditions.encoreTurns = usageResults.encoreTurns;
                            targetPokemon.battleProperties.volatileStatusConditions.encoreMoveId = targetPokemon.battleProperties.lastMoveId;

                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " must provide an encore!");

                        }

                        #endregion

                        #region Heal Block

                        if (usageResults.inflictHealBlock)
                        {

                            targetPokemon.battleProperties.volatileStatusConditions.healBlock = 6; //One more than the intended 5 turns as the counter is decremented on this turn

                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " was stopped from healing itself");

                        }

                        #endregion

                        #region Identified

                        if (usageResults.inflictIdentified)
                        {
                            targetPokemon.battleProperties.volatileStatusConditions.identified = true;
                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " was identified!");
                        }

                        #endregion

                        #region Infatuated

                        if (usageResults.inflictInfatuated)
                        {
                            targetPokemon.battleProperties.volatileStatusConditions.infatuated = true;
                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " has fallen in love!");
                        }

                        #endregion

                        #region Leech Seed

                        if (usageResults.inflictLeechSeed)
                        {
                            targetPokemon.battleProperties.volatileStatusConditions.leechSeed = true;
                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " was leeched!");
                        }

                        #endregion

                        #region Nightmare

                        if (usageResults.inflictNightmare)
                        {

                            targetPokemon.battleProperties.volatileStatusConditions.nightmare = true;

                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " started having nightmares!");

                        }

                        #endregion

                        #region Perish Song

                        if (usageResults.inflictPerishSong)
                        {

                            if (userPokemon.battleProperties.volatileStatusConditions.perishSong < 0)
                                userPokemon.battleProperties.volatileStatusConditions.perishSong = 4;

                            if (targetPokemon.battleProperties.volatileStatusConditions.perishSong < 0)
                                targetPokemon.battleProperties.volatileStatusConditions.perishSong = 4;

                            battleAnimationSequencer.EnqueueSingleText("The pokemon don't feel so good..."); //Marvel's Avengers: Infinity War reference

                        }

                        #endregion

                        #region Taunt

                        if (usageResults.tauntTurns > 0)
                        {
                            targetPokemon.battleProperties.volatileStatusConditions.tauntTurns = usageResults.tauntTurns;
                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " fell for the taunt!");
                        }

                        #endregion

                        #region Torment

                        if (usageResults.inflictTorment)
                        {
                            targetPokemon.battleProperties.volatileStatusConditions.torment = true;
                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " is being tormented!");
                        }

                        #endregion

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
                        else if (usageResults.clearTargetNonVolatileStatusCondition)
                        {

                            targetPokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;

                            battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " recovered from its condition");

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

                    }

                    #endregion

                    #region User Effects

                    #region Volatile Battle Statuses

                    #region Aqua Ring

                    if (usageResults.setAquaRing)
                    {
                        userPokemon.battleProperties.volatileBattleStatus.aquaRing = true;
                        battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " was veiled by water");
                    }

                    #endregion

                    #region Bracing

                    if (usageResults.setBracing)
                    {
                        userPokemon.battleProperties.volatileBattleStatus.bracing = true;
                        battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " braces itself");
                    }

                    #endregion

                    #region Defense Curl

                    if (usageResults.setDefenseCurl)
                    {
                        userPokemon.battleProperties.volatileBattleStatus.defenseCurl = true;
                    }

                    #endregion

                    #region Rooting

                    if (usageResults.setRooting)
                    {
                        userPokemon.battleProperties.volatileBattleStatus.rooting = true;
                        battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " planted its roots");
                    }

                    #endregion

                    #region Protection

                    if (usageResults.setProtection)
                    {
                        userPokemon.battleProperties.volatileBattleStatus.protection = true;
                        battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " protects itself");
                        userPokemon.battleProperties.consecutiveProtectionMoves++;
                    }
                    else
                    {
                        //If move isn't protection move, reset user consecutive protection moves
                        userPokemon.battleProperties.ResetConsevutiveProtectionMoves();
                    }

                    #endregion

                    #region Recharging

                    if (usageResults.setRecharging)
                    {
                        userPokemon.battleProperties.volatileBattleStatus.rechargingStage = 2;
                    }

                    #endregion

                    #region Taking Aim

                    if (usageResults.setTakingAim)
                    {
                        userPokemon.battleProperties.volatileBattleStatus.takingAim = true;
                        battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " locked on");
                    }

                    if (usageResults.unsetTakingAim)
                    {
                        userPokemon.battleProperties.volatileBattleStatus.takingAim = false;
                    }

                    #endregion

                    #region Thrashing

                    if (usageResults.thrashingTurns > 0)
                    {
                        userPokemon.battleProperties.volatileBattleStatus.thrashTurns = usageResults.thrashingTurns;
                        userPokemon.battleProperties.volatileBattleStatus.thrashMoveId = move.id;
                    }

                    #endregion

                    #region Charging and Semi-Invulnerable

                    if (usageResults.setCharging)
                    {

                        userPokemon.battleProperties.volatileBattleStatus.chargingStage = 2;
                        userPokemon.battleProperties.volatileBattleStatus.chargingMoveId = move.id;

                        battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " started charging");

                    }

                    switch (usageResults.setSemiInvulnerable)
                    {

                        case true:
                            userPokemon.battleProperties.volatileBattleStatus.semiInvulnerable = true;
                            break;

                        case false:
                            userPokemon.battleProperties.volatileBattleStatus.semiInvulnerable = false;
                            break;

                    }

                    #endregion

                    #region Stockpile

                    if (usageResults.stockpileChange > 0)
                    {
                        battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " stockpiled " + usageResults.stockpileChange.ToString());
                        userPokemon.battleProperties.volatileBattleStatus.stockpileAmount += usageResults.stockpileChange;
                    }
                    else if (usageResults.stockpileChange < 0)
                    {
                        battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " used " + Mathf.Abs(usageResults.stockpileChange).ToString() + " stockpile");
                        userPokemon.battleProperties.volatileBattleStatus.stockpileAmount += usageResults.stockpileChange;
                    }

                    #endregion

                    #region Electric Charging

                    if (usageResults.setElectricCharged == true)
                    {
                        userPokemon.battleProperties.volatileBattleStatus.electricCharged = true;
                        battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " charged itself");
                    }
                    else if (usageResults.setElectricCharged == false)
                    {
                        userPokemon.battleProperties.volatileBattleStatus.electricCharged = false;
                    }

                    #endregion

                    #endregion

                    #region User Damage

                    if (usageResults.userDamageDealt > 0)
                    {

                        int userInitialHealth = userPokemon.health;

                        userPokemon.TakeDamage(usageResults.userDamageDealt);

                        battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " was hurt from the recoil");
                        battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(userPokemon, userInitialHealth, action.user is BattleParticipantPlayer));

                    }

                    //Stop loop if user fainted
                    if (userPokemon.IsFainted)
                        break;

                    #endregion

                    #region Non-Volatile Status Condition

                    if (usageResults.clearUserNonVolatileStatusCondition)
                    {

                        userPokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.None;
                        battleAnimationSequencer.EnqueueSingleText(userPokemon.GetDisplayName() + " was cleared of its status condition");

                        if (userIsPlayer)
                            battleLayoutController.overviewPaneManager.opponentPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(userPokemon.nonVolatileStatusCondition);
                        else
                            battleLayoutController.overviewPaneManager.playerPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(userPokemon.nonVolatileStatusCondition);

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

                    #region Set Last-Used Move

                    userPokemon.battleProperties.lastMoveId = move.id;

                    #endregion

                    #endregion

                    #region Stage Effects

                    #region Stage Modifiers

                    #region Trick Room

                    if (usageResults.setTrickRoomDuration > 0)
                    {

                        battleAnimationSequencer.EnqueueSingleText("Something started feeling backwards...");

                        battleData.stageModifiers.trickRoomRemainingTurns = usageResults.setTrickRoomDuration;

                    }

                    #endregion

                    #region Spikes

                    if (usageResults.clearUserStageSpikes)
                    {

                        if ((userIsPlayer && battleData.stageModifiers.playerSpikesStage > 0) || (!userIsPlayer && battleData.stageModifiers.opponentSpikesStage > 0))
                            battleAnimationSequencer.EnqueueSingleText("The spikes surrounding " + action.user.GetName() + "'s team disappeared");

                        if (userIsPlayer)
                            battleData.stageModifiers.playerSpikesStage = 0;
                        else
                            battleData.stageModifiers.opponentSpikesStage = 0;

                    }
                    else if (usageResults.increaseTargetStageSpikes)
                    {

                        if (userIsPlayer)
                        {

                            battleData.stageModifiers.opponentSpikesStage++;

                            if (battleData.stageModifiers.opponentSpikesStage > BattleData.StageModifiers.maxSpikesStage)
                                battleData.stageModifiers.opponentSpikesStage = BattleData.StageModifiers.maxSpikesStage;

                        }
                        else
                        {

                            battleData.stageModifiers.playerSpikesStage++;

                            if (battleData.stageModifiers.playerSpikesStage > BattleData.StageModifiers.maxSpikesStage)
                                battleData.stageModifiers.playerSpikesStage = BattleData.StageModifiers.maxSpikesStage;

                        }

                        battleAnimationSequencer.EnqueueSingleText("Spikes were lain around " + action.user.GetName() + "'s team");

                    }

                    #endregion

                    #region Toxic Spikes

                    if (usageResults.clearUserStageToxicSpikes)
                    {

                        if ((userIsPlayer && battleData.stageModifiers.playerToxicSpikesStage > 0) || (!userIsPlayer && battleData.stageModifiers.opponentToxicSpikesStage > 0))
                            battleAnimationSequencer.EnqueueSingleText("The toxic spikes surrounding " + action.user.GetName() + "'s team disappeared");

                        if (userIsPlayer)
                            battleData.stageModifiers.playerToxicSpikesStage = 0;
                        else
                            battleData.stageModifiers.opponentToxicSpikesStage = 0;

                    }
                    else if (usageResults.increaseTargetToxicStageSpikes)
                    {

                        if (userIsPlayer)
                        {

                            battleData.stageModifiers.opponentToxicSpikesStage++;

                            if (battleData.stageModifiers.opponentToxicSpikesStage > BattleData.StageModifiers.maxToxicSpikesStage)
                                battleData.stageModifiers.opponentToxicSpikesStage = BattleData.StageModifiers.maxToxicSpikesStage;

                        }
                        else
                        {

                            battleData.stageModifiers.playerToxicSpikesStage++;

                            if (battleData.stageModifiers.playerToxicSpikesStage > BattleData.StageModifiers.maxToxicSpikesStage)
                                battleData.stageModifiers.playerToxicSpikesStage = BattleData.StageModifiers.maxToxicSpikesStage;

                        }

                        battleAnimationSequencer.EnqueueSingleText("Toxic spikes were lain around " + action.user.GetName() + "'s team");

                    }

                    #endregion

                    #region Pointed Stones

                    if (usageResults.clearUserStagePointedStones)
                    {

                        if ((userIsPlayer && battleData.stageModifiers.playerPointedStonesEnabled) || (!userIsPlayer && battleData.stageModifiers.opponentPointedStonesEnabled))
                            battleAnimationSequencer.EnqueueSingleText("The pointed stones surrounding " + action.user.GetName() + "'s team disappeared");

                        if (userIsPlayer)
                            battleData.stageModifiers.playerPointedStonesEnabled = false;
                        else
                            battleData.stageModifiers.opponentPointedStonesEnabled = false;

                    }
                    else if (usageResults.setTargetStagePointedStones)
                    {

                        if (userIsPlayer)
                            battleData.stageModifiers.opponentPointedStonesEnabled = true;
                        else
                            battleData.stageModifiers.playerPointedStonesEnabled = true;

                        battleAnimationSequencer.EnqueueSingleText("Pointed stones hover around " + action.user.GetName() + "'s team");

                    }

                    #endregion

                    #endregion

                    #region Weather

                    if (usageResults.newWeatherId != null && usageResults.newWeatherId != battleData.currentWeatherId)
                    {

                        // Logic
                        SetChangedWeather((int)usageResults.newWeatherId);

                        // Animation
                        battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
                        {
                            type = BattleAnimationSequencer.Animation.Type.WeatherDisplay,
                            weatherDisplayTargetWeatherId = (int)usageResults.newWeatherId
                        });

                    }

                    #endregion

                    #endregion

                    //Prepare usage results (not allowing misses) for next hit
                    if (i < moveHitCount - 1)
                    {
                        usageResults = move.CalculateEffect(
                            userPokemon,
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

                //If move failed, cancel any charging
                userPokemon.battleProperties.volatileBattleStatus.chargingStage = 0;
                if (userPokemon.battleProperties.volatileBattleStatus.semiInvulnerable)
                    battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation
                    {
                        type = BattleAnimationSequencer.Animation.Type.PokemonSemiInvulnerableStop,
                        pokemonSemiInvulnerableParticipantIsPlayer = userIsPlayer
                    });
                userPokemon.battleProperties.volatileBattleStatus.semiInvulnerable = false;

                //If move failed, cancel thrashing
                userPokemon.battleProperties.volatileBattleStatus.thrashTurns = 0;

                //If move failed, reset consecutive protection moves
                userPokemon.battleProperties.ResetConsevutiveProtectionMoves();

                battleAnimationSequencer.EnqueueSingleText("It failed!");
                yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            }
            else if (usageResults.missed)
            {

                //If move missed, cancel any charging
                userPokemon.battleProperties.volatileBattleStatus.chargingStage = 0;
                if (userPokemon.battleProperties.volatileBattleStatus.semiInvulnerable)
                    battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation
                    {
                        type = BattleAnimationSequencer.Animation.Type.PokemonSemiInvulnerableStop,
                        pokemonSemiInvulnerableParticipantIsPlayer = userIsPlayer
                    });
                userPokemon.battleProperties.volatileBattleStatus.semiInvulnerable = false;

                //If move missed, cancel thrashing
                userPokemon.battleProperties.volatileBattleStatus.thrashTurns = 0;

                //If move missed, reset consecutive protection moves
                userPokemon.battleProperties.ResetConsevutiveProtectionMoves();

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

            if (action.user != battleData.participantPlayer)
                Debug.LogWarning("Non-player participant is fleeing");

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

            bool escapeSuccess = battleData.RandomRange(0, 256) <= escapeChance;

            if (escapeSuccess)
            {

                battleAnimationSequencer.EnqueueSingleText(battleData.participantPlayer.ActivePokemon.GetDisplayName() + " escaped successfully");

                battleData.playerFled = true;
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

            bool userIsPlayer = action.user == battleData.participantPlayer;

            action.user.ActivePokemon.badlyPoisonedCounter = 1;
            action.user.ActivePokemon.battleProperties.ResetVolatileProperties();

            action.user.activePokemonIndex = action.switchPokemonIndex;
            action.user.ActivePokemon.battleProperties.ResetVolatileProperties();

            PokemonInstance newPokemon = action.user.ActivePokemon;

            if (action.user == battleData.participantOpponent)
                TryAddSeenOpponentCurrentPokemon();

            battleAnimationSequencer.EnqueueSingleText(action.user.GetName() + " switched in " + newPokemon.GetDisplayName());

            battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
            {
                type = userIsPlayer
                ? BattleAnimationSequencer.Animation.Type.PlayerRetract
                : BattleAnimationSequencer.Animation.Type.OpponentRetract
            });

            battleAnimationSequencer.EnqueueAnimation(new BattleAnimationSequencer.Animation()
            {
                type = userIsPlayer
                ? BattleAnimationSequencer.Animation.Type.PlayerSendOut
                : BattleAnimationSequencer.Animation.Type.OpponentSendOutTrainer,
                sendOutPokemon = newPokemon,
                participantPokemonStates = action.user.GetPokemon().Select(x => BattleLayout.PokeBallLineController.GetPokemonInstanceBallState(x)).ToArray()
            });

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            #region Entry Hazards

            #region Spikes

            byte spikesLevel = battleData.GetParticipantSpikesLevel(userIsPlayer);

            if (spikesLevel > 0)
            {

                int targetInitialHealth = newPokemon.health;

                int damageDealt = newPokemon.TakeDamage(BattleData.StageModifiers.CalculateSpikesDamage(spikesLevel, newPokemon));
                newPokemon.battleProperties.AddDamageThisTurn(damageDealt);

                battleAnimationSequencer.EnqueueSingleText(newPokemon.GetDisplayName() + " was damaged by the spikes");
                battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(newPokemon, targetInitialHealth, userIsPlayer));

            }

            #endregion

            #region Toxic Spikes

            byte toxicSpikesLevel = battleData.GetParticipantToxicSpikesLevel(userIsPlayer);

            if (toxicSpikesLevel > 0 && newPokemon.nonVolatileStatusCondition == PokemonInstance.NonVolatileStatusCondition.None && !newPokemon.HasType(Pokemon.Type.Poison))
            {

                if (toxicSpikesLevel == 1)
                {

                    newPokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.Poisoned;

                    battleAnimationSequencer.EnqueueSingleText(newPokemon.GetDisplayName() + " was poisoned by the toxic spikes");

                }
                else
                {

                    newPokemon.nonVolatileStatusCondition = PokemonInstance.NonVolatileStatusCondition.BadlyPoisoned;

                    battleAnimationSequencer.EnqueueSingleText(newPokemon.GetDisplayName() + " was badly poisoned by the toxic spikes");

                }

                if (userIsPlayer)
                    battleLayoutController.overviewPaneManager.playerPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(newPokemon.nonVolatileStatusCondition);
                else
                    battleLayoutController.overviewPaneManager.opponentPokemonOverviewPaneController.UpdateNonVolatileStatsCondition(newPokemon.nonVolatileStatusCondition);

            }

            #endregion

            #region Pointed Stones

            bool pointedStonesEnabled = battleData.GetParticipantPointedStonesEnabled(userIsPlayer);

            if (pointedStonesEnabled)
            {

                int targetInitialHealth = newPokemon.health;

                int damageDealt = newPokemon.TakeDamage(BattleData.StageModifiers.CalculatePointedStonesDamage(newPokemon));
                newPokemon.battleProperties.AddDamageThisTurn(damageDealt);

                battleAnimationSequencer.EnqueueSingleText(newPokemon.GetDisplayName() + " was damaged by the pointed stones");
                battleAnimationSequencer.EnqueueAnimation(GenerateDamageAnimation(newPokemon, targetInitialHealth, userIsPlayer));

            }

            #endregion

            #endregion

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
                pokeBallUseWobbleCount = wobbleCount,
                speciesId = targetPokemon.speciesId
            });

            if (shakeResults.All(x => x))
            {
                battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " was caught!");
            }
            else
            {
                battleAnimationSequencer.EnqueueSingleText(targetPokemon.GetDisplayName() + " broke out!");
            }

            #endregion

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

            #region Successful Catch

            if (shakeResults.All(x => x)) //If the catch is successful
            {

                battleData.opponentWasCaptured = true;

                TryTriggerVictoryMusic();

                #region Experience and EV Distribution

                if (battleData.expEvGainEnabled)
                    yield return StartCoroutine(DistributeExperienceAndEVsForCurrentOpponentPokemon());

                #endregion

                #region Target Pokemon Catch Details Setting

                targetPokemon.pokeBallId = pokeBall.id;
                targetPokemon.originalTrainerName = PlayerData.singleton.profile.name;
                targetPokemon.originalTrainerGuid = PlayerData.singleton.profile.guid;
                targetPokemon.catchTime = PokemonInstance.GetCurrentEpochTime();

                #endregion

                #region Add Pokemon to Player Pokemon

                PlayerData.singleton.pokedex.AddPokemonCaught(targetPokemon);

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

            affectedPokemon.AddFriendship(itemUsageEffects.friendshipGained);

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

        /// <summary>
        /// Executes a recharging action where the participant's pokemon must recharge
        /// </summary>
        private IEnumerator ExecuteAction_Recharging(BattleParticipant.Action action)
        {

            battleAnimationSequencer.EnqueueSingleText(action.user.ActivePokemon.GetDisplayName() + " must recharge");

            yield return StartCoroutine(battleAnimationSequencer.PlayAll());

        }

        #endregion

    }

}
