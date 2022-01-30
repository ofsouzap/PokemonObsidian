using System;
using System.Net.Sockets;
using Networking;
using Serialization;

namespace Battle
{
    public class BattleParticipantNetworkedPlayer : BattleParticipantPlayer
    {

        protected NetworkStream stream;
        protected Serializer serializer;

        public BattleParticipantNetworkedPlayer(NetworkStream stream,
            Serializer serializer)
        {
            this.stream = stream;
            this.serializer = serializer;
        }

        protected override void SetChosenAction(Action action)
        {

            base.SetChosenAction(action);

            //Chosen action must be sent to other player
            Connection.TrySendNetworkBattleAction(stream, serializer, action);

        }

        protected override void SetChosenPokemonIndex(int index)
        {

            base.SetChosenPokemonIndex(index);

            //Chosen next pokemon index must be sent to other player
            Connection.TrySendNetworkBattleNextPokemonIndex(stream, serializer, index);

        }

        public override void ChooseActionFight(int moveIndex)
        {

            base.ChooseActionFight(moveIndex);

            battleManager.SetTextBoxTextToWaitingForOpponent();

        }

    }
}
