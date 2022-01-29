using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Serialization;
using Pokemon;
using Battle;

namespace Networking
{
    public static class Connection
    {

        public const ushort pokemonObsidianIdentifier = 0x6f70;
        public static byte[] PokemonObsidianIdentifierBytes => BitConverter.GetBytes(pokemonObsidianIdentifier);

        public const uint yesCode = 0x706b6361;
        public static byte[] YesCodeBytes => BitConverter.GetBytes(yesCode);

        public const uint noCode = 0x6e686170;
        public static byte[] NoCodeBytes => BitConverter.GetBytes(noCode);

        public const uint networkBattleCommActionCode = 0x809ef0ab;
        public static byte[] NetworkBattleCommActionCodeBytes => BitConverter.GetBytes(networkBattleCommActionCode);

        public const uint networkBattleCommChosenPokemonCode = 0x809fceab;
        public static byte[] NetworkBattleCommChosenPokemonCodeBytes => BitConverter.GetBytes(networkBattleCommChosenPokemonCode);

        public const int mainPort = 59004;
        public const int discoveryPort = 59005;

        public const SocketType socketType = SocketType.Stream;
        public const ProtocolType protocolType = ProtocolType.Tcp;

        public const int defaultTimeout = 5000;

        public static IPAddress GetHostIPAddress()
        {
            string hostname = Dns.GetHostName();
            return Dns.GetHostEntry(hostname).AddressList
                .Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                .ToArray()[0];
        }

        public delegate void LogCallback(string msg);

        public static void LogNetworkEvent(string msg)
        {
            if (GameSettings.singleton.networkLogEnabled)
                Debug.Log(msg);
        }

        #region Standard Codes

        private static void SendYes(NetworkStream stream)
        {
            stream.Write(YesCodeBytes, 0, 4);
        }

        private static void SendNo(NetworkStream stream)
        {
            stream.Write(NoCodeBytes, 0, 4);
        }

        private static bool? TryReceiveResponseYN(NetworkStream stream,
            out uint recvAck)
        {

            byte[] buffer = new byte[4];

            stream.Read(buffer, 0, 4);

            recvAck = BitConverter.ToUInt32(buffer, 0);

            if (recvAck == yesCode)
                return true;
            else if (recvAck == noCode)
                return false;
            else
                return null;

        }

        private static void SendPokemonObsidianIdentifier(NetworkStream stream)
        {
            stream.Write(PokemonObsidianIdentifierBytes, 0, 2);
        }

        private static bool TryReceivePokemonObsidianIdentifier(NetworkStream stream,
            out ushort recvPmonId)
        {

            byte[] buffer = new byte[2];

            stream.Read(buffer, 0, 2);

            recvPmonId = BitConverter.ToUInt16(buffer, 0);

            return recvPmonId == pokemonObsidianIdentifier;

        }

        private static void SendNetworkBattleCommType(NetworkStream stream, NetworkBattleCommType type)
        {

            switch (type)
            {

                case NetworkBattleCommType.Action:
                    stream.Write(NetworkBattleCommActionCodeBytes, 0, 4);
                    break;

                case NetworkBattleCommType.ChosenPokemon:
                    stream.Write(NetworkBattleCommChosenPokemonCodeBytes, 0, 4);
                    break;

                default:
                    throw new Exception("Unknown type");

            }

        }

        private static NetworkBattleCommType? ReceiveNetworkBattleCommType(NetworkStream stream)
        {

            byte[] buffer = new byte[4];

            stream.Read(buffer, 0, 4);

            uint recvCode = BitConverter.ToUInt32(buffer, 0);

            if (recvCode == networkBattleCommActionCode)
                return NetworkBattleCommType.Action;
            else if (recvCode == networkBattleCommChosenPokemonCode)
                return NetworkBattleCommType.ChosenPokemon;
            else
                return null;

        }

        #endregion

        #region Connection Establishment

        /// <param name="port">The port to use. Negative means to use the default port</param>
        public static bool TryConnectToServer(out Socket socket,
            LogCallback errCallback,
            LogCallback statusCallback,
            string ipAddress,
            int port)
        {

            IPAddress address;

            try
            {
                address = IPAddress.Parse(ipAddress);
            }
            catch (NullReferenceException)
            {
                socket = default;
                errCallback("IP address provided is null");
                return false;
            }
            catch (FormatException)
            {
                socket = default;
                errCallback("Invalid IP address format");
                return false;
            }

            return TryConnectToServer(out socket,
                errCallback,
                statusCallback,
                address,
                port);

        }

        /// <param name="port">The port to use. Negative means to use the default port</param>
        public static bool TryConnectToServer(out Socket socket,
            LogCallback errCallback,
            LogCallback statusCallback,
            IPAddress ipAddress,
            int port = -1)
        {

            if (port < 0)
                port = mainPort;

            if (port < IPEndPoint.MinPort)
            {
                socket = default;
                errCallback("Port number provided too small");
                return false;
            }

            if (port > IPEndPoint.MaxPort)
            {
                socket = default;
                errCallback("Port number provided too great");
                return false;
            }

            socket = new Socket(socketType, protocolType);
            LogNetworkEvent($"Socket created");

            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            LogNetworkEvent($"Socket end point is {ipAddress}:{port}");

            if (!GameSettings.singleton.networkTimeoutDisabled)
            {
                socket.ReceiveTimeout = defaultTimeout;
                LogNetworkEvent("Set socket receive timeout");
            }

            try
            {

                statusCallback("Connecting to server...");
                socket.Connect(endPoint);
                LogNetworkEvent("Connected to server");
                statusCallback("Connected to server");

            }
            catch (SocketException e)
            {
                socket.Close();
                errCallback("Socket Error: " + e.Message);
                LogNetworkEvent("Socket Error Message: \"" + e.Message + "\". Error Stack: " + e.StackTrace);
                return false;
            }
            catch (ObjectDisposedException)
            {
                socket.Close();
                errCallback("Socket closed unexpectedly");
                return false;
            }
            catch (System.Security.SecurityException)
            {
                socket.Close();
                errCallback("Invalid permissions");
                return false;
            }
            catch (InvalidOperationException)
            {
                socket.Close();
                errCallback("Socket unexpectedly in listening mode");
                return false;
            }
            
            return true;

        }

        public delegate void StartedListening(IPEndPoint endPoint);
        public delegate void ClientConnected(bool success, Socket socket);

        /// <param name="port">The port to use. Negative means to use the default port</param>
        /// <param name="clientConnectedListener">The function to call when a client is connected. ONLY FOR SYNCHRONOUS MODE</param>
        public static void TryStartHostServer(StartedListening startedListeningListener,
            ClientConnected clientConnectedListener,
            LogCallback errCallback,
            LogCallback statusCallback,
            int port = -1,
            bool useAsync = true)
        {

            if (port < 0)
                port = mainPort;

            IPEndPoint ep = new IPEndPoint(GetHostIPAddress(), port);

            if (!useAsync)
            {

                Socket sock = SyncServerListening(ep,
                    errCallback,
                    statusCallback);

                startedListeningListener?.Invoke(ep);

                clientConnectedListener?.Invoke(
                    true,
                    sock);

            }
            else
            {

                statusCallback("Awaiting connection...");

                StartAsyncServerListening(ep, errCallback, statusCallback);

                startedListeningListener?.Invoke(ep);

            }

        }

        public static Socket FetchAsyncNewClient()
        {

            if (asyncNewClient != null)
            {
                Socket s = asyncNewClient;
                SetAsyncNewClient(null);
                return s;
            }
            else
                return null;

        }

        private static Socket SyncServerListening(IPEndPoint ep,
            LogCallback errCallback,
            LogCallback statusCallback)
        {

            Socket listenerSock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            listenerSock.Bind(ep);
            listenerSock.Listen(1);

            statusCallback($"Waiting for other user at {ep.Address}:{ep.Port}...");

            Socket sock = listenerSock.Accept();

            statusCallback("Other user connected");

            return sock;

        }

        private static void StartAsyncServerListening(IPEndPoint ep,
            LogCallback errCallback,
            LogCallback statusCallback)
        {

            awaitingClient = true;

            Task serverTask = new Task(() => AsyncServerListeningTask(ep));

            serverTask.Start();

        }

        #region Async Functionality

        private static readonly object asyncNewClientLock = new object();
        private static bool awaitingClient = false;
        private static Socket asyncNewClient = null;

        private static void SetAsyncNewClient(Socket sock)
        {

            lock (asyncNewClientLock)
            {

                if (sock == null)
                    asyncNewClient = null;
                else
                {

                    if (!awaitingClient)
                        throw new Exception("Unecessary client connected");

                    asyncNewClient = sock;
                    awaitingClient = false;

                }

            }

        }

        private static void AsyncServerListeningTask(IPEndPoint ep)
        {

            Socket listenerSock = new Socket(SocketType.Stream, ProtocolType.Tcp);

            listenerSock.Bind(ep);
            listenerSock.Listen(1);

            Socket sock = listenerSock.Accept();

            SetAsyncNewClient(sock);

        }

        #endregion

        #endregion

        #region Network Stream

        public static NetworkStream CreateNetworkStream(Socket socket)
        {

            //Having the network stream own the socket should make the socket close when the network stream is closed
            NetworkStream stream = new NetworkStream(socket, true);
            stream.ReadTimeout = GameSettings.singleton.networkTimeoutDisabled ? -1 : defaultTimeout;
            stream.WriteTimeout = GameSettings.singleton.networkTimeoutDisabled ? -1 : defaultTimeout;
            return stream;

        }

        #endregion

        #region Connection Verification

        public static bool VerifyConnection_Client(NetworkStream stream,
            LogCallback errCallback,
            LogCallback statusCallback)
        {

            LogNetworkEvent("Starting client connection verification");

            try
            {

                byte[] buffer;

                //Server sends Pokemon Obsidian Identifier

                LogNetworkEvent("Receiving identifier...");

                if (!TryReceivePokemonObsidianIdentifier(stream, out _))
                {
                    errCallback("Identifier mismatch");
                    LogNetworkEvent("Pokemon Obsidian Identifier mismatch");
                    SendNo(stream);
                    return false;
                }
                else
                {
                    LogNetworkEvent("Identifier matches");
                    SendYes(stream);
                }

                //Client sends Pokemon Obsidian Identifier
                SendPokemonObsidianIdentifier(stream);
                LogNetworkEvent("Sent identifier");

                if (TryReceiveResponseYN(stream, out _) != true)
                {
                    errCallback("Failed to receive identifier yes response");
                    return false;
                }

                //Server sends game version
                buffer = new byte[2];
                LogNetworkEvent("Receiving game version");
                stream.Read(buffer, 0, 2);
                statusCallback("Received game version");
                ushort serverGameVersion = BitConverter.ToUInt16(buffer, 0);

                if (serverGameVersion != GameVersion.version)
                {
                    statusCallback("Mismatching game versions");
                    errCallback("Game version mismatch");
                    SendNo(stream);
                    return false;
                }

                //Server sends serializer version
                buffer = new byte[2];
                LogNetworkEvent("Receiving serializer version");
                stream.Read(buffer, 0, 2);
                LogNetworkEvent("Received serializer version");
                ushort serverSerializerVersion = BitConverter.ToUInt16(buffer, 0);

                if (serverSerializerVersion != Serialize.defaultSerializerVersion)
                {
                    statusCallback("Mismatching game versions");
                    errCallback("Serializer version mismatch");
                    SendNo(stream);
                    return false;
                }

                //Client sends yes
                SendYes(stream);

                return true;

            }
            catch (IOException e) //It is assumed that the network stream timed out
            {
                errCallback("IOException. Message: \"" + e.Message + "\" stack trace: " + e.StackTrace);
                if (e.InnerException is SocketException sockE)
                    errCallback($"IOException's inner socket exception code: {sockE.SocketErrorCode}");
                return false;
            }

        }

        public static bool VerifyConnection_Server(NetworkStream stream,
            LogCallback errCallback,
            LogCallback statusCallback)
        {

            LogNetworkEvent("Starting server connection verification");

            try
            {

                byte[] buffer;

                //Server sends Pokemon Obsidian Identifier
                SendPokemonObsidianIdentifier(stream);
                LogNetworkEvent("Sent identifier");

                if (TryReceiveResponseYN(stream, out _) != true)
                {
                    errCallback("Failed to receive identifier yes response");
                    return false;
                }

                //Client sends Pokemon Obsidian Identifier

                LogNetworkEvent("Receiving identifier...");

                if (!TryReceivePokemonObsidianIdentifier(stream, out ushort recvPmonId))
                {
                    errCallback("Identifier mismatch");
                    SendNo(stream);
                    return false;
                }
                else
                {
                    LogNetworkEvent("Identifier matches");
                    SendYes(stream);
                }

                //Server sends game version
                buffer = GameVersion.VersionBytes;
                statusCallback("Sending game version...");
                stream.Write(buffer, 0, 2);
                statusCallback("Sent game version");

                //Server sends serializer version
                buffer = BitConverter.GetBytes(Serialize.defaultSerializerVersion);
                statusCallback("Sending serializer version...");
                stream.Write(buffer, 0, 2);
                statusCallback("Sent serializer version");

                if (TryReceiveResponseYN(stream, out _) != true)
                {
                    statusCallback("Mismatching game versions");
                    errCallback("Failed to receive versions yes response");
                    return false;
                }

                return true;

            }
            catch (IOException e)
            {
                errCallback("IOException. Message: \"" + e.Message + "\" stack trace: " + e.StackTrace);
                if (e.InnerException is SocketException sockE)
                    errCallback($"IOException's inner socket exception code: {sockE.SocketErrorCode}");
                return false;
            }

        }

        #endregion

        #region Exchange Battle Entrance Arguments

        private static bool TrySendBattleEntranceArguments(NetworkStream stream,
            LogCallback errCallback,
            LogCallback statusCallback,
            Serializer serializer,
            PlayerData player = null)
        {

            try
            {

                LogNetworkEvent("Starting to send battle arguments");

                LogNetworkEvent($"Sending name ({player.profile.name})...");
                serializer.SerializeString(stream, player.profile.name);
                LogNetworkEvent("Sent name");

                LogNetworkEvent("Starting to send party pokemon...");

                for (byte i = 0; i < PlayerData.partyCapacity; i++)
                {

                    PokemonInstance pokemon = player.partyPokemon[i];

                    if (pokemon == null)
                    {
                        LogNetworkEvent("Sending null pokemon...");
                        serializer.SerializeBool(stream, false);
                        LogNetworkEvent("Sent null pokemon");
                    }
                    else
                    {

                        LogNetworkEvent("Sending pokemon...");
                        serializer.SerializeBool(stream, true);
                        serializer.SerializePokemonInstance(stream, pokemon);
                        LogNetworkEvent($"Sent pokemon {string.Format("{0:X}", pokemon.Hash)}");

                    }

                }

                LogNetworkEvent("Sent party pokemon");

                LogNetworkEvent($"Sending sprite name ({player.profile.SpriteName})...");
                serializer.SerializeString(stream, player.profile.SpriteName);
                LogNetworkEvent("Sent sprite name");

                LogNetworkEvent("Battle entrance arguments sent");

                return true;

            }
            catch (IOException e)
            {
                errCallback("IOException\n-->Message: \"" + e.Message + "\"\n-->Stack Trace:\n" + e.StackTrace);
                return false;
            }

        }

        private static bool TryReceiveBattleEntranceArguments(NetworkStream stream,
            LogCallback errCallback,
            LogCallback statusCallback,
            Serializer serializer,
            out string name,
            out PokemonInstance[] pokemon,
            out string spriteResourceName)
        {

            try
            {

                LogNetworkEvent("Starting to receive battle entrance arguments");

                LogNetworkEvent("Receiving name...");
                name = serializer.DeserializeString(stream);
                LogNetworkEvent($"Received name ({name})");

                LogNetworkEvent("Receiving party...");

                pokemon = new PokemonInstance[PlayerData.partyCapacity];

                for (byte i = 0; i < PlayerData.partyCapacity; i++)
                {

                    LogNetworkEvent("Receiving pokemon is not null");
                    bool pokemonIsNotNull = serializer.DeserializeBool(stream);
                    LogNetworkEvent("Received pokemon is not null (" + (pokemonIsNotNull ? "true" : "false") + ")");

                    if (!pokemonIsNotNull)
                    {
                        LogNetworkEvent("(Pokemon was null, setting as such)");
                        pokemon[i] = null;
                    }
                    else
                    {

                        LogNetworkEvent("Receiving pokemon instance...");
                        pokemon[i] = serializer.DeserializePokemonInstance(stream);
                        LogNetworkEvent($"Received pokemon instance (Hash - {string.Format("{0:X}", pokemon[i].Hash)})");

                    }

                }

                LogNetworkEvent("Received party");

                LogNetworkEvent("Receiving sprite name...");
                spriteResourceName = serializer.DeserializeString(stream);
                LogNetworkEvent("Received sprite name");

                return true;

            }
            catch (IOException e)
            {
                errCallback("IOException\n-->Message: \"" + e.Message + "\"\n-->Stack Trace:\n" + e.StackTrace);
                name = default;
                pokemon = default;
                spriteResourceName = default;
                return false;
            }

        }

        public static bool TryExchangeBattleEntranceArguments_Client(NetworkStream stream,
            LogCallback errCallback,
            LogCallback statusCallback,
            out string name,
            out PokemonInstance[] pokemon,
            out string spriteResourceName,
            out int randomSeed,
            PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            Serializer serializer = Serialize.DefaultSerializer;

            try
            {

                if (!TryReceiveBattleEntranceArguments(stream,
                    errCallback,
                    statusCallback,
                    serializer,
                    out name,
                    out pokemon,
                    out spriteResourceName))
                {

                    errCallback("Failed to receive battle entrance arguments");
                    SendNo(stream);
                    randomSeed = default;
                    return false;

                }

                SendYes(stream);

                if (!TrySendBattleEntranceArguments(stream,
                    errCallback,
                    statusCallback,
                    serializer,
                    player))
                {
                    errCallback("Failed to send battle arguments");
                    randomSeed = default;
                    return false;
                }

                switch (TryReceiveResponseYN(stream, out _))
                {

                    case false:
                    case null:
                        errCallback("Didn't receive yes after sending battle entrance arguments");
                        randomSeed = default;
                        return false;

                    case true:
                        break;

                }

                LogNetworkEvent("Receiving random seed...");
                byte[] randomSeedBuffer = new byte[4];
                stream.Read(randomSeedBuffer, 0, 4);
                randomSeed = BitConverter.ToInt32(randomSeedBuffer, 0);
                LogNetworkEvent($"Received random seed {randomSeed}");

                return true;

            }
            catch (IOException)
            {
                errCallback("Connection lost");
                name = default;
                pokemon = default;
                spriteResourceName = default;
                randomSeed = default;
                return false;
            }

        }

        public static bool TryExchangeBattleEntranceArguments_Server(NetworkStream stream,
            LogCallback errCallback,
            LogCallback statusCallback,
            out string name,
            out PokemonInstance[] pokemon,
            out string spriteResourceName,
            PlayerData player = null,
            int randomSeed = 0)
        {

            if (player == null)
                player = PlayerData.singleton;

            Serializer serializer = Serialize.DefaultSerializer;

            try
            {

                if (!TrySendBattleEntranceArguments(stream,
                    errCallback,
                    statusCallback,
                    serializer,
                    player))
                {
                    errCallback("Failed to send battle arguments");
                    name = default;
                    pokemon = default;
                    spriteResourceName = default;
                    return false;
                }

                switch (TryReceiveResponseYN(stream, out _))
                {

                    case false:
                    case null:
                        errCallback("Didn't receive yes after sending battle entrance arguments");
                        name = default;
                        pokemon = default;
                        spriteResourceName = default;
                        return false;

                    case true:
                        break;

                }

                if (!TryReceiveBattleEntranceArguments(stream,
                    errCallback,
                    statusCallback,
                    serializer,
                    out name,
                    out pokemon,
                    out spriteResourceName))
                {

                    errCallback("Failed to receive battle entrance arguments");
                    SendNo(stream);
                    return false;

                }

                SendYes(stream);

                LogNetworkEvent($"Sending random seed ({randomSeed})...");
                stream.Write(BitConverter.GetBytes(randomSeed), 0, 4);
                LogNetworkEvent($"Sent random seed");

                return true;

            }
            catch (IOException)
            {
                errCallback("Connection lost");
                name = default;
                pokemon = default;
                spriteResourceName = default;
                return false;
            }

        }

        #endregion

        #region Network Battle Comms

        public enum NetworkBattleCommType
        {
            Action,
            ChosenPokemon
        }

        public struct NetworkBattleComm
        {

            public NetworkBattleCommType type;
            public BattleParticipant.Action battleAction; //For actions
            public int pokemonIndex; //For chosen pokemon


            public NetworkBattleComm(BattleParticipant.Action battleAction)
            {
                type = NetworkBattleCommType.Action;
                this.battleAction = battleAction;
                pokemonIndex = -1;
            }

            public NetworkBattleComm(int pokemonIndex)
            {
                type = NetworkBattleCommType.ChosenPokemon;
                battleAction = default;
                this.pokemonIndex = pokemonIndex;
            }

        }

        private static bool listeningForNetworkBattleComms = false;
        private static Task listenForNetworkBattleCommsTask = null;

        private static BattleParticipant networkBattleCommActionUser = null;
        private static BattleParticipant networkBattleCommActionTarget = null;

        #region Network Battle Comms Queue

        private static readonly object recvBattleCommLock = new object();
        private static Queue<NetworkBattleComm> networkBattleCommsQueue = new Queue<NetworkBattleComm>();

        private static void ClearNetworkBattleCommsQueue()
        {
            lock (recvBattleCommLock)
            {
                networkBattleCommsQueue.Clear();
            }
        }

        private static void EnqueueNetworkBattleComm(NetworkBattleComm comm)
        {
            lock (recvBattleCommLock)
            {
                networkBattleCommsQueue.Enqueue(comm);
            }
        }

        public static NetworkBattleComm? GetNextNetworkBattleComm()
        {
            lock (recvBattleCommLock)
            {

                if (networkBattleCommsQueue.Count > 0)
                    return networkBattleCommsQueue.Dequeue();

                else
                    return null;

            }
        }

        #endregion

        #region Connection Error Occured

        private static readonly object networkCommsConnErrorOccuredLock = new object();
        private static bool networkCommsConnErrorOccured = false;

        private static void SetNetworkCommsConnErrorOccured(bool state)
        {
            lock (networkCommsConnErrorOccuredLock)
            {
                networkCommsConnErrorOccured = state;
            }
        }

        public static bool NetworkCommsConnErrorOccured
        {
            get => networkCommsConnErrorOccured;
        }

        #endregion

        public static void StartListenForNetworkBattleComms(NetworkStream stream,
            Serializer serializer,
            BattleParticipant actionUser,
            BattleParticipant actionTarget,
            int refreshDelay = 100)
        {

            networkBattleCommActionUser = actionUser;
            networkBattleCommActionTarget = actionTarget;

            if (listenForNetworkBattleCommsTask != null && listenForNetworkBattleCommsTask.Status == TaskStatus.Running)
            {
                throw new Exception("Network battle comms listener task already running");
            }

            ClearNetworkBattleCommsQueue();
            SetNetworkCommsConnErrorOccured(false);
            listeningForNetworkBattleComms = true;

            listenForNetworkBattleCommsTask = new Task(() => WaitForRecvAction(stream, serializer, refreshDelay));
            listenForNetworkBattleCommsTask.Start();

        }

        public static void StopListenForNetworkBattleComms()
        {
            listeningForNetworkBattleComms = false;
        }

        private static void WaitForRecvAction(NetworkStream stream,
            Serializer serializer,
            int refreshDelay = 100)
        {

            while (listeningForNetworkBattleComms)
            {

                if (stream.CanRead && stream.DataAvailable)
                {
                    if (!TryReceiveNetworkBattleComm(stream, serializer, out NetworkBattleComm comm))
                    {
                        SetNetworkCommsConnErrorOccured(true);
                    }
                    else
                    {
                        EnqueueNetworkBattleComm(comm);
                    }
                }

                Thread.Sleep(refreshDelay);

            }

        }

        private static bool TryReceiveNetworkBattleComm(NetworkStream stream,
            Serializer serializer,
            out NetworkBattleComm comm)
        {

            switch (ReceiveNetworkBattleCommType(stream))
            {

                case NetworkBattleCommType.Action:

                    #region Read Action

                    BattleParticipant.Action action = serializer.DeserializeBattleAction(stream,
                        networkBattleCommActionUser,
                        networkBattleCommActionTarget);
                    comm = new NetworkBattleComm(action);

                    #endregion

                    return true;

                case NetworkBattleCommType.ChosenPokemon:

                    #region Read Chosen Pokemon

                    byte[] indexBuffer = new byte[4];
                    stream.Read(indexBuffer, 0, 4);

                    int index = BitConverter.ToInt32(indexBuffer, 0);

                    comm = new NetworkBattleComm(index);

                    #endregion

                    return true;

                case null:
                    comm = default;
                    return false;

                default:
                    comm = default;
                    return false;

            }

        }

        public static bool TrySendNetworkBattleAction(NetworkStream stream,
            Serializer serializer,
            BattleParticipant.Action action)
        {

            try
            {

                SendNetworkBattleCommType(stream, NetworkBattleCommType.Action);

                serializer.SerializeBattleAction(stream, action);

                return true;

            }
            catch (IOException)
            {
                SetNetworkCommsConnErrorOccured(true);
                return false;
            }

        }

        public static bool TrySendNetworkBattleNextPokemonIndex(NetworkStream stream,
            Serializer serializer,
            int index)
        {

            SendNetworkBattleCommType(stream, NetworkBattleCommType.ChosenPokemon);

            byte[] buffer = BitConverter.GetBytes(index);
            stream.Write(buffer, 0, 4);

            return true;

        }

        #endregion

    }
}
