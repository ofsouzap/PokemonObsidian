using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Serialization;
using UnityEngine;
using Battle;
using Pokemon;

namespace Networking
{
    public static partial class Connection
    {

        public abstract class NetworkCommsManager
        {

            #region Comms

            public abstract class Comm
            {

                protected NetworkCommsManager commsManager;
                protected Serializer Serializer => commsManager.serializer;
                protected NetworkStream Stream => commsManager.stream;

                public Comm(NetworkCommsManager commsManager)
                {

                    this.commsManager = commsManager;

                    if (!commTypesVerified)
                        VerifyCommTypes();

                }

                public abstract void ReadArgumentsFromStream();

                public bool TrySendToStream()
                {

                    if (!commsManager.TrySendCommTypeCode(this))
                    {
                        return false;
                    }

                    return TrySendArgumentsToStream();

                }

                protected abstract bool TrySendArgumentsToStream();

                #region Comm Type Data

                public struct CommTypeData
                {

                    public System.Type type;
                    public uint code;
                    public Func<NetworkCommsManager, Comm> constructor;

                    public CommTypeData(System.Type type, uint code, Func<NetworkCommsManager, Comm> constructor)
                    {

                        if (!type.IsSubclassOf(typeof(Comm)))
                            throw new ArgumentException("Type passed wasn't subclass of Comm");

                        this.type = type;
                        this.code = code;
                        this.constructor = constructor;

                    }

                }

                public static readonly CommTypeData[] commTypes = new CommTypeData[]
                {
                    new CommTypeData(typeof(PingComm), PingComm.pingCommCode, ncm => new PingComm(ncm)),
                    new CommTypeData(typeof(BattleActionComm), BattleActionComm.battleActionCommCode, ncm => new BattleActionComm(ncm)),
                    new CommTypeData(typeof(BattleChosenPokemonComm), BattleChosenPokemonComm.battleChosenPokemonCommCode, ncm => new BattleChosenPokemonComm(ncm)),
                    new CommTypeData(typeof(TradeOfferPokemonComm), TradeOfferPokemonComm.tradeOfferPokemonCommCode, ncm => new TradeOfferPokemonComm(ncm)),
                    new CommTypeData(typeof(TradeCancelOfferPokemonComm), TradeCancelOfferPokemonComm.tradeCancelOfferPokemonCommCode, ncm => new TradeCancelOfferPokemonComm(ncm)),
                    new CommTypeData(typeof(TradeAcceptTradeComm), TradeAcceptTradeComm.tradeAcceptTradeCommCode, ncm => new TradeAcceptTradeComm(ncm)),
                    new CommTypeData(typeof(TradeDeclineTradeComm), TradeDeclineTradeComm.tradeDeclineTradeCommCode, ncm => new TradeDeclineTradeComm(ncm)),
                    new CommTypeData(typeof(TradeCloseTradeComm), TradeCloseTradeComm.tradeCloseTradeCommCode, ncm => new TradeCloseTradeComm(ncm)),
                };

                public static bool TryGetCommTypeDataByCode(uint code,
                    out CommTypeData commTypeData)
                {

                    CommTypeData[] matches = commTypes.Where(x => x.code == code).ToArray();

                    if (matches.Length == 0)
                    {
                        commTypeData = default;
                        return false;
                    }
                    else
                    {
                        commTypeData = matches[0];
                        return true;
                    }

                }

                public static bool TryGetCommTypeDataByCommInstance(Comm comm,
                    out CommTypeData commTypeData)
                {

                    CommTypeData[] matches = commTypes.Where(x => comm.GetType() == x.type).ToArray();

                    if (matches.Length == 0)
                    {
                        commTypeData = default;
                        return false;
                    }
                    else
                    {
                        commTypeData = matches[0];
                        return true;
                    }

                }

                private static bool commTypesVerified = false;
                private static void VerifyCommTypes()
                {

                    var grouped = commTypes.GroupBy(x => x.code);
                    if (grouped.Any(p => p.Count() > 1))
                    {
                        throw new Exception("Duplicate code found in commTypes");
                    }

                    commTypesVerified = true;

                }

                #endregion

            }

            public class PingComm : Comm
            {

                public const uint pingCommCode = 0x00000001;

                public PingComm(NetworkCommsManager commsManager) : base(commsManager) { }

                public override void ReadArgumentsFromStream() { } //Ping takes no arguments

                protected override bool TrySendArgumentsToStream() { return true; }

            }

            #region Battle

            public class BattleActionComm : Comm
            {

                public const uint battleActionCommCode = 0x00000002;

                public BattleParticipant.Action action;

                public BattleActionComm(NetworkCommsManager commsManager) : base(commsManager) { }

                public BattleActionComm(NetworkCommsManager commsManager, BattleParticipant.Action action) : base(commsManager)
                {
                    this.action = action;
                }

                public override void ReadArgumentsFromStream()
                {

                    if (commsManager is NetworkBattleCommsManager battleCommsManager)
                    {
                        action = Serializer.DeserializeBattleAction(Stream, battleCommsManager.recvActionUser, battleCommsManager.recvActionTarget);
                    }
                    else
                    {
                        throw new ArgumentException("Comms manager wasn't battle comms manager for battle action comm initialiser");
                    }

                }

                protected override bool TrySendArgumentsToStream()
                {

                    try
                    {
                        Serializer.SerializeBattleAction(Stream, action);
                        return true;
                    }
                    catch (IOException)
                    {
                        return false;
                    }

                }

            }

            public class BattleChosenPokemonComm : Comm
            {

                public const uint battleChosenPokemonCommCode = 0x00000003;

                public int pokemonIndex;

                public BattleChosenPokemonComm(NetworkCommsManager commsManager) : base(commsManager) { }

                public BattleChosenPokemonComm(NetworkCommsManager commsManager, int pokemonIndex) : base(commsManager)
                {
                    this.pokemonIndex = pokemonIndex;
                }

                public override void ReadArgumentsFromStream()
                {

                    byte[] buffer = new byte[4];
                    Stream.Read(buffer, 0, 4);
                    pokemonIndex = BitConverter.ToInt32(buffer, 0);

                }

                protected override bool TrySendArgumentsToStream()
                {

                    try
                    {
                        Stream.Write(BitConverter.GetBytes(pokemonIndex), 0, 4);
                        return true;
                    }
                    catch (IOException)
                    {
                        return false;
                    }

                }

            }

            #endregion

            #region Trade

            public class TradeOfferPokemonComm : Comm
            {

                public const uint tradeOfferPokemonCommCode = 0x00000004;

                public PokemonInstance pokemon;

                public TradeOfferPokemonComm(NetworkCommsManager commsManager) : base(commsManager) { }

                public TradeOfferPokemonComm(NetworkCommsManager commsManager, PokemonInstance pokemon) : base(commsManager)
                {
                    this.pokemon = pokemon;
                }

                public override void ReadArgumentsFromStream()
                {
                    pokemon = Serializer.DeserializePokemonInstance(Stream);
                }

                protected override bool TrySendArgumentsToStream()
                {
                    try
                    {
                        Serializer.SerializePokemonInstance(Stream, pokemon);
                        return true;
                    }
                    catch (IOException)
                    {
                        return false;
                    }
                }

            }

            public class TradeCancelOfferPokemonComm : Comm
            {

                public const uint tradeCancelOfferPokemonCommCode = 0x00000005;

                public TradeCancelOfferPokemonComm(NetworkCommsManager commmsManager) : base(commmsManager) { }

                public override void ReadArgumentsFromStream() { }

                protected override bool TrySendArgumentsToStream() { return true; }

            }

            public class TradeAcceptTradeComm : Comm
            {

                public const uint tradeAcceptTradeCommCode = 0x00000006;

                public TradeAcceptTradeComm(NetworkCommsManager commmsManager) : base(commmsManager) { }

                public override void ReadArgumentsFromStream() { }

                protected override bool TrySendArgumentsToStream() { return true; }

            }

            public class TradeDeclineTradeComm : Comm
            {

                public const uint tradeDeclineTradeCommCode = 0x00000007;

                public TradeDeclineTradeComm(NetworkCommsManager commmsManager) : base(commmsManager) { }

                public override void ReadArgumentsFromStream() { }

                protected override bool TrySendArgumentsToStream() { return true; }

            }

            public class TradeCloseTradeComm : Comm
            {

                public const uint tradeCloseTradeCommCode = 0x00000008;

                public TradeCloseTradeComm(NetworkCommsManager commmsManager) : base(commmsManager) { }

                public override void ReadArgumentsFromStream() { }

                protected override bool TrySendArgumentsToStream() { return true; }

            }

            #endregion

            #endregion

            protected NetworkStream stream;
            public Serializer serializer
            {
                get;
                protected set;
            }
            public int refreshDelay = 100;
            byte connectionTestPingRefreshDelays = 10;

            protected bool TrySendConnectionTestPing()
            {

                PingComm comm = new PingComm(this);

                if (comm.TrySendToStream())
                    return true;
                else
                    return false;

            }

            protected bool TrySendCommTypeCode(uint code)
            {

                try
                {
                    stream.Write(BitConverter.GetBytes(code), 0, 4);
                    return true;
                }
                catch (IOException)
                {
                    return false;
                }

            }

            protected bool TrySendCommTypeCode(Comm comm)
            {

                if (Comm.TryGetCommTypeDataByCommInstance(comm, out Comm.CommTypeData commTypeData))
                {
                    return TrySendCommTypeCode(commTypeData.code);
                }
                else
                    return false;

            }

            protected uint ReceiveCommTypeCode()
            {

                byte[] buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                uint code = BitConverter.ToUInt32(buffer, 0);
                return code;

            }

            protected bool TryReceiveComm(out Comm comm)
            {

                uint code = ReceiveCommTypeCode();

                if (!Comm.TryGetCommTypeDataByCode(code, out Comm.CommTypeData commTypeData))
                {
                    comm = default;
                    return false;
                }

                comm = commTypeData.constructor(this);
                comm.ReadArgumentsFromStream();

                return true;

            }

            private bool listeningForComms = false;
            private Task listenForCommsTask = null;

            #region Comms Queue

            private readonly object recvCommLock = new object();
            private Queue<Comm> commQueue = new Queue<Comm>();

            protected void ClearCommsQueue()
            {
                lock (recvCommLock)
                {
                    commQueue.Clear();
                }
            }

            protected void EnqueueComm(Comm comm)
            {
                lock (recvCommLock)
                {
                    commQueue.Enqueue(comm);
                }
            }

            public Comm GetNextComm()
            {
                lock (recvCommLock)
                {

                    if (commQueue.Count > 0)
                        return commQueue.Dequeue();
                    else
                        return null;

                }
            }

            #endregion

            #region Connection Error Occured

            private readonly object connErrorOccuredLock = new object();
            protected bool connErrorOccured = false;

            protected void SetConnErrorOccured(bool state)
            {
                lock (connErrorOccuredLock)
                {
                    connErrorOccured = state;
                }
            }

            public bool CommsConnErrorOccured
            {
                get
                {
                    lock (connErrorOccuredLock)
                    {
                        return connErrorOccured;
                    }
                }
            }

            #endregion

            public NetworkCommsManager(NetworkStream stream,
                Serializer serializer,
                int refreshDelay = 100,
                byte connectionTestPingRefreshDelays = 10)
            {

                this.stream = stream;
                this.serializer = serializer;
                this.refreshDelay = refreshDelay;
                this.connectionTestPingRefreshDelays = connectionTestPingRefreshDelays;

            }

            public virtual void StartListening()
            {

                if (listenForCommsTask != null && listenForCommsTask.Status == TaskStatus.Running)
                {
                    throw new Exception("Comms manager task already running");
                }

                ClearCommsQueue();
                SetConnErrorOccured(false);
                listeningForComms = true;

                listenForCommsTask = new Task(() => WaitForRecvComm());
                listenForCommsTask.Start();

            }

            public void StopListening()
            {
                listeningForComms = false;
            }

            protected void WaitForRecvComm()
            {

                byte connectionTestPingRefreshCounter = 0;

                while (listeningForComms)
                {

                    if (!CommsConnErrorOccured) //If a connection error has already been detected, no need to check for one
                    {

                        connectionTestPingRefreshCounter++;

                        if (connectionTestPingRefreshCounter >= connectionTestPingRefreshDelays)
                        {

                            if (!TrySendConnectionTestPing())
                            {
                                SetConnErrorOccured(true);
                            }

                            connectionTestPingRefreshCounter = 0;

                        }

                    }

                    if (stream.DataAvailable)
                    {
                        if (!TryReceiveComm(out Comm comm))
                        {
                            SetConnErrorOccured(true);
                        }
                        else
                        {

                            if (!(comm is PingComm))
                                EnqueueComm(comm);

                        }
                    }

                    Thread.Sleep(refreshDelay);

                }

            }

        }

        public class NetworkBattleCommsManager : NetworkCommsManager
        {

            public BattleParticipant recvActionUser;
            public BattleParticipant recvActionTarget;

            public NetworkBattleCommsManager(NetworkStream stream,
                Serializer serializer,
                BattleParticipant recvActionUser,
                BattleParticipant recvActionTarget,
                int refreshDelay = 100,
                byte connectionTestPingRefreshDelays = 10) : base(stream, serializer, refreshDelay, connectionTestPingRefreshDelays)
            {

                this.recvActionUser = recvActionUser;
                this.recvActionTarget = recvActionTarget;

            }

            public NetworkBattleCommsManager(NetworkStream stream,
                Serializer serializer,
                int refreshDelay = 100,
                byte connectionTestPingRefreshDelays = 10) : this(stream, serializer, null, null, refreshDelay, connectionTestPingRefreshDelays)
            { }

            public void SetRecvActionParticipants(BattleParticipant user,
                BattleParticipant target)
            {
                recvActionUser = user;
                recvActionTarget = target;
            }

            public bool TrySendBattleAction(BattleParticipant.Action action)
            {

                BattleActionComm comm = new BattleActionComm(this, action);

                if (!comm.TrySendToStream())
                {
                    SetConnErrorOccured(true);
                    return false;
                }
                else
                    return true;

            }

            public bool TrySendNextPokemonIndex(int index)
            {

                BattleChosenPokemonComm comm = new BattleChosenPokemonComm(this, index);

                if (!comm.TrySendToStream())
                {
                    SetConnErrorOccured(true);
                    return false;
                }
                else
                    return true;

            }

        }

        public class NetworkTradeCommsManager : NetworkCommsManager
        {

            public NetworkTradeCommsManager(NetworkStream stream,
                Serializer serializer,
                int refreshDelay = 100,
                byte connectionTestPingRefreshDelays = 10) : base(stream, serializer, refreshDelay, connectionTestPingRefreshDelays)
            { }

            public bool TrySendOfferPokemon(PokemonInstance pokemon)
            {
                TradeOfferPokemonComm comm = new TradeOfferPokemonComm(this, pokemon);
                return comm.TrySendToStream();
            }

            public bool TrySendCancelOfferPokemon()
            {
                TradeCancelOfferPokemonComm comm = new TradeCancelOfferPokemonComm(this);
                return comm.TrySendToStream();
            }

            public bool TrySendAcceptTrade()
            {
                TradeAcceptTradeComm comm = new TradeAcceptTradeComm(this);
                return comm.TrySendToStream();
            }

            public bool TrySendDeclineTrade()
            {
                TradeDeclineTradeComm comm = new TradeDeclineTradeComm(this);
                return comm.TrySendToStream();
            }

            public bool TrySendCloseTrade()
            {
                TradeCloseTradeComm comm = new TradeCloseTradeComm(this);
                return comm.TrySendToStream();
            }

        }

    }
}
