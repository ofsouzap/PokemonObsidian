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
    public static partial class Connection
    {

        public const ushort pokemonObsidianIdentifier = 0x6f70;
        public static byte[] PokemonObsidianIdentifierBytes => BitConverter.GetBytes(pokemonObsidianIdentifier);

        public const uint yesCode = 0x706b6361;
        public static byte[] YesCodeBytes => BitConverter.GetBytes(yesCode);

        public const uint noCode = 0x6e686170;
        public static byte[] NoCodeBytes => BitConverter.GetBytes(noCode);

        public const int mainPort = 59004;
        public const int discoveryPort = 59005;

        public const SocketType socketType = SocketType.Stream;
        public const ProtocolType protocolType = ProtocolType.Tcp;

        public const int defaultTimeout = 5000;

        #region Connection Purpose

        public enum ConnectionPurpose
        {
            Battle,
            Trade
        };

        public static ushort GetConnectionPurposeCode(ConnectionPurpose p)
            => (ushort)p;

        public static byte[] GetConnectionPurposeBytes(ConnectionPurpose p)
            => BitConverter.GetBytes(GetConnectionPurposeCode(p));

        #endregion

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

        public static string GetSocketExceptionMessage(SocketError err)
            => err switch
            {
                SocketError.TimedOut => "Connection timed out",
                SocketError.NetworkUnreachable => "Unable to connect to network",
                SocketError.ConnectionReset => "Connection reset",
                SocketError.ConnectionRefused => "Connection refused",
                SocketError.AddressAlreadyInUse => "This device's address already in use",
                SocketError.AddressNotAvailable => "Unable to use device's address",
                SocketError.AccessDenied => "Access denied",
                SocketError.HostUnreachable => "Host unreachable",
                SocketError.HostNotFound => "Host not found",
                SocketError.HostDown => "Host down",
                _ => null
            };

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

        #endregion

        #region Connection Establishment

        /// <param name="port">The port to use. Negative means to use the default port</param>
        public static bool TryConnectToServer(out Socket socket,
            LogCallback errCallback,
            LogCallback statusCallback,
            string ipAddress,
            int port,
            int timeout = defaultTimeout)
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

            return TryConnectToServer(socket: out socket,
                errCallback: errCallback,
                statusCallback: statusCallback,
                ipAddress: address,
                port: port,
                timeout: timeout);

        }

        /// <param name="port">The port to use. Negative means to use the default port</param>
        public static bool TryConnectToServer(out Socket socket,
            LogCallback errCallback,
            LogCallback statusCallback,
            IPAddress ipAddress,
            int port = -1,
            int timeout = defaultTimeout)
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
                socket.SendTimeout = defaultTimeout;
                LogNetworkEvent("Set socket receive timeout");
            }

            try
            {

                statusCallback("Connecting to server...");

                LogNetworkEvent("Beginning connection...");
                IAsyncResult res = socket.BeginConnect(endPoint, null, null);

                bool x = res.AsyncWaitHandle.WaitOne(timeout, true);
                
                if (socket.Connected)
                {
                    LogNetworkEvent("Connected");
                    socket.EndConnect(res);
                }
                else
                {
                    socket.Close();
                    errCallback("Connection timed out");
                    LogNetworkEvent("Connection timed out");
                    return false;
                }

                LogNetworkEvent("Connected to server");
                statusCallback("Connected to server");

            }
            catch (SocketException e)
            {
                socket.Close();
                string msg = GetSocketExceptionMessage(e.SocketErrorCode);
                errCallback(msg ?? e.Message);
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

        private static Socket asyncServerListenerSocket = null;

        private static void StartAsyncServerListening(IPEndPoint ep,
            LogCallback errCallback,
            LogCallback statusCallback)
        {

            awaitingClient = true;

            asyncServerListenerSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            Task asyncServerTask = new Task(() => AsyncServerListeningTask(asyncServerListenerSocket, ep));

            asyncServerTask.Start();

        }

        public static bool TryStopAsyncServerListening()
        {

            if (asyncServerListenerSocket != null)
            {
                asyncServerListenerSocket.Close();
                return true;
            }
            else
                return false;

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

        private static void AsyncServerListeningTask(Socket listenerSock, IPEndPoint ep)
        {

            listenerSock.Bind(ep);
            listenerSock.Listen(1);

            IAsyncResult res = listenerSock.BeginAccept(null, null);

            res.AsyncWaitHandle.WaitOne();

            Socket cliSock = null;

            try
            {
                cliSock = listenerSock.EndAccept(res);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            SetAsyncNewClient(cliSock);

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
            ConnectionPurpose purpose,
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

                //Server sends connection purpose
                ushort clientPurposeCode = GetConnectionPurposeCode(purpose);
                buffer = new byte[2];
                LogNetworkEvent("Receiving purpose bytes");
                stream.Read(buffer, 0, 2);
                LogNetworkEvent("Received purpose bytes");
                ushort serverPurposeCode = BitConverter.ToUInt16(buffer, 0);

                if (serverPurposeCode != clientPurposeCode)
                {
                    statusCallback("Different connection purposes");
                    errCallback("Connection purpose mismatch");
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
            ConnectionPurpose purpose,
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

                //Try receive yes response
                if (TryReceiveResponseYN(stream, out _) != true)
                {
                    statusCallback("Mismatching game versions");
                    errCallback("Failed to receive versions yes response");
                    return false;
                }

                //Server sends connection purpose
                buffer = GetConnectionPurposeBytes(purpose);
                statusCallback("Sending connection purpose...");
                stream.Write(buffer, 0, 2);
                statusCallback("Sent connection purpose...");

                //Try receive yes response
                if (TryReceiveResponseYN(stream, out _) != true)
                {
                    statusCallback("Different connection purposes");
                    errCallback("Failed to receive connection purpose yes response");
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
            out byte? standarisedLevel,
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
                    standarisedLevel = default;
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
                    standarisedLevel = default;
                    return false;
                }

                switch (TryReceiveResponseYN(stream, out _))
                {

                    case false:
                    case null:
                        errCallback("Didn't receive yes after sending battle entrance arguments");
                        randomSeed = default;
                        standarisedLevel = default;
                        return false;

                    case true:
                        break;

                }

                LogNetworkEvent("Receiving random seed...");
                byte[] randomSeedBuffer = new byte[4];
                stream.Read(randomSeedBuffer, 0, 4);
                randomSeed = BitConverter.ToInt32(randomSeedBuffer, 0);
                LogNetworkEvent($"Received random seed {randomSeed}");

                LogNetworkEvent("Receiving standarised level...");
                if (serializer.DeserializeBool(stream))
                {
                    standarisedLevel = Convert.ToByte(stream.ReadByte());
                }
                else
                    standarisedLevel = null;
                LogNetworkEvent($"Received standarised level {standarisedLevel}");

                return true;

            }
            catch (IOException)
            {
                errCallback("Connection lost");
                name = default;
                pokemon = default;
                spriteResourceName = default;
                randomSeed = default;
                standarisedLevel = default;
                return false;
            }

        }

        public static bool TryExchangeBattleEntranceArguments_Server(NetworkStream stream,
            LogCallback errCallback,
            LogCallback statusCallback,
            out string name,
            out PokemonInstance[] pokemon,
            out string spriteResourceName,
            byte? standarisedLevel,
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

                LogNetworkEvent($"Sending standarised level ({standarisedLevel})...");
                if (standarisedLevel != null)
                {
                    serializer.SerializeBool(stream, true);
                    stream.Write(new byte[1] { ((byte)standarisedLevel) }, 0, 1);
                }
                else
                    serializer.SerializeBool(stream, false);
                LogNetworkEvent($"Sent standarised level");

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

        #region Exchange Trade Entrance Arguments

        private static bool TrySendTradeEntranceArguments(NetworkStream stream,
            LogCallback errCallback,
            LogCallback statusCallback,
            Serializer serializer,
            PlayerData player = null)
        {

            try
            {

                LogNetworkEvent("Starting to send trade arguments");

                LogNetworkEvent($"Sending name ({player.profile.name})...");
                serializer.SerializeString(stream, player.profile.name);
                LogNetworkEvent("Sent name");

                LogNetworkEvent("Starting to send trade-received pokemon...");
                serializer.SerializePlayerTradeReceivedPokemonGuids(stream, player.TradeReceivedPokemonArray);
                LogNetworkEvent("Sent trade-received pokemon pokemon");

                LogNetworkEvent("Trade entrance arguments sent");

                return true;

            }
            catch (IOException e)
            {
                errCallback("IOException\n-->Message: \"" + e.Message + "\"\n-->Stack Trace:\n" + e.StackTrace);
                return false;
            }

        }

        private static bool TryReceiveTradeEntranceArguments(NetworkStream stream,
            LogCallback errCallback,
            LogCallback statusCallback,
            Serializer serializer,
            out string name,
            out Guid[] tradeReceivedPokemonGuids)
        {

            try
            {

                LogNetworkEvent("Starting to receive trade entrance arguments");

                LogNetworkEvent("Receiving name...");
                name = serializer.DeserializeString(stream);
                LogNetworkEvent($"Received name ({name})");

                LogNetworkEvent("Receiving trade-received pokemon...");
                serializer.DeserializePlayerTradeReceivedPokemonGuids(stream, out tradeReceivedPokemonGuids);
                LogNetworkEvent("Received trade-received pokemon");

                return true;

            }
            catch (IOException e)
            {
                errCallback("IOException\n-->Message: \"" + e.Message + "\"\n-->Stack Trace:\n" + e.StackTrace);
                name = default;
                tradeReceivedPokemonGuids = default;
                return false;
            }

        }

        public static bool TryExchangeTradeEntranceArguments_Client(NetworkStream stream,
            LogCallback errCallback,
            LogCallback statusCallback,
            out string name,
            out Guid[] tradeReceivedPokemonGuids,
            PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            Serializer serializer = Serialize.DefaultSerializer;

            try
            {

                if (!TryReceiveTradeEntranceArguments(stream,
                    errCallback,
                    statusCallback,
                    serializer,
                    out name,
                    out tradeReceivedPokemonGuids))
                {

                    errCallback("Failed to receive trade entrance arguments");
                    SendNo(stream);
                    return false;

                }

                SendYes(stream);

                if (!TrySendTradeEntranceArguments(stream,
                    errCallback,
                    statusCallback,
                    serializer,
                    player))
                {
                    errCallback("Failed to send trade arguments");
                    return false;
                }

                switch (TryReceiveResponseYN(stream, out _))
                {

                    case false:
                    case null:
                        errCallback("Didn't receive yes after sending trade entrance arguments");
                        return false;

                    case true:
                        break;

                }

                return true;

            }
            catch (IOException)
            {
                errCallback("Connection lost");
                name = default;
                tradeReceivedPokemonGuids = default;
                return false;
            }

        }

        public static bool TryExchangeTradeEntranceArguments_Server(NetworkStream stream,
            LogCallback errCallback,
            LogCallback statusCallback,
            out string name,
            out Guid[] tradeReceivedPokemonGuids,
            PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            Serializer serializer = Serialize.DefaultSerializer;

            try
            {

                if (!TrySendTradeEntranceArguments(stream,
                    errCallback,
                    statusCallback,
                    serializer,
                    player))
                {
                    errCallback("Failed to send trade arguments");
                    name = default;
                    tradeReceivedPokemonGuids = default;
                    return false;
                }

                switch (TryReceiveResponseYN(stream, out _))
                {

                    case false:
                    case null:
                        errCallback("Didn't receive yes after sending trade entrance arguments");
                        name = default;
                        tradeReceivedPokemonGuids = default;
                        return false;

                    case true:
                        break;

                }

                if (!TryReceiveTradeEntranceArguments(stream,
                    errCallback,
                    statusCallback,
                    serializer,
                    out name,
                    out tradeReceivedPokemonGuids))
                {

                    errCallback("Failed to receive trade entrance arguments");
                    SendNo(stream);
                    return false;

                }

                SendYes(stream);

                return true;

            }
            catch (IOException)
            {
                errCallback("Connection lost");
                name = default;
                tradeReceivedPokemonGuids = default;
                return false;
            }

        }

        #endregion

    }
}
