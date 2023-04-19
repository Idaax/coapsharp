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
    /// This class represents the CoAP version being used. As of now, only V 1.0 is supported
    /// </summary>
    public class CoAPVersion : IParsable
    {
        #region Constants
        /// <summary>
        /// Indicates the latest version CoAP supported by this library
        /// </summary>
        public const byte LATEST_VERSION = 0x01;
        #endregion

        #region Implementation
        /// <summary>
        /// List of all supported versions of CoAP by this library
        /// </summary>
        protected byte[] _supportedVersions = new byte[] { LATEST_VERSION};
        #endregion

        #region Properties
        /// <summary>
        /// Accessor for the current version of the CoAP that this library supports
        /// </summary>
        public byte CurrentVersion { get { return LATEST_VERSION; } }
        /// <summary>
        /// Accessor for supported versions of CoAP by this library
        /// </summary>
        public byte[] SupportedVersions { get { return _supportedVersions;} }
        #endregion

        #region Operations
        /// <summary>
        /// Parse the CoAP message stream and extract the version.
        /// Unsupported version will throw format exception
        /// </summary>
        /// <param name="coapMsgStream">The CoAP message stream that contains the version information</param>
        /// <param name="startIndex">The index from where to start reading the message stream to seek version information</param>
        /// <param name="extraInfo">Not used</param>
        /// <returns>The next location post the version information</returns>
        public int Parse(byte[] coapMsgStream, int startIndex, UInt16 extraInfo)
        {
            if (coapMsgStream == null || coapMsgStream.Length == 0 || startIndex < 0) return startIndex;//do nothing 
            if (coapMsgStream.Length < AbstractCoAPMessage.HEADER_LENGTH) throw new CoAPFormatException("Invalid CoAP message stream");
            if (startIndex >= coapMsgStream.Length) throw new ArgumentException("Start index beyond message stream length");
            //First two bits contain the version information (bits 0-1 starting from left)
            byte version = (byte)(coapMsgStream[startIndex] >> 6);
            if (!this.IsValid(version)) throw new CoAPFormatException("CoAP version not supported.");
            return startIndex; //Start index does not change...we have not covered one full byte.
        }
        /// <summary>
        /// Convert this into a byte stream in network byte order
        /// </summary>
        /// <param name="reserved">Not used now</param>
        /// <returns>byte array</returns>
        public byte[] ToStream(UInt16 reserved)
        {
            byte[] ver = new byte[1] { this.CurrentVersion };
            ver[0] = (byte)(ver[0] << 6); //first two bits represent the version (MSB)
            return ver;
        }
        /// <summary>
        /// Check if the passed version number is valid or not
        /// </summary>
        /// <param name="version">The version number to check</param>
        /// <returns>boolean</returns>
        public bool IsValid(UInt16 version)
        {
            return (version == CurrentVersion);
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Convert to a string representation
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return "Version:" + CurrentVersion.ToString();
        }
        #endregion
    }
}
