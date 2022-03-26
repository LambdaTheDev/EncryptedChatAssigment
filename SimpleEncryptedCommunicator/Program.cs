using System;
using System.Net;
using LambdaTheDev.SimpleEncryptedCommunicator.Encryption;

namespace LambdaTheDev.SimpleEncryptedCommunicator
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Menu();
        }

        private static void Menu()
        {
            Console.Clear();
            Console.WriteLine("Type S to start server, and C to start client:");
            string input = Console.ReadLine();

            switch (input)
            {
                case "S":
                case "s":
                    Server server = new Server(ApplicationConsts.Port);
                    server.Listen();
                    break;

                case "C":
                case "c":
                    Console.WriteLine("IP:");
                    
                    IPAddress address = IPAddress.Loopback;
                    string ip = Console.ReadLine();
                    if(!string.IsNullOrEmpty(ip)) address = IPAddress.Parse(ip);
                    
                    Console.WriteLine("Port:");
                    ushort port;
                    string portStr = Console.ReadLine();
                    port = Convert.ToUInt16(portStr);
                    
                    Console.WriteLine("Nick:");
                    string nickname = Console.ReadLine();

                    Client client = new Client(address, port, nickname, new CesarCipher(4));
                    client.Start();

                    break;

                default:
                    Menu();
                    break;
            }
        }
    }
}