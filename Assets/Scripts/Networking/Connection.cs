using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Serialization;
using Pokemon;

namespace Networking
{
    public static class Connection
    {

        public const ushort pokemonObsidianIdentifier = 0x6f70;
        public static byte[] PokemonObsidianIdentifierBytes => BitConverter.GetBytes(pokemonObsidianIdentifier);

        public const uint acknowledgementCode = 0x706b6361;
        public static byte[] AcknowledgementCodeBytes => BitConverter.GetBytes(acknowledgementCode);

        public const uint failedToReceiveCode = 0x6e686170;
        public static byte[] FailedToReceiveCodeBytes => BitConverter.GetBytes(failedToReceiveCode);

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

        private static void LogNetworkEvent(string msg)
        {
            if (GameSettings.singleton.networkLogEnabled)
                Debug.Log(msg);
        }

        private static void LogNetworkEventSocketIsConnected(Socket socket)
        {
            LogNetworkEvent("Socket connected - " + (socket.Connected ? "yes" : "no"));
        }

        #region Standard Codes

        private static void SendAck(NetworkStream stream)
        {
            stream.Write(AcknowledgementCodeBytes, 0, 4);
        }

        private static bool TryReceiveAck(NetworkStream stream,
            out uint recvAck)
        {

            byte[] buffer = new byte[4];

            stream.Read(buffer, 0, 4);

            recvAck = BitConverter.ToUInt32(buffer, 0);

            return recvAck == acknowledgementCode;

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
            out string errMsg,
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
                errMsg = "IP address provided is null";
                return false;
            }
            catch (FormatException)
            {
                socket = default;
                errMsg = "Invalid IP address format";
                return false;
            }

            return TryConnectToServer(out socket,
                out errMsg,
                address,
                port);

        }

        /// <param name="port">The port to use. Negative means to use the default port</param>
        public static bool TryConnectToServer(out Socket socket,
            out string errMsg,
            IPAddress ipAddress,
            int port)
        {

            if (port < 0)
                port = mainPort;

            if (port < IPEndPoint.MinPort)
            {
                socket = default;
                errMsg = "Port number provided too small";
                return false;
            }

            if (port > IPEndPoint.MaxPort)
            {
                socket = default;
                errMsg = "Port number provided too great";
                return false;
            }

            socket = new Socket(socketType, protocolType);
            LogNetworkEvent($"Socket created (local endpoint - {socket.LocalEndPoint})");

            IPEndPoint endPoint = new IPEndPoint(ipAddress, port);
            LogNetworkEvent($"Socket end point set to {ipAddress}:{port}");

            if (!GameSettings.singleton.networkTimeoutDisabled)
            {
                socket.ReceiveTimeout = defaultTimeout;
                LogNetworkEvent("Set socket receive timeout");
            }

            try
            {

                if (!GameSettings.singleton.networkTimeoutDisabled)
                {
                    IAsyncResult connectResult = socket.BeginConnect(endPoint, null, null);
                    LogNetworkEvent("Started connecting socket");
                    if (connectResult.AsyncWaitHandle.WaitOne(defaultTimeout, true))
                    {
                        LogNetworkEvent("Socket connected");
                        socket.EndConnect(connectResult);
                    }
                    else
                    {
                        socket.Close();
                        errMsg = "Connection timed out";
                        LogNetworkEvent("Connection timed out");
                        return false;
                    }
                }
                else
                {
                    LogNetworkEvent("Connecting socket...");
                    socket.Connect(endPoint);
                    LogNetworkEvent("Socket connected");
                }

            }
            catch (SocketException e)
            {
                socket.Close();
                errMsg = "Socket Error: " + e.Message;
                LogNetworkEvent("Socket Error Message: \"" + e.Message + "\". Error Stack: " + e.StackTrace);
                return false;
            }
            catch (ObjectDisposedException)
            {
                socket.Close();
                errMsg = "Socket closed unexpectedly";
                return false;
            }
            catch (System.Security.SecurityException)
            {
                socket.Close();
                errMsg = "Invalid permissions";
                return false;
            }
            catch (InvalidOperationException)
            {
                socket.Close();
                errMsg = "Socket unexpectedly in listening mode";
                return false;
            }

            errMsg = default;
            return true;

        }

        public delegate void StartedListening();
        public delegate void ClientConnected(bool success, string errMsg, Socket socket);

        /// <param name="port">The port to use. Negative means to use the default port</param>
        public static void TryStartHostServer(StartedListening startedListeningListener,
            ClientConnected clientConnectedListener,
            int port = -1)
        {

            if (port < 0)
                port = mainPort;

            if (port < IPEndPoint.MinPort)
            {
                clientConnectedListener?.Invoke(false, "Port number provided too small", default);
            }

            if (port > IPEndPoint.MaxPort)
            {
                clientConnectedListener?.Invoke(false, "Port number provided too great", default);
            }

            Socket socket = new Socket(socketType, protocolType);
            LogNetworkEvent("Socket created");

            IPEndPoint endPoint = new IPEndPoint(GetHostIPAddress(), port);

            socket.Bind(endPoint);
            LogNetworkEvent($"Socket bound to {endPoint.Address}:{port}");

            socket.Listen(1);
            LogNetworkEvent("Socket set to listen");

            startedListeningListener?.Invoke();

            Socket cliSocket;

            LogNetworkEvent("Awaiting connection");

            try
            {
                cliSocket = socket.Accept();
            }
            catch (SocketException e)
            {
                clientConnectedListener?.Invoke(false, "Socket Error: " + e.Message, default);
                return;
            }
            catch (ObjectDisposedException)
            {
                clientConnectedListener?.Invoke(false, "Socket closed unexpectedly", default);
                return;
            }

            LogNetworkEvent("Connection accepted");

            clientConnectedListener?.Invoke(true, "", cliSocket);

        }

        #endregion

        #region Network Stream

        public static NetworkStream CreateNetworkStream(Socket socket)
        {

            //Having the network stream own the socket should make the socket close when the network stream is closed
            NetworkStream stream = new NetworkStream(socket, true);
            stream.ReadTimeout = GameSettings.singleton.networkTimeoutDisabled ? -1 : defaultTimeout;
            return stream;

        }

        #endregion

        #region Connection Verification

        public static bool VerifyConnection_Client(NetworkStream stream)
        {

            LogNetworkEvent("Starting client connection verification");

            try
            {

                byte[] buffer;

                //Server sends Pokemon Obsidian Identifier

                LogNetworkEvent("Receiving identifier...");

                if (!TryReceivePokemonObsidianIdentifier(stream, out ushort recvPmonId))
                {
                    LogNetworkEvent("Identifier mismatch");
                    Debug.LogWarning("Pokemon Obsidian Identifier mismatch. Received - " + recvPmonId.ToString() + ", expected - " + pokemonObsidianIdentifier.ToString());
                    return false;
                }
                else
                {
                    LogNetworkEvent("Identifier matches");
                }

                //Client sends acknowledgement
                SendAck(stream);
                LogNetworkEvent("Sent ack");

                //Server sends game version
                buffer = new byte[2];
                LogNetworkEvent("Receiving game version");
                stream.Read(buffer, 0, 2);
                LogNetworkEvent("Received game version");
                ushort serverGameVersion = BitConverter.ToUInt16(buffer, 0);

                if (serverGameVersion != GameVersion.version)
                {
                    LogNetworkEvent("Game version mismatch");
                    Debug.LogWarning($"Game version mismatch (server - {serverGameVersion}, client - {GameVersion.version})");
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
                    LogNetworkEvent("Serializer version mismatch");
                    Debug.LogWarning($"Game version mismatch (server - {serverSerializerVersion}, client - {Serialize.defaultSerializerVersion})");
                    return false;
                }

                //Client sends acknowledgement
                SendAck(stream);

                return true;

            }
            catch (IOException e) //It is assumed that the network stream timed out
            {
                Debug.LogWarning("Network stream timed out");
                LogNetworkEvent("IOException. Message: \"" + e.Message + "\" stack trace: " + e.StackTrace);
                if (e.InnerException is SocketException sockE)
                    LogNetworkEvent($"IOException's inner socket exception code: {sockE.SocketErrorCode}");
                return false;
            }

        }

        public static bool VerifyConnection_Server(NetworkStream stream)
        {

            LogNetworkEvent("Starting server connection verification");

            try
            {

                byte[] buffer;

                uint recvAck;

                //Server sends Pokemon Obsidian Identifier
                SendPokemonObsidianIdentifier(stream);
                LogNetworkEvent("Sent identifier");

                //Client sends acknowledgement

                LogNetworkEvent("Receiving ack...");

                if (!TryReceiveAck(stream, out recvAck))
                {
                    LogNetworkEvent("Invalid ack");
                    Debug.LogWarning("Incorrect acknowledgement code received. Received - " + recvAck.ToString());
                    return false;
                }
                else
                {
                    LogNetworkEvent("Successfully received ack");
                }

                //Server sends game version
                buffer = GameVersion.VersionBytes;
                LogNetworkEvent("Sending game version...");
                stream.Write(buffer, 0, 2);
                LogNetworkEvent("Sent game version");

                //Server sends serializer version
                buffer = BitConverter.GetBytes(Serialize.defaultSerializerVersion);
                LogNetworkEvent("Sending serializer version...");
                stream.Write(buffer, 0, 2);
                LogNetworkEvent("Sent serializer version");

                //Client sends acknowledgement
                
                LogNetworkEvent("Receiving ack...");

                if (!TryReceiveAck(stream, out recvAck))
                {
                    LogNetworkEvent("Invalid ack");
                    Debug.LogWarning("Incorrect acknowledgement code received. Received - " + recvAck.ToString());
                    return false;
                }
                else
                {
                    LogNetworkEvent("Successfully received ack");
                }

                return true;

            }
            catch (IOException e)
            {
                Debug.LogWarning("Connection lost");
                LogNetworkEvent("IOException. Message: \"" + e.Message + "\" stack trace: " + e.StackTrace);
                if (e.InnerException is SocketException sockE)
                    LogNetworkEvent($"IOException's inner socket exception code: {sockE.SocketErrorCode}");
                return false;
            }

        }

        #endregion

        #region Exchange Battle Entrance Arguments

        private static void SendBattleEntranceArguments(NetworkStream stream,
            Serializer serializer,
            PlayerData player = null)
        {

            LogNetworkEvent("Starting send battle arguments");

            if (player == null)
                player = PlayerData.singleton;

            if (serializer == null)
                serializer = Serialize.DefaultSerializer;

            LogNetworkEvent("Sending name...");
            serializer.SerializeString(stream, player.profile.name);
            LogNetworkEvent("Sent name");

            LogNetworkEvent("Starting to send party");

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
                    LogNetworkEvent($"Pmon EVs: {pokemon.effortValues.attack},{pokemon.effortValues.defense},{pokemon.effortValues.specialAttack},{pokemon.effortValues.specialDefense},{pokemon.effortValues.speed},{pokemon.effortValues.health}"); //TODO - remove once done debugging
                    LogNetworkEvent($"Pmon IVs: {pokemon.individualValues.attack},{pokemon.individualValues.defense},{pokemon.individualValues.specialAttack},{pokemon.individualValues.specialDefense},{pokemon.individualValues.speed},{pokemon.individualValues.health}"); //TODO - remove once done debugging
                    serializer.SerializePokemonInstance(stream, pokemon);
                    LogNetworkEvent($"Sent pokemon  (Hash - {string.Format("{0:X}", pokemon.Hash)})");
                }

            }

            LogNetworkEvent("Party sent");

            LogNetworkEvent("Sending sprite name");
            serializer.SerializeString(stream, player.profile.SpriteName);
            LogNetworkEvent("Sent sprite name");

        }

        private static bool TryReceiveBattleEntranceArguments(NetworkStream stream,
            Serializer serializer,
            out string name,
            out PokemonInstance[] pokemon,
            out string spriteResourceName)
        {

            LogNetworkEvent("Starting receive battle arguments");

            try
            {

                LogNetworkEvent("Receiving name...");
                name = serializer.DeserializeString(stream);
                LogNetworkEvent("Received name");

                LogNetworkEvent("Starting to receive party");

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
                Debug.LogWarning("Connection error");
                LogNetworkEvent("IOException. Message: \"" + e.Message + "\" stack trace: " + e.StackTrace);
                name = default;
                pokemon = default;
                spriteResourceName = default;
                return false;
            }

        }

        public static bool TryExchangeBattleEntranceArguments_Client(NetworkStream stream,
            out string name,
            out PokemonInstance[] pokemon,
            out string spriteResourceName,
            PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            Serializer serializer = Serialize.DefaultSerializer;

            try
            {

                //Server sends battle entrance arguments
                if (!TryReceiveBattleEntranceArguments(stream,
                    serializer,
                    out name,
                    out pokemon,
                    out spriteResourceName))
                {

                    Debug.LogWarning("Failed to receive battle entrance arguments");
                    return false;

                }

                //Client sends acknowledgement
                SendAck(stream);

                //Client sends battle entrance arguments
                SendBattleEntranceArguments(stream, serializer, player);

                //Server sends acknowledgement

                if (!TryReceiveAck(stream, out uint recvAck))
                {
                    Debug.LogWarning($"Incorrect acknowledgement code received. Recevied - " + recvAck.ToString());
                    return false;
                }

                return true;

            }
            catch (IOException)
            {
                Debug.LogWarning("Connection lost");
                name = default;
                pokemon = default;
                spriteResourceName = default;
                return false;
            }

        }

        public static bool TryExchangeBattleEntranceArguments_Server(NetworkStream stream,
            out string name,
            out PokemonInstance[] pokemon,
            out string spriteResourceName,
            PlayerData player = null)
        {

            if (player == null)
                player = PlayerData.singleton;

            Serializer serializer = Serialize.DefaultSerializer;

            try
            {

                //Server sends battle entrance arguments
                SendBattleEntranceArguments(stream, serializer, player);

                //Client sends acknowledgement

                if (!TryReceiveAck(stream, out uint recvAck))
                {
                    Debug.LogWarning($"Incorrect acknowledgement code received. Received - " + recvAck.ToString());
                    name = default;
                    pokemon = default;
                    spriteResourceName = default;
                    return false;
                }

                //Client sends battle entrance arguments
                if (!TryReceiveBattleEntranceArguments(stream,
                    serializer,
                    out name,
                    out pokemon,
                    out spriteResourceName))
                {

                    Debug.LogWarning("Failed to receive battle entrance arguments");
                    return false;

                }

                //Server sends acknowledgement
                SendAck(stream);

                return true;

            }
            catch (IOException)
            {
                Debug.LogWarning("Connection lost");
                name = default;
                pokemon = default;
                spriteResourceName = default;
                return false;
            }

        }

        #endregion

    }
}
