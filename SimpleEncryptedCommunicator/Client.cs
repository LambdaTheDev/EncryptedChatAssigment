using System;
using System.Net;
using System.Text;
using LambdaTheDev.SimpleEncryptedCommunicator.Encryption;
using LambdaTheDev.SimpleEncryptedCommunicator.Transport;

namespace LambdaTheDev.SimpleEncryptedCommunicator
{
    public class Client
    {
        private readonly char[] _input = new char[ApplicationConsts.MaxMessageLength];
        private readonly char[] _output = new char[ApplicationConsts.MaxMessageLength];


        private readonly IEncryptionAlgorithm _algorithm;
        private readonly NetworkClient _client;
        private readonly string _nickname;


        public Client(IPAddress address, ushort port, string nickname, IEncryptionAlgorithm algo)
        {
            _algorithm = algo;
            _client = new NetworkClient(new IPEndPoint(address, port), nickname);
            _nickname = nickname;
        }

        public void Start()
        {
            int i = _nickname.Length + 2;
            for (int j = 0; j < _nickname.Length; j++)
                _input[j] = _nickname[j];

            _input[_nickname.Length] = ' ';
            _input[_nickname.Length + 1] = ':';

            _client.StartThreaded();
            
            byte[] buffer = new byte[ApplicationConsts.Mtu];
            UTF8Encoding encoding = new UTF8Encoding();
            
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo info = Console.ReadKey();
                    if (info.Key == ConsoleKey.Enter)
                    {
                        ArraySegment<char> encrypted = _algorithm.Encrypt(new ArraySegment<char>(_input, 0, i));

                        encoding.GetBytes(_input, 0, i, buffer, 0);
                        _client.Send(new ArraySegment<byte>(buffer, 0, i));
                        
                        i = _nickname.Length + 2;
                    }
                    else
                    {
                        _input[i++] = info.KeyChar;
                    }
                }

                while (_client.IncomingPackets.TryPeek(out NetworkPacket packet))
                {
                    int chars = encoding.GetChars(packet.Buffer.Array!, packet.Buffer.Offset, packet.Buffer.Count,
                        _output, _nickname.Length + 2);

                    ArraySegment<char> decrypted = _algorithm.Decrypt(new ArraySegment<char>(_output, 0, chars));
                    Console.WriteLine(new string(decrypted));
                }
            }
        }
    }
}