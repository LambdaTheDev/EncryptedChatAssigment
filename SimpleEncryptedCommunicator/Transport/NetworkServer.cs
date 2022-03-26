using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LambdaTheDev.SimpleEncryptedCommunicator.Transport
{
    public class NetworkServer
    {
        public readonly ConcurrentQueue<NetworkPacket> IncomingPackets = new();

        private readonly ConcurrentDictionary<int, NetworkConnection> _connections = new();
        private readonly TcpListener _server;

        private Thread _clientListeningThread;
        private volatile bool _enabled;
        

        public NetworkServer(IPEndPoint endPoint)
        {
            _server = new TcpListener(endPoint);
            _enabled = false;
        }

        public void StartThreaded()
        {
            _clientListeningThread = new Thread(() =>
            {
                _server.Start();
                
                while (_enabled)
                {
                    TcpClient incomingClient = _server.AcceptTcpClient();
                    HandleIncomingConnection(incomingClient);
                }
            });

            _enabled = true;
            _clientListeningThread.IsBackground = true;
            _clientListeningThread.Priority = ThreadPriority.BelowNormal;
            _clientListeningThread.Start();
        }

        public void Stop()
        {
            _enabled = false;
        }

        private async Task HandleIncomingConnection(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            byte[] incomingBuffer = new byte[ApplicationConsts.Mtu];
            byte[] messageBuffer = new byte[1024];
            UTF8Encoding encoding = new UTF8Encoding();

            while (_enabled)
            {
                int incomingPacketLength;
                
                try
                {
                    incomingPacketLength = await stream.ReadAsync(incomingBuffer, 0, incomingBuffer.Length);
                }
                catch { break; }

                if (incomingPacketLength == 0)
                    break;

                bool clientExists = _connections.TryGetValue(client.GetHashCode(), out NetworkConnection connection);
                if (clientExists)
                {
                    NetworkPacket packet = NetworkPacket.Rent();
                    packet.Buffer = new ArraySegment<byte>(incomingBuffer, 1, incomingPacketLength);
                    packet.Connection = connection;

                    IncomingPackets.Enqueue(packet);
                }
                else
                {
                    string nickname = encoding.GetString(incomingBuffer, 1, incomingPacketLength);
                    connection = new NetworkConnection(0, stream, nickname);
                    _connections.TryAdd(client.GetHashCode(), connection);

                    string msg = " has joined a chat!";
                    int offset = encoding.GetBytes(nickname, 0, nickname.Length, messageBuffer, 0);
                    offset += encoding.GetBytes(msg, 0, msg.Length, messageBuffer, offset);

                    Broadcast(new ArraySegment<byte>(messageBuffer, 0, offset));
                }
            }
        }

        public async Task Broadcast(ArraySegment<byte> data)
        {
            foreach (var conn in _connections)
            {
                await conn.Value.Stream.WriteAsync(data.Array!, data.Offset, data.Count);
            }
        }
    }
}