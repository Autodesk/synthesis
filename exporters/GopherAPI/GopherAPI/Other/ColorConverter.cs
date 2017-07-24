using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Drawing;

namespace GopherAPI.Other
{
    internal static class GopherColors
    {
        internal static Color ParseColor(byte[] rawColor, out bool isDefault)
        {
            if (rawColor.Length != 2)
            {
                throw new ArgumentException("ERROR: Expected 2 bytes", "rawColor");
            }
            BitArray bitArr = new BitArray(rawColor);

            int Red = 0, Green = 0, Blue = 0;

            //Red
            if (bitArr[0])
                Red += 16;
            if (bitArr[1])
                Red += 8;
            if (bitArr[2])
                Red += 4;
            if (bitArr[3])
                Red += 2;
            if (bitArr[4])
                Red += 1;
            //Green
            if (bitArr[5])
                Green += 16;
            if (bitArr[6])
                Green += 8;
            if (bitArr[7])
                Green += 4;
            if (bitArr[8])
                Green += 2;
            if (bitArr[9])
                Green += 1;
            //Blue
            if (bitArr[10])
                Blue += 16;
            if (bitArr[11])
                Blue += 8;
            if (bitArr[12])
                Blue += 4;
            if (bitArr[13])
                Blue += 2;
            if (bitArr[14])
                Blue += 1;

            Red = Red * 8;
            Green = Green * 8;
            Blue = Blue * 8;

            isDefault = bitArr[15];

            return Color.FromArgb(Red, Green, Blue);
        }

        private static byte[] BitToByte(BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }
        private static bool[] IntToBits(int n)
        {
            var ret = new bool[5];

            if (n >= 32)
            {
                throw new ArgumentOutOfRangeException("n", "ERROR: Converted Color Value must be between 0 and 31");
            }
            if (n >= 16)
            {
                ret[0] = true;
                n -= 16;
            }
            if (n >= 8)
            {
                ret[1] = true;
                n -= 8;
            }
            if (n >= 4)
            {
                ret[2] = true;
                n -= 4;
            }
            if (n >= 2)
            {
                ret[3] = true;
                n -= 2;
            }
            if (n >= 1)
            {
                ret[4] = true;
                n -= 1;
            }
            return ret;
        }

        /// <summary>
        /// Converts a System.Drawing.Color into a 2-bit STL color
        /// </summary>
        /// <param name="ARGB"></param>
        /// <param name="IsDefault"></param>
        /// <returns></returns>
        internal static byte[] ConvertColor(Color ARGB, bool IsDefault)
        {
            BitArray BitArr = new BitArray(16);
            int R = ARGB.R / 8, G = ARGB.G / 8, B = ARGB.B / 8;

            var index = 0;
            foreach (var b in IntToBits(R))
            {
                BitArr.Set(index, b);
                index++;
            }
            foreach (var b in IntToBits(G))
            {
                BitArr.Set(index, b);
                index++;
            }
            foreach (var b in IntToBits(B))
            {
                BitArr.Set(index, b);
                index++;
            }
            BitArr.Set(15, IsDefault);

            return BitToByte(BitArr);
        }
    }
}
