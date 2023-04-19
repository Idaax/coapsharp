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
using Idaax.CoAP.Message;
using Idaax.CoAP.Helpers;
using Idaax.CoAP.Exceptions;

namespace Idaax.CoAP.Message
{
    /// <summary>
    /// Holds the message ID passed around in a CoAP message
    /// </summary>
    public class CoAPMessageID : IParsable
    {
        #region Properties
        /// <summary>
        /// Accessor/Mutator for the CoAP message ID
        /// </summary>
        public UInt16 Value { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public CoAPMessageID() { }
        /// <summary>
        /// Overloaded constructor
        /// </summary>
        /// <param name="mId">The message Id</param>
        public CoAPMessageID(UInt16 mId) { this.Value = mId; }
        #endregion

        #region Operations
        /// <summary>
        /// Parse the CoAP message stream and extract message Id (in network byte order)
        /// </summary>
        /// <param name="coapMsgStream">The CoAP message stream that contains the token length and value</param>
        /// <param name="startIndex">The index to start looking for the value</param>
        /// <param name="extraInfo">Not used</param>
        /// <returns>The next index from where next set of information can be extracted from the message stream</returns>
        public int Parse(byte[] coapMsgStream, int startIndex, UInt16 extraInfo)
        {
            if (coapMsgStream == null || coapMsgStream.Length == 0 || startIndex < 0) return startIndex;//do nothing 
            if (coapMsgStream.Length < AbstractCoAPMessage.HEADER_LENGTH) throw new CoAPFormatException("Invalid CoAP message stream");
            if (startIndex >= coapMsgStream.Length) throw new ArgumentException("Start index beyond message stream length");

            //We read two bytes...
            byte[] mid = {coapMsgStream[startIndex] ,coapMsgStream[startIndex + 1]};
            //We received them in network byte order...fix the order based on the platform
            mid = AbstractNetworkUtils.FromNetworkByteOrder(mid);
            Value = AbstractByteUtils.ToUInt16(mid);
            return (startIndex + 2);
        }
        /// <summary>
        /// Convert this object into a byte stream in network byte order
        /// </summary>
        /// <param name="reserved">Not used now</param>
        /// <returns>byte array</returns>
        public byte[] ToStream(UInt16 reserved)
        {
            byte[] mID = AbstractByteUtils.GetBytes( this.Value );
            mID = AbstractNetworkUtils.ToNetworkByteOrder(mID);
            return mID;
        }
        /// <summary>
        /// Message ID is 2 bytes...check if each of the bytes is valid (always returns true)
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>bool</returns>
        public bool IsValid(UInt16 value)
        {
            return true;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Convert to a string representation
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "ID = " + this.Value.ToString();
        }
        #endregion
    }
}
