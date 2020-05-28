using System;

namespace WebApp.Service.Helpers
{
    // CRC8-ITU
    // http://www.sunshine2k.de/coding/javascript/crc/crc_js.html
    public static class Crc8
    {
        private static readonly byte[] s_table = new byte[256];

        private const byte Init = 0x00;
        private const byte Poly = 0x07;
        private const byte FinalXor = 0x55;

        static Crc8()
        {
            for (var i = 0; i < 256; ++i)
            {
                var temp = i;
                for (var j = 0; j < 8; ++j)
                    if ((temp & 0x80) != 0)
                        temp = (temp << 1) ^ Poly;
                    else
                        temp <<= 1;
                s_table[i] = (byte)temp;
            }
        }

        public static byte ComputeChecksum(ReadOnlySpan<byte> bytes)
        {
            var crc = Init;
            for (int i = 0, n = bytes.Length; i < n; i++)
                crc = s_table[crc ^ bytes[i]];
            return (byte)(crc ^ FinalXor);
        }
    }
}
