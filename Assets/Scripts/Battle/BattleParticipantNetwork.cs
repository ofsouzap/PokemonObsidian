using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Networking;
using Serialization;
using Pokemon;

namespace Battle
{

    public class BattleParticipantNetwork : BattleParticipant
    {

        protected string name;
        protected PokemonInstance[] pokemon;
        protected string spriteResourceName;

        private NetworkStream stream;
        private Serializer serializer;

        private Queue<Action> chosenActionQueue = new Queue<Action>();
        private Queue<int> nextPokemonIndexQueue = new Queue<int>();

        public BattleParticipantNetwork(NetworkStream stream,
            Serializer serializer,
            string name,
            PokemonInstance[] pokemon,
            string spriteResourceName)
        {

            this.name = name;
            this.pokemon = pokemon;
            this.spriteResourceName = spriteResourceName;
            this.stream = stream;
            this.serializer = serializer;

        }

        ~BattleParticipantNetwork()
        {
            StopRefreshingForNetworkComms();
        }

        public override string GetName() => name;

        public override void StartChoosingAction(BattleData battleData) { }

        public override PokemonInstance[] GetPokemon() => pokemon;

        public override void StartChoosingNextPokemon() { }

        public override bool CheckIfDefeated()
        {
            return GetPokemon().Where(x => x != null).All((x) => x.IsFainted);
        }

        /// <summary>
        /// Starts task of listening for network comms and also starts task to update the queue in this with received comms
        /// </summary>
        public void StartListeningForNetworkComms(BattleParticipant actionsTarget)
        {

            Connection.StartListenForNetworkBattleComms(stream, serializer, this, actionsTarget);

            StartRefreshingForNetworkComms();

        }

        #region Refreshing for Network Comms

        private static readonly object chosenActionLock = new object();

        private void AddChosenAction(Action action)
        {
            lock (chosenActionLock)
            {
                chosenActionQueue.Enqueue(action);
            }
        }

        private Action GetNextChosenAction()
        {
            lock (chosenActionLock)
            {
                return chosenActionQueue.Dequeue();
            }
        }

        private int GetChosenActionQueueLength()
        {
            lock (chosenActionLock)
            {
                return chosenActionQueue.Count;
            }
        }

        private static readonly object nextPokemonLock = new object();

        private void AddNextPokemon(int index)
        {
            lock (nextPokemonLock)
            {
                nextPokemonIndexQueue.Enqueue(index);
            }
        }

        private int GetNextNextPokemonIndex()
        {
            lock (nextPokemonLock)
            {
                return nextPokemonIndexQueue.Dequeue();
            }
        }

        private int GetNextPokemonIndexQueueLength()
        {
            lock (nextPokemonLock)
            {
                return nextPokemonIndexQueue.Count;
            }
        }

        private Task refreshingForNetworkCommsTask = null;
        private bool continueRefreshingForNetworkComms = false;

        private void RefreshingForNetworkComms(int refreshDelay = 100)
        {

            while (continueRefreshingForNetworkComms)
            {

                Connection.NetworkBattleComm? commN = Connection.GetNextNetworkBattleComm();

                if (commN != null)
                {

                    Connection.NetworkBattleComm comm = (Connection.NetworkBattleComm)commN;

                    switch (comm.type)
                    {

                        case Connection.NetworkBattleCommType.Action:
                            AddChosenAction(comm.battleAction);
                            break;

                        case Connection.NetworkBattleCommType.ChosenPokemon:
                            AddNextPokemon(comm.pokemonIndex);
                            break;

                        default:
                            throw new Exception("Unknown network battle comm type");

                    }

                }

                Thread.Sleep(refreshDelay);

            }

        }

        private void StartRefreshingForNetworkComms()
        {

            continueRefreshingForNetworkComms = true;
            if (refreshingForNetworkCommsTask != null && refreshingForNetworkCommsTask.Status == TaskStatus.Running)
            {
                throw new Exception("Already refreshing for network comms");
            }

            refreshingForNetworkCommsTask = new Task(() => RefreshingForNetworkComms());
            refreshingForNetworkCommsTask.Start();

        }

        public void StopRefreshingForNetworkComms()
        {
            continueRefreshingForNetworkComms = false;
        }

        #endregion

        public void StartChoosingAction() { }

        public override void ChooseActionFight(BattleData battleData, bool useStruggle, int moveIndex) { }

        public override bool GetActionHasBeenChosen()
        {
            return GetChosenActionQueueLength() > 0;
        }

        public override Action GetChosenAction()
        {

            if (GetChosenActionQueueLength() > 0)
                return GetNextChosenAction();
            else
                throw new Exception("Trying to get action when none available");

        }
        
        public override bool GetNextPokemonHasBeenChosen()
        {
            return GetNextPokemonIndexQueueLength() > 0;
        }

        public override int GetChosenNextPokemonIndex()
    {

            if (GetNextPokemonIndexQueueLength() > 0)
                return GetNextNextPokemonIndex();
            else
                return -1;

        }

    }

}
