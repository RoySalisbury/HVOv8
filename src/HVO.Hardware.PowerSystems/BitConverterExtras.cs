using System.Net.Sockets;

namespace HVO.Hardware.PowerSystems
{
    public static class BitConverterExtras
    {
        private static void HexEncodeBytes(Span<byte> inputBytes, Span<byte> hexEncodedBytes)
        {
            Span<byte> hexAlphabet = stackalloc byte[] {
                (byte)'0',
                (byte)'1',
                (byte)'2',
                (byte)'3',
                (byte)'4',
                (byte)'5',
                (byte)'6',
                (byte)'7',
                (byte)'8',
                (byte)'9',
                (byte)'A',
                (byte)'B',
                (byte)'C',
                (byte)'D',
                (byte)'E',
                (byte)'F'
            };

            for (int i = 0; i < inputBytes.Length; i++)
            {
                hexEncodedBytes[i * 2] = hexAlphabet[inputBytes[i] >> 4];
                hexEncodedBytes[i * 2 + 1] = hexAlphabet[inputBytes[i] & 0xF];
            }
        }

        private static void HexEncodeBytes(Span<byte> inputBytes, Span<char> hexEncodedChars)
        {
            Span<char> hexAlphabet = stackalloc char[] {
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9',
                'A',
                'B',
                'C',
                'D',
                'E',
                'F'
            };

            for (int i = 0; i < inputBytes.Length; i++)
            {
                hexEncodedChars[i * 2] = hexAlphabet[inputBytes[i] >> 4];
                hexEncodedChars[i * 2 + 1] = hexAlphabet[inputBytes[i] & 0xF];
            }
        }

        public static string BytesToHexString(Span<byte> inputBytes)
        {
            int finalLength = inputBytes.Length * 2;
            Span<char> encodedChars = finalLength < 2048 ? stackalloc char[finalLength] : new char[finalLength];

            HexEncodeBytes(inputBytes, encodedChars);
            return new string(encodedChars);
        }
    }
}
