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
    /// This class represents the actual payload that is associated with a CoAP message.
    /// Payload length is limited to 1KB currently
    /// </summary>
    public class CoAPPayload : IParsable
    {
        #region Properties
        /// <summary>
        /// Accessor/Mutator for the payload value
        /// </summary>
        public byte[] Value { get; set; }

        #endregion

        #region Constructors
        /// <summary>
        /// The default constructor
        /// </summary>
        public CoAPPayload() { }
        /// <summary>
        /// Creates a payload with string data
        /// </summary>
        /// <param name="payloadData">Payload data as string</param>
        public CoAPPayload(string payloadData)
        {
            if (payloadData == null || payloadData.Trim().Length == 0)
                throw new ArgumentNullException("Payload data cannot be NULL or empty string");
            if(payloadData.Trim().Length > AbstractNetworkUtils.GetMaxMessageSize()/2)
                throw new ArgumentException("Payload size cannot be more than " + AbstractNetworkUtils.GetMaxMessageSize() / 2);
            this.Value = AbstractByteUtils.StringToByteUTF8(payloadData.Trim());
        }
        /// <summary>
        /// Creates a payload with byte stream
        /// </summary>
        /// <param name="payloadData">Payload as byte stream</param>
        public CoAPPayload(byte[] payloadData)
        {
            if (payloadData == null || payloadData.Length == 0)
                throw new ArgumentNullException("Payload data cannot be NULL or empty byte stream");
            if (payloadData.Length > AbstractNetworkUtils.GetMaxMessageSize() / 2)
                throw new ArgumentException("Payload size cannot be more than " + AbstractNetworkUtils.GetMaxMessageSize() / 2);
            this.Value = payloadData;
        }
        #endregion

        #region Operations
        /// <summary>
        /// Parse the incoming message and extract the payload
        /// </summary>
        /// <param name="coapMsgStream">The CoAP message stream that contains the payload information</param>
        /// <param name="startIndex">The index from where to start reading the message stream</param>
        /// <param name="extraInfo">Not used</param>
        /// <returns>The next location post the version information</returns>
        public int Parse(byte[] coapMsgStream, int startIndex, ushort extraInfo)
        {
            if (coapMsgStream == null || coapMsgStream.Length == 0 || startIndex < 0) return startIndex;//do nothing 
            if (coapMsgStream.Length < AbstractCoAPMessage.HEADER_LENGTH) throw new CoAPFormatException("Invalid CoAP message stream");
            if (startIndex >= coapMsgStream.Length) return startIndex; //do nothing..end of stream
            
            byte payloadMarker = coapMsgStream[startIndex];
            if (payloadMarker != 0xFF) throw new CoAPFormatException("Invalid payload marker");
            //Payload marker found...must have the payload
            if (startIndex + 1 > coapMsgStream.Length - 1)
                throw new CoAPFormatException("Payload marker present, but no payload");
            int elmsToCopy = coapMsgStream.Length - (startIndex + 1);
            Value = new byte[elmsToCopy];
            Array.Copy(coapMsgStream, startIndex + 1, Value, 0, elmsToCopy);
            //TOCHECK::Value = AbstractNetworkUtils.FromNetworkByteOrder(Value);
            return (coapMsgStream.Length);//Last one
        }
        /// <summary>
        /// Convert this object into a byte stream in network byte order
        /// </summary>
        /// <param name="reserved">Not used now</param>
        /// <returns>byte array</returns>
        public byte[] ToStream(UInt16 reserved)
        {
            byte[] thePayloadStream = null;
            if (this.Value != null && this.Value.Length > 0)
            {
                thePayloadStream = new byte[this.Value.Length];
                Array.Copy(this.Value, 0, thePayloadStream, 0, this.Value.Length);
                //TOCHECK::thePayloadStream = AbstractNetworkUtils.ToNetworkByteOrder(thePayloadStream);
            }
            return thePayloadStream;
        }
        /// <summary>
        /// Not used...always returns true
        /// </summary>
        /// <param name="value">NA</param>
        /// <returns>true</returns>
        public bool IsValid(ushort value)
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
            if (this.Value == null || this.Value.Length == 0)
                return "";
            else
                return AbstractByteUtils.ByteToStringUTF8(this.Value);
        }
        #endregion
    }
}
