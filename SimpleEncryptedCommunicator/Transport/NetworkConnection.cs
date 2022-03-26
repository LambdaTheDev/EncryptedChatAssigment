using System;
using System.Net.Sockets;

namespace LambdaTheDev.SimpleEncryptedCommunicator.Transport
{
    public class NetworkConnection
    {
        public int Id { get; }
        public NetworkStream Stream { get; }
        public string Nickname { get; }

        
        public NetworkConnection(int id, NetworkStream stream, string nick)
        {
            Id = id;
            Stream = stream;
            Nickname = nick;
        }

        public void Send(ArraySegment<byte> data)
        {
            Stream.Write(data.Array!, data.Offset, data.Count);
        }
    }
}