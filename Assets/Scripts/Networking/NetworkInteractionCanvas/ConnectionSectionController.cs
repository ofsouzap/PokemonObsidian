using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using FreeRoaming.AreaControllers;
using Menus;
using Pokemon;
using Battle;

namespace Networking.NetworkInteractionCanvas
{
    public abstract class ConnectionSectionController : MenuController
    {

        public GameObject defaultSelectedGameObject;

        protected NetworkInteractionCanvasController canvasController;

        protected FreeRoamSceneController sceneController;

        protected ConnectionMode connectionMode;

        public virtual void SetUp(NetworkInteractionCanvasController canvasController)
        {

            sceneController = FreeRoamSceneController.GetFreeRoamSceneController(gameObject.scene);

            this.canvasController = canvasController;

            Hide();

        }

        protected virtual void Start() { }

        protected virtual void Update() { }

        public virtual void Launch(ConnectionMode mode)
        {

            if (defaultSelectedGameObject != null)
                EventSystem.current.SetSelectedGameObject(defaultSelectedGameObject);

            sceneController.SetSceneRunningState(false);
            Show();

        }

        protected virtual void CloseMenu()
        {

            sceneController.SetSceneRunningState(true);
            Hide();

        }

        protected abstract void SetInteractable(bool state);
        protected abstract void SetInteractivityForServer();
        protected abstract void SetInteractivityForClient();

        #region Processing Established Connection

        protected IEnumerator ProcessConnection_Server(Socket socket)
        {

            NetworkStream stream = Connection.CreateNetworkStream(socket);

            //Verify connection
            if (!Connection.VerifyConnection_Server(stream))
            {
                canvasController.SetStatusMessageError("Failed to verify connection as server");
                stream.Close();
                Launch(connectionMode); //Reset section
                yield break;
            }

            //Get battle entrance arguments
            if (!Connection.TryExchangeBattleEntranceArguments_Server(stream,
                out string opponentName,
                out PokemonInstance[] opponentPokemon,
                out string opponentSpriteResourceName))
            {
                canvasController.SetStatusMessageError("Failed to exchange battle data");
                stream.Close();
                Launch(connectionMode); //Reset section
                yield break;
            }

            //Set battle entrance arguments
            BattleEntranceArguments.SetBattleEntranceArgumentsForNetworkBattle(stream, opponentName, opponentPokemon, opponentSpriteResourceName);

            //Launch battle
            CloseMenu();
            GameSceneManager.LaunchBattleScene();

        }

        protected IEnumerator ProcessConnection_Client(Socket socket)
        {

            NetworkStream stream = Connection.CreateNetworkStream(socket);

            //Verify connection
            if (!Connection.VerifyConnection_Client(stream))
            {
                canvasController.SetStatusMessageError("Failed to verify connection as client");
                stream.Close();
                Launch(connectionMode); //Reset section
                yield break;
            }

            //Get battle entrance arguments
            if (!Connection.TryExchangeBattleEntranceArguments_Client(stream,
                out string opponentName,
                out PokemonInstance[] opponentPokemon,
                out string opponentSpriteResourceName))
            {
                canvasController.SetStatusMessageError("Failed to exchange battle data");
                stream.Close();
                Launch(connectionMode); //Reset section
                yield break;
            }

            //Set battle entrance arguments
            BattleEntranceArguments.SetBattleEntranceArgumentsForNetworkBattle(stream, opponentName, opponentPokemon, opponentSpriteResourceName);

            //Launch battle
            CloseMenu();
            GameSceneManager.LaunchBattleScene();

        }

        #endregion

    }
}