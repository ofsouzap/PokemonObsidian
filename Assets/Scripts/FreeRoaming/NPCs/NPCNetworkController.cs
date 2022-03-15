using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using Networking;
using Networking.NetworkInteractionCanvas;

namespace FreeRoaming.NPCs
{
    public class NPCNetworkController : NPCController
    {

        public NetworkInteractionCanvasController networkInteractionCanvasController;

        public static readonly string[] connectionModeMenuOptions = new string[]
        {
            "Goodbye",
            "Direct (server)",
            "Direct (client)"
        };

        public override void Interact(GameCharacterController interacter)
        {

            if (interacter is PlayerController)
            {
                TryTurn(GetOppositeDirection(interacter.directionFacing));
                StartCoroutine(PlayerInteraction());
            }

        }

        protected IEnumerator PlayerInteraction()
        {

            bool thisPausedScene = sceneController.SceneIsRunning;
            bool textBoxControllerWasHidden = !textBoxController.IsShown;

            if (thisPausedScene)
                sceneController.SetSceneRunningState(false);

            if (textBoxControllerWasHidden)
                textBoxController.Show();

            TryTurn(GetOppositeDirection(PlayerController.singleton.directionFacing));

            textBoxController.RevealText(GetFormattedSpokenMessage("Hi, how would you like to connect to another trainer?"));

            yield return textBoxController.GetUserChoice(connectionModeMenuOptions);

            //If the menu is launched, the scene shouldn't be unpaused

            switch (textBoxController.userChoiceIndexSelected)
            {

                case 0: //Goodbye

                    if (thisPausedScene)
                        sceneController.SetSceneRunningState(true);

                    break; //Do nothing

                case 1: //Direct connection (server)
                    networkInteractionCanvasController.LaunchDirectConnection(ConnectionMode.Server);
                    break;

                case 2: //Direct connection (client)
                    networkInteractionCanvasController.LaunchDirectConnection(ConnectionMode.Client);
                    break;

                default:

                    Debug.LogError("Invalid connection mode menu option selected - " + textBoxController.userChoiceIndexSelected.ToString());

                    if (thisPausedScene)
                        sceneController.SetSceneRunningState(true);

                    break;

            }

            if (textBoxControllerWasHidden)
                textBoxController.Hide();

        }

    }
}