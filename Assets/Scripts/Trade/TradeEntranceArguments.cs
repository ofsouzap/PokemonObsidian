using System;
using System.Net;
using System.Net.Sockets;

namespace Trade
{
    public static class TradeEntranceArguments
    {

        public static bool argumentsSet = false;

        public static NetworkStream networkStream;

        public static string otherUserName;

        public static Guid[] disallowedSendPokemonGuids; //Guids of pokemon that the player is not allowed to send to the other user

    }
}
