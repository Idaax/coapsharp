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
using Idaax.CoAP.Exceptions;

namespace Idaax.CoAP.Message
{
    /// <summary>
    /// Represents the possible message types a CoAP message might represent
    /// </summary>
    public class CoAPMessageType : IParsable
    {
        #region Constants
        /// <summary>
        /// Represents a Confirmable message type
        /// </summary>
        public const byte CON = 0;
        /// <summary>
        /// Represents a non-confirmable message type
        /// </summary>
        public const byte NON = 1;
        /// <summary>
        /// Represents an acknowledment message type
        /// </summary>
        public const byte ACK = 2;
        /// <summary>
        /// Represents a reset message type
        /// </summary>
        public const byte RST = 3;
        #endregion

        #region Implementation
        /// <summary>
        /// Holds what message type this class represents
        /// </summary>
        protected byte _messageType = CoAPMessageType.NON;
        #endregion

        #region Properties
        /// <summary>
        /// Accessor/Mutator for the message type
        /// </summary>
        public byte Value 
        { 
            get { return _messageType; }
            set
            {
                if (!this.IsValid(value)) throw new CoAPFormatException("Invalid message type value");
                _messageType = value;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public CoAPMessageType() { }
        /// <summary>
        /// Overloaded constructor
        /// </summary>
        /// <param name="mType">The message type</param>
        public CoAPMessageType(byte mType)
        {
            if (!this.IsValid(mType)) throw new ArgumentException("Invalid message type");
            this._messageType = mType;
        }
        #endregion

        #region Operations
        /// <summary>
        /// Parse the given CoAP message stream, starting from the location and look for the message type
        /// </summary>
        /// <param name="coapMsgStream">The CoAP message stream that contains the message type value</param>
        /// <param name="startIndex">The index to start looking for the value</param>
        /// <param name="extraInfo">Not used</param>
        /// <returns>The next index from where next set of information can be extracted from the message stream</returns>
        public int Parse(byte[] coapMsgStream, int startIndex, UInt16 extraInfo)
        {
            if (coapMsgStream == null || coapMsgStream.Length == 0 || startIndex < 0) return startIndex;//do nothing 
            if (coapMsgStream.Length < AbstractCoAPMessage.HEADER_LENGTH) throw new CoAPFormatException("Invalid CoAP message stream");
            if (startIndex >= coapMsgStream.Length) throw new ArgumentException("Start index beyond message stream length");

            //bits 2-3 (starting from left)
            byte mt = (byte)((coapMsgStream[startIndex] & 0x30) >> 4);
            this.Value = mt;
            return startIndex; //we have not moved from where we started....
        }
        /// <summary>
        /// Convert this object into a stream in network byte order
        /// </summary>
        /// <param name="reserved">Not used now</param>
        /// <returns>byte array</returns>
        public byte[] ToStream(UInt16 reserved)
        {
            byte[] mType = new byte[1] {this.Value};
            mType[0] = (byte)(mType[0] << 4);
            return mType;
        }
        /// <summary>
        /// Check if the message type is valid or not
        /// </summary>
        /// <param name="msgType">The message type value to check</param>
        /// <returns>bool</returns>
        public bool IsValid(UInt16 msgType)
        {
            return (msgType == CoAPMessageType.ACK ||
                    msgType == CoAPMessageType.CON ||
                    msgType == CoAPMessageType.NON ||
                    msgType == CoAPMessageType.RST);

        }
        #endregion        

        #region Overrides
        /// <summary>
        /// Convert to a string representation
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            switch(this.Value)
            {
                case CoAPMessageType.ACK: return "Type: ACK";
                case CoAPMessageType.CON: return "Type: CON";
                case CoAPMessageType.NON: return "Type: NON";
                case CoAPMessageType.RST: return "Type: RST";
                default: return "Type: Unknown";
            }  
        }
        #endregion
    }
}
