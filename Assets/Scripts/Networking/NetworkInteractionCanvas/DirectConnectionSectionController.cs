using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace Networking.NetworkInteractionCanvas
{
    public class DirectConnectionSectionController : ConnectionSectionController
    {

        protected bool interactive;

        [Header("Connection Settings")]

        public Dropdown purposeDropdown;
        public InputField addressInput;
        public InputField portInput;

        public Button goButton;
        public Button closeButton;

        [Header("Battle Settings")]

        public Dropdown standarisedLevelDropdown;

        #region Dropdown Options

        protected static readonly string[] purposeDropdownOptions = new string[]
        {
            "Battle",
            "Trade"
        };

        protected static readonly Dictionary<string, Connection.ConnectionPurpose> purposeDropdownOptionValues = new Dictionary<string, Connection.ConnectionPurpose>()
        {
            { "Battle", Connection.ConnectionPurpose.Battle },
            { "Trade", Connection.ConnectionPurpose.Trade }
        };

        protected static void CheckPurposeDropdownOptionCorrelations()
        {

            foreach (string s in purposeDropdownOptions)
                if (!purposeDropdownOptionValues.ContainsKey(s))
                    Debug.LogError("No purpose dropdown option value for \"" + s + '"');

        }

        protected static readonly string[] standarisedLevelDropdownOptions = new string[]
        {
            "No Change",
            "Level 20",
            "Level 60"
        };

        protected static readonly Dictionary<string, byte?> standardisedLevelDropdownOptionValues = new Dictionary<string, byte?>()
        {
            { "No Change", null },
            { "Level 20", 20 },
            { "Level 60", 60 }
        };

        protected static void CheckStandardisedLevelDropdownOptionCorrelations()
        {

            foreach (string s in purposeDropdownOptions)
                if (!purposeDropdownOptionValues.ContainsKey(s))
                    Debug.LogError("No standardised level dropdown option value for \"" + s + '"');

        }

        #endregion

        protected override MenuSelectableController[] GetSelectables()
        {

            List<MenuSelectableController> selectables = new List<MenuSelectableController>();
            selectables.Add(addressInput.GetComponent<MenuSelectableController>());
            selectables.Add(portInput.GetComponent<MenuSelectableController>());
            selectables.Add(goButton.GetComponent<MenuSelectableController>());
            selectables.Add(closeButton.GetComponent<MenuSelectableController>());
            return selectables.ToArray();

        }

        public override void SetUp(NetworkInteractionCanvasController canvasController)
        {

            base.SetUp(canvasController);

            CheckPurposeDropdownOptionCorrelations();
            CheckStandardisedLevelDropdownOptionCorrelations();

            if (goButton.GetComponent<MenuSelectableController>() == null)
            {
                Debug.LogError("No MenuSelectableController in go button");
            }

            if (closeButton.GetComponent<MenuSelectableController>() == null)
            {
                Debug.LogError("No MenuSelectableController in close button");
            }

            if (purposeDropdown.GetComponent<MenuSelectableController>() == null)
            {
                Debug.LogError("No MenuSelectableController in purpose dropdown");
            }

            if (addressInput.GetComponent<MenuSelectableController>() == null)
            {
                Debug.LogError("No MenuSelectableController in address input field");
            }

            if (portInput.GetComponent<MenuSelectableController>() == null)
            {
                Debug.LogError("No MenuSelectableController in port input field");
            }

            goButton.onClick.RemoveAllListeners();
            goButton.onClick.AddListener(() => GoButtonListener());

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => CloseMenu());

            #region Dropdown Set-Up

            // Adding dropdown options done by making new Dropdown.OptionData for each string value in the array

            purposeDropdown.ClearOptions();
            purposeDropdown.AddOptions(new List<Dropdown.OptionData>(purposeDropdownOptions.Select(s => new Dropdown.OptionData(s))));

            standarisedLevelDropdown.ClearOptions();
            standarisedLevelDropdown.AddOptions(new List<Dropdown.OptionData>(standarisedLevelDropdownOptions.Select(s => new Dropdown.OptionData(s))));

            #endregion

        }

        protected override void Update()
        {

            base.Update();

            UpdateProcessServerConnection();

        }

        public override void Launch(ConnectionMode mode)
        {

            base.Launch(mode);

            switch (mode)
            {

                case ConnectionMode.Server:
                    addressInput.text = Connection.GetHostIPAddress().ToString();
                    SetInteractivityForServer();
                    break;

                case ConnectionMode.Client:
                    SetInteractivityForClient();
                    break;

                default:
                    Debug.LogError("Unknown connection mode - " + mode);
                    return;

            }

            connectionMode = mode;

        }

        protected override void CloseMenu()
        {

            base.CloseMenu();

            Connection.TryStopAsyncServerListening();

        }

        #region Interactivity

        protected override void SetInteractable(bool state)
        {
            purposeDropdown.interactable = state;
            addressInput.interactable = state;
            portInput.interactable = state;
            goButton.interactable = state;
            closeButton.interactable = state;
            standarisedLevelDropdown.interactable = state;
            interactive = state;
        }

        protected override void SetInteractivityForServer()
        {
            purposeDropdown.interactable = true;
            addressInput.interactable = false;
            portInput.interactable = true;
            goButton.interactable = true;
            closeButton.interactable = true;
            standarisedLevelDropdown.interactable = true;
            interactive = true;
        }

        protected override void SetInteractivityForClient()
        {
            purposeDropdown.interactable = true;
            addressInput.interactable = true;
            portInput.interactable = true;
            goButton.interactable = true;
            closeButton.interactable = true;
            standarisedLevelDropdown.interactable = false;
            interactive = true;
        }

        protected override void SetInteractivityForServerListening()
        {
            purposeDropdown.interactable = false;
            addressInput.interactable = false;
            portInput.interactable = false;
            goButton.interactable = false;
            closeButton.interactable = true;
            standarisedLevelDropdown.interactable = false;
            interactive = false;
        }

        #endregion

        private void GoButtonListener()
        {

            string addressString = addressInput.text;

            string purposeString = purposeDropdownOptions[purposeDropdown.value];
            Connection.ConnectionPurpose purpose = purposeDropdownOptionValues[purposeString];

            string standardisedLevelString = standarisedLevelDropdownOptions[standarisedLevelDropdown.value];
            byte? standardisedLevel = standardisedLevelDropdownOptionValues[standardisedLevelString];

            int port;

            if (portInput.text == "")
                port = -1;
            else
            {
                if (!int.TryParse(portInput.text, out port))
                {
                    canvasController.SetStatusMessageError("Invalid port provided");
                    return;
                }
            }

            switch (connectionMode)
            {

                case ConnectionMode.Server:
                    SetInteractivityForServerListening();
                    Run_Server(purpose, standardisedLevel, port);
                    break;

                case ConnectionMode.Client:
                    SetInteractable(false);
                    Run_Client(purpose, addressString, port);
                    break;

                default:
                    Debug.LogError("Unknown connection mode - " + connectionMode);
                    return;

            }

        }

        #region Running Modes

        private void UpdateProcessServerConnection()
        {

            if (serverConnectionToProcess != null)
            {

                if (connectionPurpose != null)
                {
                    StartCoroutine(ProcessConnection_Server(serverConnectionToProcess,
                        (Connection.ConnectionPurpose)connectionPurpose,
                        serverConnectionToProcessStandarisedLevel));
                    connectionPurpose = null;
                }
                else
                    Debug.LogError("Connection purpose not set when trying to process server connection");

                SetServerConnectionToProcess(null, null);
                
            }

        }

        private Connection.ConnectionPurpose? connectionPurpose;

        private static readonly object serverConnectionToProcessLock = new object();
        private Socket serverConnectionToProcess = null;
        private byte? serverConnectionToProcessStandarisedLevel;

        private void SetServerConnectionToProcess(Socket socket,
            byte? standarisedLevel)
        {
            lock (serverConnectionToProcessLock)
            {
                serverConnectionToProcess = socket;
                serverConnectionToProcessStandarisedLevel = standarisedLevel;
            }
        }

        protected void Run_Server(Connection.ConnectionPurpose purpose,
            byte? standardisedLevel,
            int port = -1)
        {

            StartCoroutine(Server_Coroutine(purpose, standardisedLevel, port));

        }

        protected IEnumerator Server_Coroutine(Connection.ConnectionPurpose purpose,
            byte? standardisedLevel,
            int port = -1)
        {

            if (serverConnectionToProcess != null)
            {
                serverConnectionToProcess?.Close();
                Debug.LogWarning("Connection waiting to be processed");
                canvasController.SetStatusMessageError("Server connection trying to be opened whilst one already active");
                SetInteractable(true);
            }

            canvasController.SetStatusMessage("Setting up server...");

            Connection.TryStartHostServer(

                startedListeningListener: (IPEndPoint endPoint) => //Started listening
                {
                    canvasController.SetStatusMessage($"Listening for connection at {endPoint.Address}:{endPoint.Port}...");
                },

                clientConnectedListener: null, //Async server doesn't use clientConnectedListener

                errCallback: canvasController.SetStatusMessageError,
                statusCallback: canvasController.SetStatusMessage,

                port: port);

            Socket sock;

            while (true)
            {

                sock = Connection.FetchAsyncNewClient();

                if (sock != null)
                    break;

                yield return new WaitForFixedUpdate();

            }

            connectionPurpose = purpose;
            SetServerConnectionToProcess(sock, standardisedLevel);

        }

        protected void Run_Client(Connection.ConnectionPurpose purpose,
            string addressString,
            int port = -1)
        {

            if (!Connection.TryConnectToServer(
                out Socket socket,
                errCallback: canvasController.SetStatusMessageError,
                statusCallback: canvasController.SetStatusMessage,
                ipAddress: addressString,
                port: port))
            {

                socket?.Close();
                SetInteractable(true);
                return;

            }
            else
            {
                StartCoroutine(ProcessConnection_Client(socket, purpose));
            }

        }

        #endregion

    }
}