using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Menus;

namespace Networking.NetworkInteractionCanvas
{
    [RequireComponent(typeof(Canvas))]
    public class NetworkInteractionCanvasController : MonoBehaviour
    {

        public Text statusMessageText;

        public DirectConnectionSectionController directConnectionSectionController;

        private void Start()
        {

            SetUp();

        }

        private void Update()
        {
            StatusMessageTextUpdate();
        }

        public void SetUp()
        {

            ClearStatusMessage();
            directConnectionSectionController.SetUp(this);

        }

        #region Status Message

        private string statusMessageToUpdateTo = null;

        public void SetStatusMessageError(string s)
            => SetStatusMessage("Error: " + s);

        private static readonly object statusMessageLock = new object();

        public void SetStatusMessage(string s)
        {
            lock (statusMessageLock)
            {
                statusMessageToUpdateTo = s;
            }
        }

        private void StatusMessageTextUpdate()
        {

            if (statusMessageToUpdateTo != null)
            {
                statusMessageText.text = statusMessageToUpdateTo;
                statusMessageToUpdateTo = null;
                SetStatusMessageTextState(true);
            }

        }

        public void ClearStatusMessage()
        {
            SetStatusMessageTextState(false);
        }

        private void SetStatusMessageTextState(bool state)
        {
            statusMessageText.enabled = state;
        }

        #endregion

        public void LaunchDirectConnection(ConnectionMode mode)
        {
            directConnectionSectionController.Launch(mode);
        }

    }
}