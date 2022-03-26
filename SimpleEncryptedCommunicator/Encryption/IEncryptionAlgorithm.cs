using System;

namespace LambdaTheDev.SimpleEncryptedCommunicator.Encryption
{
    public interface IEncryptionAlgorithm
    {
        ArraySegment<char> Encrypt(ArraySegment<char> input);
        ArraySegment<char> Decrypt(ArraySegment<char> input);
    }
}