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
using System.Text;

namespace Idaax.CoAP.Message
{
    /// <summary>
    /// An object within this library, that is formed by parsing the CoAP message stream should
    /// implement this.
    /// </summary>
    public interface IParsable
    {
        #region Operations
        /// <summary>
        /// Class that represents a chunk in the CoAP message stream and that is formed by parsing
        /// the message stream, should implement this
        /// </summary>
        /// <param name="coapMsgStream">The CoAP message stream that is to be parsed</param>
        /// <param name="startIndex">The index from where to begin parsing</param>
        /// <param name="extraInfo">Any additional information</param>
        /// <returns>The next position in the stream from where others should start looking for other values</returns>
        int Parse(byte[] coapMsgStream, int startIndex, UInt16 extraInfo);
        /// <summary>
        /// Convert the object to a stream of bytes
        /// </summary>
        /// <param name="extraInfo">unsigned int 16 bits to pass any additional information for stream conversion</param>
        /// <returns>byte stream</returns>
        byte[] ToStream(UInt16 extraInfo);
        /// <summary>
        /// Check if the value is valid or not
        /// </summary>
        /// <param name="value">The value to check for validity</param>
        /// <returns>boolean</returns>
        bool IsValid(UInt16 value);
        #endregion
    }
}
