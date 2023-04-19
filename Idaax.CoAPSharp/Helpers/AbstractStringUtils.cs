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


namespace Idaax.CoAP.Helpers
{
    /// <summary>
    /// A utility class for string object
    /// </summary>
    public abstract class AbstractStringUtils
    {
        /// <summary>
        /// Replace a given character with another within a string
        /// </summary>
        /// <param name="input">The input string where characters have to be replaced</param>
        /// <param name="thisOne">Which character to replace</param>
        /// <param name="withThis">Which character to replace with</param>
        /// <returns>string</returns>
        public static string Replace(string input, char thisOne, char withThis)
        {
            if (input == null) return null;
            if (thisOne == withThis) return input;
            char[] chars = input.ToCharArray();
            for (int count = 0; count < chars.Length; count++)
                if (chars[count] == thisOne) chars[count] = withThis;
            return new string(chars);
        }
        /// <summary>
        /// Pad the left side of the string with the given character
        /// </summary>
        /// <param name="input">The input string to pad with</param>
        /// <param name="padWith">Which character to use to do padding</param>
        /// <param name="length">The length of the resulting string</param>
        /// <returns>string</returns>
        public static string PadLeft(string input, char padWith, int length)
        {
            if (input == null) return null;
            if (input.Length >= length) return input;
            string paddedStr = input;
            while (paddedStr.Length < length)
                paddedStr = new string(padWith, 1) + paddedStr;
            return paddedStr;
        }
    }
}
