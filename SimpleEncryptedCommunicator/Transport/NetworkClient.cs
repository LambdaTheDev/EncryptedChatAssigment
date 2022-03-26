using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace LambdaTheDev.SimpleEncryptedCommunicator.Transport
{
    public class NetworkClient
    {
        public readonly ConcurrentQueue<NetworkPacket> IncomingPackets = new();

        private readonly TcpClient _client;
        private readonly IPEndPoint _serverEndPoint;
        private readonly string _nickname;
        private NetworkStream _stream;

        private volatile bool _enabled;
        
        
        public NetworkClient(IPEndPoint endPoint, string nickname)
        {
            _client = new TcpClient();
            _serverEndPoint = endPoint;
            _nickname = nickname;
        }

        public void StartThreaded()
        {
            Thread listeningThread = new Thread(() =>
            {
                _client.Connect(_serverEndPoint);
                _stream = _client.GetStream();

                byte[] nick = Encoding.UTF8.GetBytes(_nickname);
                _stream.Write(nick);
                _stream.Flush();

                byte[] incomingBuffer = new byte[ApplicationConsts.Mtu];
                NetworkConnection connection = new NetworkConnection(0, _stream, _nickname);
                
                while (_enabled)
                {
                    int incomingPacketLength;
                
                    try
                    {
                        incomingPacketLength = _stream.Read(incomingBuffer, 0, incomingBuffer.Length);
                    }
                    catch { break; }

                    if (incomingPacketLength == 0)
                        break;
                    
                    NetworkPacket packet = NetworkPacket.Rent();
                    packet.Buffer = new ArraySegment<byte>(incomingBuffer, 0, incomingPacketLength);
                    packet.Connection = connection;

                    IncomingPackets.Enqueue(packet);
                }
            });

            _enabled = true;
            listeningThread.IsBackground = true;
            listeningThread.Priority = ThreadPriority.BelowNormal;
            listeningThread.Start();
        }

        public void Send(ArraySegment<byte> data)
        {
            _stream.Write(data);
        }
    }
}