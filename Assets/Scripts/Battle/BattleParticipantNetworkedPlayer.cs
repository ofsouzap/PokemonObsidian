using System;
using System.Net.Sockets;
using Networking;
using Serialization;

namespace Battle
{
    public class BattleParticipantNetworkedPlayer : BattleParticipantPlayer
    {

        protected Connection.NetworkBattleCommsManager commsManager;

        public BattleParticipantNetworkedPlayer(Connection.NetworkBattleCommsManager commsManager)
        {
            this.commsManager = commsManager;
        }

        protected override void SetChosenAction(Action action)
        {

            base.SetChosenAction(action);

            //Chosen action must be sent to other player
            commsManager.TrySendBattleAction(action);

        }

        protected override void SetChosenPokemonIndex(int index)
        {

            base.SetChosenPokemonIndex(index);

            //Chosen next pokemon index must be sent to other player
            commsManager.TrySendNextPokemonIndex(index);

        }

        public override void ChooseActionFight(int moveIndex)
        {

            base.ChooseActionFight(moveIndex);

            battleManager.SetTextBoxTextToWaitingForOpponent();

        }

    }
}
