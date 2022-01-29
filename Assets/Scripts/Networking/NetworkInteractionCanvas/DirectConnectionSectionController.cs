using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace Networking.NetworkInteractionCanvas
{
    public class DirectConnectionSectionController : ConnectionSectionController
    {

        protected bool interactive;

        public InputField addressInput;
        public InputField portInput;

        public Button goButton;
        public Button closeButton;

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

            if (goButton.GetComponent<MenuSelectableController>() == null)
            {
                Debug.LogError("No MenuSelectableController in go button");
            }

            if (closeButton.GetComponent<MenuSelectableController>() == null)
            {
                Debug.LogError("No MenuSelectableController in close button");
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



        }

        #region Interactivity

        protected override void SetInteractable(bool state)
        {
            addressInput.interactable = state;
            portInput.interactable = state;
            goButton.interactable = state;
            closeButton.interactable = state;
            interactive = state;
        }

        protected override void SetInteractivityForServer()
        {
            addressInput.interactable = false;
            portInput.interactable = true;
            goButton.interactable = true;
            closeButton.interactable = true;
            interactive = true;
        }

        protected override void SetInteractivityForClient()
        {
            addressInput.interactable = true;
            portInput.interactable = true;
            goButton.interactable = true;
            closeButton.interactable = true;
            interactive = true;
        }

        #endregion

        private void GoButtonListener()
        {

            string addressString = addressInput.text;

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
                    SetInteractable(false);
                    Run_Server(port);
                    break;

                case ConnectionMode.Client:
                    SetInteractable(false);
                    Run_Client(addressString, port);
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
                StartCoroutine(ProcessConnection_Server(serverConnectionToProcess));
                SetServerConnectionToProcess(null);
            }

        }

        private static readonly object serverConnectionToProcessLock = new object();
        private Socket serverConnectionToProcess = null;

        private void SetServerConnectionToProcess(Socket socket)
        {
            lock (serverConnectionToProcessLock)
            {
                serverConnectionToProcess = socket;
            }
        }

        protected void Run_Server(int port = -1)
        {

            StartCoroutine(Server_Coroutine(port));

        }

        protected IEnumerator Server_Coroutine(int port = -1)
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

            SetServerConnectionToProcess(sock);

        }

        protected void Run_Client(string addressString,
            int port = -1)
        {

            if (!Connection.TryConnectToServer(
                out Socket socket,
                errCallback: canvasController.SetStatusMessageError,
                statusCallback: canvasController.SetStatusMessage,
                addressString,
                port))
            {

                socket?.Close();
                SetInteractable(true);
                return;

            }
            else
            {
                StartCoroutine(ProcessConnection_Client(socket));
            }

        }

        #endregion

    }
}