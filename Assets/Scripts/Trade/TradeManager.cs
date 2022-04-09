using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Trade.TradeUI;
using Networking;
using Pokemon;
using Serialization;
using EvolutionScene;

namespace Trade
{
    public sealed class TradeManager : GeneralSceneManager
    {

        // This class is based on BattleManager as it should function similarly

        public static TradeManager GetTradeSceneTradeManager(Scene scene)
        {

            TradeManager[] managers = FindObjectsOfType<TradeManager>()
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

        #region Text Box Messages

        public const string connErrorOccuredMessage = "Connection error. Ending trade...";

        private const string offerPokemonConfirmPrompt = "Are you sure you want to offer this pokemon?";
        private static readonly string[] offerPokemonConfirmOptions = new string[]
        {
            "Cancel",
            "Offer"
        };
        public const string disallowedOfferPokemonMessageSuffix = " isn't allowed to receive this pokemon";

        public const string waitingForOtherUserOfferMessagePrefix = "Waiting for ";
        public const string waitingForOtherUserOfferMessageSuffix = " to offer a pokemon...";
        private static readonly string[] waitingForOtherUserOfferOptions = new string[]
        {
            "Cancel"
        };

        private const string offerDecisionPrompt = "Would you like to accept this trade?";
        private static readonly string[] offerDecisionOptions = new string[]
        {
            "Decline",
            "Accept"
        };

        public const string waitingForOtherUserOfferDecisionMessagePrefix = "Waiting for ";
        public const string waitingForOtherUserOfferDecisionMessageSuffix = " to accept the offer...";

        private const string offerDeclinedByOtherUserMessageSuffix = " declined the offer.";

        private const string closeTradeConfirmPrompt = "Are you sure you want to stop trading?";
        private static readonly string[] closeTradeConfirmOptions = new string[]
        {
            "Back",
            "Close Trade"
        };

        private const string tradeClosedByOtherUserMessageSuffix = " has stopped trading. Closing trade menu...";

        #endregion

        private enum TradeStage
        {
            ChoosingOffer,
            ConfirmingOfferChoice,
            WaitingForOtherUserOffer,
            DecidingOnOffer,
            WaitingForOtherUserDecisionOnOffer,
            ConfirmingCancelTrade
        }

        private PlayerData Player => PlayerData.singleton;

        public TradeUIController tradeUIController;

        public TradeAnimationCanvasController tradeAnimationController;

        private Coroutine mainTradeSceneCoroutine;
        private TradeStage tradeStage;

        private TextBoxController textBoxController;

        private Connection.NetworkTradeCommsManager commsManager;

        public NetworkStream networkStream { get; private set; }
        public string otherUserName { get; private set; }
        public Guid[] disallowedSendPokemonGuids { get; private set; }

        private PlayerData.PokemonLocator? locatorToBeConfirmed;

        private bool tryingToConfirmCloseTrade = false;

        private PlayerData.PokemonLocator? playerOfferedPokemonLocator;
        private PokemonInstance PlayerOfferedPokemon =>
            playerOfferedPokemonLocator == null
            ? null
            : ((PlayerData.PokemonLocator)playerOfferedPokemonLocator).Get(Player);
        private PokemonInstance otherUserOfferedPokemon;

        private bool waitingForPlayerChoice = false;

        private bool needToNotifyPlayerDisallowedTrade = false;

        /// <summary>
        /// Whether the other user has accepted the trade. If false, the other user may have not decided yet. If the other user declines, this shouldn't need to be checked
        /// </summary>
        private bool otherUserAcceptedTrade = false;

        private bool tradeReadyToExecute = false;

        private void Start()
        {

            textBoxController = TextBoxController.GetTextBoxController(Scene);

            SetUpScene();

        }

        private void OnDestroy()
        {

            StopCoroutine(mainTradeSceneCoroutine);
            CloseNetworking();

        }

        private void SetUpScene()
        {

            tradeUIController.SetUp(this);
            tradeAnimationController.Hide();
            textBoxController.Hide();

        }

        public void StartTradeScene()
        {

            mainTradeSceneCoroutine = StartCoroutine(MainTradeSceneCoroutine());

        }

        private IEnumerator MainTradeSceneCoroutine()
        {

            #region Initial Setup

            waitingForPlayerChoice = false;
            tradeReadyToExecute = false;

            //Check arguments are set
            if (!TradeEntranceArguments.argumentsSet)
            {
                Debug.LogError("Trade entrance arguments not set");
                GameSceneManager.CloseTradeScene();
                yield break;
            }

            //Set up trade stage
            tradeStage = 0;

            //Read entrance arguments
            networkStream = TradeEntranceArguments.networkStream;
            otherUserName = TradeEntranceArguments.otherUserName;
            disallowedSendPokemonGuids = TradeEntranceArguments.disallowedSendPokemonGuids;

            //Set the other user's displayed name according to newly-read-from-entrance-arguments name
            tradeUIController.RefreshOtherUserName();

            //Initialise offered pokemon
            playerOfferedPokemonLocator = null;
            otherUserOfferedPokemon = null;

            //Create comms manager
            commsManager = new Connection.NetworkTradeCommsManager(
                stream: networkStream,
                serializer: Serialize.DefaultSerializer);

            //Start listening
            commsManager.StartListening();

            #endregion

            #region Trade Decision Loop

            tradeUIController.SetInteractionEnabled(true);

            while (true)
            {

                #region Comm Receiving

                Connection.NetworkCommsManager.Comm comm = commsManager.GetNextComm();

                if (comm != null)
                {
                    yield return StartCoroutine(HandleTradeComm(comm));
                }

                #endregion

                #region Actions Needing To Be Taken

                if (locatorToBeConfirmed != null)
                {

                    if (tradeStage != TradeStage.ChoosingOffer && tradeStage != TradeStage.ConfirmingOfferChoice)
                    {
                        Debug.LogError("Locator to be confirmed has been set when not in correct trade stage");
                    }
                    else if (tradeStage == TradeStage.ChoosingOffer)
                    {
                        yield return StartCoroutine(SetTradeStage(TradeStage.ConfirmingOfferChoice));
                    }

                }
                else if (tryingToConfirmCloseTrade)
                {

                    if (tradeStage != TradeStage.ChoosingOffer)
                    {
                        Debug.LogError("Trying to close trade when not in correct trade stage");
                    }
                    else
                    {
                        tryingToConfirmCloseTrade = false;
                        yield return StartCoroutine(SetTradeStage(TradeStage.ConfirmingCancelTrade));
                    }

                }
                else if (needToNotifyPlayerDisallowedTrade)
                {

                    if (tradeStage != TradeStage.ChoosingOffer && tradeStage != TradeStage.ConfirmingOfferChoice)
                    {
                        Debug.LogError("Trying to notify player of disallowed offer when not choosing offer pokemon");
                    }
                    else
                    {

                        needToNotifyPlayerDisallowedTrade = false;

                        textBoxController.Show();
                        textBoxController.RevealText(otherUserName + disallowedOfferPokemonMessageSuffix);
                        yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());
                        textBoxController.Hide();

                    }

                }

                #endregion

                #region Other user making decision being waited for

                //Waiting for other user's offer and other user's offer has been set
                if (tradeStage == TradeStage.WaitingForOtherUserOffer && otherUserOfferedPokemon != null)
                    yield return StartCoroutine(SetTradeStage(TradeStage.DecidingOnOffer));

                //Waiting for other user decision on offer and other user has accepted trade
                if (tradeStage == TradeStage.WaitingForOtherUserDecisionOnOffer && otherUserAcceptedTrade)
                {
                    tradeReadyToExecute = true;
                    break; //Break loop to execute trade
                }

                #endregion

                #region User Making Choices during Trade Stages

                if (waitingForPlayerChoice)
                {
                    if (textBoxController.userChoiceIndexSelected >= 0) //If choice has been made
                        switch (tradeStage)
                        {

                            case TradeStage.ConfirmingOfferChoice:
                                yield return StartCoroutine(ConfirmOfferChoiceMade());
                                break;

                            case TradeStage.WaitingForOtherUserOffer:
                                yield return StartCoroutine(CancelOfferedPokemon());
                                break;

                            case TradeStage.DecidingOnOffer:
                                yield return StartCoroutine(DecisionOnOfferChoiceMade());
                                break;

                            case TradeStage.ConfirmingCancelTrade:
                                yield return StartCoroutine(ConfirmCancelTradeChoiceMade());
                                break;

                            default:
                                Debug.LogError("Text box waiting for user choice when not on selection trade stage");
                                break;

                        }
                }

                #endregion

                if (commsManager.CommsConnErrorOccured)
                    break;

                yield return new WaitForFixedUpdate();

            }

            #endregion

            #region Dealing with Circumstances of Loop Breaking

            if (tradeReadyToExecute) //Trade decided successfully
            {

                CloseNetworking();

                tradeUIController.Hide();

                yield return StartCoroutine(ExecuteTrade());

                CloseTradeScene();

            }
            else if (commsManager.CommsConnErrorOccured) //Connection error occured
            {

                textBoxController.RevealText(connErrorOccuredMessage);
                yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

                CloseTradeScene();

            }
            else //Unknown reason
            {

                Debug.LogError("Main trade scene loop closed for unknown reason");
                GameSceneManager.CloseTradeScene();

            }

            #endregion

        }

        #region Execute Trade

        private IEnumerator ExecuteTrade()
        {

            //Animation should be run first as player offered pokemon locator will point to incorrect pokemon otherwise

            tradeAnimationController.Show();
            yield return StartCoroutine(tradeAnimationController.RunAnimation(PlayerOfferedPokemon, otherUserOfferedPokemon));
            tradeAnimationController.Hide();

            //Before this point, the pokemon at the locator's position is still the old pokemon

            ExecuteTrade_ChangePokemon();

            //After this point, the pokemon at the locator's position has been changed to the new pokemon

            PokemonInstance recvPmon = ((PlayerData.PokemonLocator)playerOfferedPokemonLocator).Get(Player);

            #region Trade Evolution

            if (recvPmon.heldItem == null || recvPmon.heldItem.id != 229) //Only consider evolving if not holding everstone
            {

                PokemonSpecies.Evolution evol = recvPmon.TryFindEvolution(trading: true);

                //If valid evolution found, perform it
                if (evol != null)
                {

                    EvolutionSceneController.entranceArguments = new EvolutionSceneController.EntranceArguments()
                    {
                        evolution = evol,
                        pokemon = recvPmon
                    };

                    DisableScene();

                    GameSceneManager.LaunchEvolutionScene();

                    bool continueAfterEvolution = false;

                    GameSceneManager.EvolutionSceneClosed += () => continueAfterEvolution = true;

                    yield return new WaitUntil(() => continueAfterEvolution);

                    EnableScene();

                }

            }

            #endregion


        }

        private void ExecuteTrade_ChangePokemon()
        {

            if (playerOfferedPokemonLocator == null)
            {
                Debug.LogError("Trying to execute trade when player offered pokemon locator not set");
                return;
            }

            if (otherUserOfferedPokemon == null)
            {
                Debug.LogError("Trying to execute trade when other user offered pokemon not set");
                return;
            }

            Player.AddTradeReceivedPokemon(otherUserOfferedPokemon);
            Player.TryRemoveTradeReceivedPokemon(((PlayerData.PokemonLocator)playerOfferedPokemonLocator).Get(Player));

            ((PlayerData.PokemonLocator)playerOfferedPokemonLocator).Set(Player, otherUserOfferedPokemon);

            ((PlayerData.PokemonLocator)playerOfferedPokemonLocator).Get(Player).RestoreFully(); //Restore received pokemon

        }

        #endregion

        private void CloseNetworking()
        {
            commsManager?.StopListening();
            networkStream?.Close();
        }

        private void StopSceneLogic()
        {

            CloseNetworking();

            if (mainTradeSceneCoroutine != null)
                StopCoroutine(mainTradeSceneCoroutine);

        }

        private void CloseTradeScene()
        {

            StopSceneLogic();

            GameSceneManager.CloseTradeScene();

        }

        public void TryConfirmOfferPokemon(PlayerData.PokemonLocator loc)
        {

            if (playerOfferedPokemonLocator != null)
            {
                Debug.LogError("Trying to set new offered pokemon when a pokemon already offered");
                return;
            }

            if (disallowedSendPokemonGuids.Contains(loc.Get(Player).guid)) //If other user isn't allowed to receive this pokemon, don't allow offering it
            {
                needToNotifyPlayerDisallowedTrade = true;
            }
            else
            {
                locatorToBeConfirmed = loc; //Main trade scene coroutine will deal with changing trade stage and prompting for confirmation
            }

        }

        /// <summary>
        /// Closes the trade scene softly and notifies the other user of this choice
        /// </summary>
        public void TryCloseTrading()
        {

            if (tradeStage != TradeStage.ChoosingOffer)
            {
                Debug.LogError("Trying to close trade when not in correct trade stage");
                return;
            }

            tryingToConfirmCloseTrade = true;

        }

        private IEnumerator OfferPokemon(PlayerData.PokemonLocator loc)
        {

            if (playerOfferedPokemonLocator != null)
            {
                Debug.LogError("Trying to set new offered pokemon when a pokemon already offered");
                yield break;
            }

            if (tradeStage != TradeStage.ConfirmingOfferChoice)
            {
                Debug.LogError("Trying to set offered pokemon when trade stage isn't choosing offer stage");
                yield break;
            }

            playerOfferedPokemonLocator = loc;

            if (!commsManager.TrySendOfferPokemon(PlayerOfferedPokemon))
            {
                Debug.LogError("Failed to send offered pokemon comm");
                yield break;
            }

            tradeUIController.SetInteractionEnabled(false);

            if (otherUserOfferedPokemon == null)
            {
                yield return StartCoroutine(SetTradeStage(TradeStage.WaitingForOtherUserOffer));
            }
            else
            {
                yield return StartCoroutine(SetTradeStage(TradeStage.DecidingOnOffer));
            }

        }

        private IEnumerator CancelOfferedPokemon()
        {

            waitingForPlayerChoice = false;

            if (tradeStage != TradeStage.WaitingForOtherUserOffer)
            {
                Debug.LogError("Trying to cancel offered pokemon when not waiting for other user offer");
                yield break;
            }

            if (textBoxController.userChoiceIndexSelected != 0) //Only one available choice
            {
                Debug.LogError("Unknown choice selected for cancelling offered pokemon");
                yield break;
            }

            if (!commsManager.TrySendCancelOfferPokemon())
            {
                Debug.LogError("Failed to send cancel offer comm");
                yield break;
            }

            yield return StartCoroutine(ClearPlayerOfferedPokemon());

            yield return StartCoroutine(SetTradeStage(TradeStage.ChoosingOffer));

        }

        private IEnumerator DeclineTradeOffer()
        {

            if (!commsManager.TrySendDeclineTrade())
            {
                Debug.LogError("Failed to send decline trade comm");
                yield break;
            }

            yield return StartCoroutine(ClearPlayerOfferedPokemon());
            yield return StartCoroutine(ClearOtherUserOfferedPokemon());
            
            yield return StartCoroutine(SetTradeStage(TradeStage.ChoosingOffer));

        }

        private IEnumerator AcceptTradeOffer()
        {

            if (!commsManager.TrySendAcceptTrade())
            {
                Debug.LogError("Failed to send accept trade comm");
                yield break;
            }

            yield return StartCoroutine(SetTradeStage(TradeStage.WaitingForOtherUserDecisionOnOffer));

        }

        private IEnumerator CloseTrade()
        {

            if (!commsManager.TrySendCloseTrade())
            {
                Debug.LogError("Failed to send close trade comm");
            }

            CloseTradeScene();

            yield break;

        }

        private IEnumerator ConfirmOfferChoiceMade()
        {

            if (locatorToBeConfirmed == null)
            {
                Debug.LogError("Locator to be confirmed is null when trying to confirm it");
                yield break;
            }

            waitingForPlayerChoice = false;

            switch (textBoxController.userChoiceIndexSelected)
            {

                case 0: //Cancel
                    locatorToBeConfirmed = null;
                    yield return StartCoroutine(SetTradeStage(TradeStage.ChoosingOffer));
                    break;

                case 1: //Confirm
                    yield return StartCoroutine(OfferPokemon((PlayerData.PokemonLocator)locatorToBeConfirmed));
                    locatorToBeConfirmed = null;
                    break;

                default:
                    Debug.LogError("Unknown used choice for confirming offered pokemon");
                    break;

            }

        }

        private IEnumerator DecisionOnOfferChoiceMade()
        {

            waitingForPlayerChoice = false;

            switch (textBoxController.userChoiceIndexSelected)
            {

                case 0: //Decline:
                    yield return StartCoroutine(DeclineTradeOffer());
                    break;

                case 1: //Accept
                    yield return StartCoroutine(AcceptTradeOffer());
                    break;

                default:
                    Debug.LogError("Unknown used choice for deciding on offer");
                    break;

            }

        }

        private IEnumerator ConfirmCancelTradeChoiceMade()
        {

            waitingForPlayerChoice = false;

            switch (textBoxController.userChoiceIndexSelected)
            {

                case 0: //Back
                    yield return StartCoroutine(SetTradeStage(TradeStage.ChoosingOffer));
                    break;

                case 1: //Close trade
                    yield return StartCoroutine(CloseTrade());
                    break;

                default:
                    Debug.LogError("Unknown used choice for confirming trade choice made");
                    break;

            }
            yield break;

        }

        /// <summary>
        /// Clears the pokemon offered by the player so they can select a new one and sets the trade stage back to choosing an offered pokemon.
        /// </summary>
        private IEnumerator ClearPlayerOfferedPokemon()
        {

            playerOfferedPokemonLocator = null;

            yield break;

        }

        /// <summary>
        /// Clears the pokemon offered by the other user, removing it from the UI and unsetting its variable.
        /// </summary>
        private IEnumerator ClearOtherUserOfferedPokemon()
        {

            otherUserOfferedPokemon = null;

            tradeUIController.SetOtherUserOfferedPokemon(null);

            yield break;

        }

        #region Handling Trade Comms

        private IEnumerator HandleTradeComm(Connection.NetworkCommsManager.Comm comm)
        {

            if (comm is Connection.NetworkCommsManager.TradeOfferPokemonComm offerComm)
                yield return StartCoroutine(HandleTradeComm_Offer(offerComm));
            else if (comm is Connection.NetworkCommsManager.TradeCancelOfferPokemonComm)
                yield return StartCoroutine(HandleTradeComm_CancelOffer());
            else if (comm is Connection.NetworkCommsManager.TradeAcceptTradeComm)
                yield return StartCoroutine(HandleTradeComm_AcceptOffer());
            else if (comm is Connection.NetworkCommsManager.TradeDeclineTradeComm)
                yield return StartCoroutine(HandleTradeComm_DeclineOffer());
            else if (comm is Connection.NetworkCommsManager.TradeCloseTradeComm)
                yield return StartCoroutine(HandleTradeComm_CancelTrade());
            else
                Debug.LogError("Unknown trade comm type");

        }

        private IEnumerator HandleTradeComm_Offer(Connection.NetworkCommsManager.TradeOfferPokemonComm offerComm)
        {

            if (otherUserOfferedPokemon != null)
            {
                Debug.LogError("Trying to set new other user offered pokemon when one already offered");
                yield break;
            }

            otherUserOfferedPokemon = offerComm.pokemon;

            tradeUIController.SetOtherUserOfferedPokemon(otherUserOfferedPokemon);

        }

        private IEnumerator HandleTradeComm_CancelOffer()
        {

            if (otherUserOfferedPokemon == null)
            {
                Debug.LogError("Trying to cancel other user offered pokemon when none has been offered yet");
                yield break;
            }

            yield return StartCoroutine(ClearOtherUserOfferedPokemon());

            //If player has offered a pokemon, clear it
            if (playerOfferedPokemonLocator != null)
                yield return StartCoroutine(ClearPlayerOfferedPokemon());

        }

        private IEnumerator HandleTradeComm_AcceptOffer()
        {

            if (playerOfferedPokemonLocator == null || otherUserOfferedPokemon == null)
            {
                Debug.LogError("Trying to have other user accept offer when an offered pokemon isn't set");
                yield break;
            }

            otherUserAcceptedTrade = true;

        }

        private IEnumerator HandleTradeComm_DeclineOffer()
        {

            if (playerOfferedPokemonLocator == null || otherUserOfferedPokemon == null)
            {
                Debug.LogError("Trying to have other user decline offer when an offered pokemon isn't set");
                yield break;
            }

            waitingForPlayerChoice = false;

            yield return StartCoroutine(ClearOtherUserOfferedPokemon());

            yield return StartCoroutine(ClearPlayerOfferedPokemon());

            textBoxController.StopGettingUserChoice();
            textBoxController.RevealText(otherUserName + offerDeclinedByOtherUserMessageSuffix);
            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            yield return StartCoroutine(SetTradeStage(TradeStage.ChoosingOffer));

        }

        private IEnumerator HandleTradeComm_CancelTrade()
        {

            waitingForPlayerChoice = false;

            yield return StartCoroutine(ClearOtherUserOfferedPokemon());

            yield return StartCoroutine(ClearPlayerOfferedPokemon());

            textBoxController.RevealText(otherUserName + tradeClosedByOtherUserMessageSuffix);

            CloseNetworking();

            yield return StartCoroutine(textBoxController.PromptAndWaitUntilUserContinue());

            CloseTradeScene();

        }

        #endregion

        #region Trade Stage Progression

        private IEnumerator SetTradeStage(TradeStage newStage)
        {

            tradeStage = newStage;

            switch (tradeStage)
            {

                case TradeStage.ChoosingOffer:
                    yield return StartCoroutine(SetUIForChoosingOffer());
                    break;

                case TradeStage.ConfirmingOfferChoice:
                    yield return StartCoroutine(SetUIForConfirmingOfferChoice());
                    break;

                case TradeStage.WaitingForOtherUserOffer:
                    yield return StartCoroutine(SetUIForWaitingForOtherUserOffer());
                    break;

                case TradeStage.DecidingOnOffer:
                    yield return StartCoroutine(SetUIForDecidingOnOffer());
                    break;

                case TradeStage.WaitingForOtherUserDecisionOnOffer:
                    yield return StartCoroutine(SetUIForWaitingForOtherUserDecisionOnOffer());
                    break;

                case TradeStage.ConfirmingCancelTrade:
                    yield return StartCoroutine(SetUIForConfirmingCloseTrade());
                    break;

                default:
                    Debug.LogError("Unknown trade stage: " + tradeStage);
                    break;

            }

        }

        private IEnumerator SetUIForChoosingOffer()
        {

            if (playerOfferedPokemonLocator != null)
            {
                Debug.LogError("Trying to set up for player choosing offered pokemon when offered pokemon already selected");
                yield break;
            }

            tradeUIController.SetInteractionEnabled(true);
            textBoxController.Hide();

            tradeUIController.TrySetDefaultSelectedGameObject();

        }

        private IEnumerator SetUIForConfirmingOfferChoice()
        {

            if (playerOfferedPokemonLocator != null)
            {
                Debug.LogError("Trying to set up for player choosing offered pokemon when offered pokemon already selected");
                yield break;
            }

            tradeUIController.SetInteractionEnabled(false, true);
            textBoxController.Show();

            textBoxController.SetTextInstant(offerPokemonConfirmPrompt);
            StartCoroutine(textBoxController.GetUserChoice(offerPokemonConfirmOptions));

            waitingForPlayerChoice = true;

        }

        private IEnumerator SetUIForWaitingForOtherUserOffer()
        {

            if (playerOfferedPokemonLocator == null)
            {
                Debug.LogError("Trying to set up for waiting for other user offer when player hasn't selected their offered pokemon");
                yield break;
            }

            if (otherUserOfferedPokemon != null)
                yield return StartCoroutine(SetUIForDecidingOnOffer());

            tradeUIController.SetInteractionEnabled(false);
            textBoxController.Show();

            textBoxController.RevealText(waitingForOtherUserOfferMessagePrefix + otherUserName + waitingForOtherUserOfferMessageSuffix);
            StartCoroutine(textBoxController.GetUserChoice(waitingForOtherUserOfferOptions));

            waitingForPlayerChoice = true;

        }

        private IEnumerator SetUIForDecidingOnOffer()
        {

            if (otherUserOfferedPokemon == null || playerOfferedPokemonLocator == null)
            {
                Debug.LogError("Trying to set up for deciding on offer when an offered pokemon isn't set");
                yield break;
            }

            tradeUIController.SetInteractionEnabled(false, true);
            textBoxController.Show();

            textBoxController.SetTextInstant(offerDecisionPrompt);
            StartCoroutine(textBoxController.GetUserChoice(offerDecisionOptions));

            waitingForPlayerChoice = true;

        }

        private IEnumerator SetUIForWaitingForOtherUserDecisionOnOffer()
        {

            if (otherUserOfferedPokemon == null || playerOfferedPokemonLocator == null)
            {
                Debug.LogError("Trying to set up for waiting for other user deciding on offer when an offered pokemon isn't set");
                yield break;
            }

            tradeUIController.SetInteractionEnabled(false);
            textBoxController.Show();
            textBoxController.RevealText(waitingForOtherUserOfferDecisionMessagePrefix + otherUserName + waitingForOtherUserOfferDecisionMessageSuffix);

        }

        private IEnumerator SetUIForConfirmingCloseTrade()
        {

            if (playerOfferedPokemonLocator != null)
            {
                Debug.LogError("Trying to confirm closing trade when player is offering a pokemon");
                yield break;
            }

            tradeUIController.SetInteractionEnabled(false, true);
            textBoxController.Show();

            textBoxController.SetTextInstant(closeTradeConfirmPrompt);
            StartCoroutine(textBoxController.GetUserChoice(closeTradeConfirmOptions));

            waitingForPlayerChoice = true;

        }

        #endregion

    }
}