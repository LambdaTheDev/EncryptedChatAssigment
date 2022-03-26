using System;
using System.Collections.Concurrent;

namespace LambdaTheDev.SimpleEncryptedCommunicator.Transport
{
    public class NetworkPacket : IDisposable
    {
        private static readonly ConcurrentStack<NetworkPacket> Pool = new();

        public ArraySegment<byte> Buffer;
        public NetworkConnection Connection;


        private NetworkPacket() { }

        public void Dispose()
        {
            Buffer = default;
            Connection = null;
            Pool.Push(this);
        }

        public static NetworkPacket Rent()
        {
            if (!Pool.TryPop(out NetworkPacket packet))
                packet = new NetworkPacket();

            return packet;
        }
    }
}