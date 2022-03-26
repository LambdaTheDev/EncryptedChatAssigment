using System;

namespace LambdaTheDev.SimpleEncryptedCommunicator.Encryption
{
    public class CesarCipher : IEncryptionAlgorithm
    {
        private readonly char _key;
        
        
        public CesarCipher(int key)
        {
            _key = (char) key;
        }
        
        public ArraySegment<char> Encrypt(ArraySegment<char> input)
        {
            for (int i = input.Offset; i < input.Count; i++)
            {
                input.Array![i] += _key;
            }

            return input;
        }

        public ArraySegment<char> Decrypt(ArraySegment<char> input)
        {
            for (int i = input.Offset; i < input.Count; i++)
            {
                input.Array![i] -= _key;
            }

            return input;
        }
    }
}