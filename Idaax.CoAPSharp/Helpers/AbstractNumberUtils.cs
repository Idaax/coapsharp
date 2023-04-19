/******************************************************************************
    CoAPSharp - C# Implementation of CoAP for .NET
    This library was originally written for .NET Micro framework. It is now
    migrated to nanoFramework.
    
    MIT License

    Copyright (c) [2023] [Idaax Inc., www.coapsharp.com]

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
 *****************************************************************************/
using System;
using System.Collections;

namespace Idaax.CoAP.Helpers
{
    /// <summary>
    /// A utilities class for numbers
    /// </summary>
    public abstract class AbstractNumberUtils
    {
        /// <summary>
        /// Convert a string representing a hexadecimal number to UInt32
        /// </summary>
        /// <param name="hexStr">The hex string</param>
        /// <returns>The unsigned integer representation of the hex string</returns>
        public static UInt32 Hex2UInt32(string hexStr)
        {
            if (hexStr == null) throw new ArgumentNullException("Hex string is null");
            if (hexStr.Trim().Length == 0) throw new ArgumentException("Hex string is empty");

            hexStr = (hexStr.Trim().IndexOf("0x") == 0) ? hexStr.Trim().Substring(2).ToUpper() : hexStr.Trim().ToUpper();
            if (hexStr.Length > 8) //more than 4 bytes or 8-nibbles
                throw new ArgumentException("Hex string too large for conversion");

            ArrayList allowedHexChars = new ArrayList();
            char[] hexCharArr = "0123456789ABCDEF".ToCharArray();
            foreach (char hexChar in hexCharArr)
                allowedHexChars.Add(hexChar);

            //check if the hex string contains dis-allowed characters
            char[] hexChars = AbstractStringUtils.PadLeft(hexStr, '0', 8).ToCharArray();
            foreach (char hex in hexChars)
                if (!allowedHexChars.Contains(hex)) throw new ArgumentException("Input string does not represent hexadecimal characters");

            UInt32 mul = 1;
            UInt32 result = 0;
            for (int count = hexChars.Length - 1; count >= 0; --count)
            {
                result += (UInt32)(mul * (allowedHexChars.IndexOf(hexChars[count])));
                mul = (uint)(mul * allowedHexChars.Count);
            }

            return result;
        }
        /// <summary>
        /// Convert the UInt32 value to its hex representation
        /// </summary>
        /// <param name="input">The input to convert to hex representation</param>
        /// <param name="minLen">
        /// The minimum length of the output string. 
        /// </param>
        /// <returns>string</returns>
        public static string UInt32ToHex(UInt32 input, int minLen)
        {
            return input.ToString("X" + minLen.ToString());
        }
    }
}
