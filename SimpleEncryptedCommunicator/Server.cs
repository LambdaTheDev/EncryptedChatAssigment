using System.Net;
using System.Threading;
using LambdaTheDev.SimpleEncryptedCommunicator.Transport;

namespace LambdaTheDev.SimpleEncryptedCommunicator
{
    public class Server
    {
        private readonly NetworkServer _server;
        
        public Server(ushort port)
        {
            _server = new NetworkServer(new IPEndPoint(IPAddress.Any, port));
            _server.StartThreaded();
        }

        public void Listen()
        {
            while (true)
            {
                while (_server.IncomingPackets.TryDequeue(out NetworkPacket packet))
                {
                    using (packet)
                    {
                        _server.Broadcast(packet.Buffer);
                    }
                }

                Thread.Sleep(100);
            }
        }
    }
}