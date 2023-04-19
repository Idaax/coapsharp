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
using System.Net;

namespace Idaax.CoAP.Helpers
{
    /// <summary>
    /// A class to provide utility methods to work with the network
    /// </summary>
    public abstract class AbstractNetworkUtils
    {
        /// <summary>
        /// Convert the byte array to network byte order. If the conditional compilation
        /// symbol _BIG_ENDIAN is defined, then it is assumed that the target machine is
        /// big endian else its assumed that the target machine is little endian
        /// </summary>
        /// <param name="data">A byte array</param>
        /// <returns>byte array in network byte order</returns>
        public static byte[] ToNetworkByteOrder(byte[] data)
        {
            if (data == null || data.Length < 2) return data;
            if (AbstractByteUtils.IsTargetLittleEndian())
                data = AbstractByteUtils.ReverseBytes(data);

            return data;
        }
        /// <summary>
        /// Convert the byte array into local representation from network byte order representation
        /// If the conditional compilation symbol _BIG_ENDIAN is defined, then it is assumed 
        /// that the target machine is big endian else its assumed that the target machine 
        /// is little endian
        /// </summary>
        /// <param name="data">A byte array</param>
        /// <returns>byte array in local byte order</returns>
        public static byte[] FromNetworkByteOrder(byte[] data)
        {
            if (data == null || data.Length < 2) return data;
            if (AbstractByteUtils.IsTargetLittleEndian())
                data = AbstractByteUtils.ReverseBytes(data);
            return data;
        }
        
        /// <summary>
        /// Get the default CoAP port address
        /// </summary>
        /// <returns>UInt16</returns>
        public static UInt16 GetDefaultCoAPPort()
        {
            return 5683;
        }
        /// <summary>
        /// Get the maximum size of the message in bytes. This includes header, token, options and payload 
        /// The current value is set at 2KB bytes. 
        /// For anything bigger than this, please look at Block option
        /// </summary>
        /// <returns>UInt16</returns>
        public static UInt16 GetMaxMessageSize()
        {            
            return 2048;
        }
        /// <summary>
        /// Get the IP address from hostname
        /// </summary>
        /// <param name="hostname">The hostname whose IP address is required</param>
        /// <returns>IPAddress</returns>
        public static IPAddress GetIPAddressFromHostname(string hostname)
        {
            IPAddress ipAddr = null;
            bool strRepresentsIPAddr = false;
            try
            {
                ipAddr = IPAddress.Parse(hostname);
                strRepresentsIPAddr = true;
            }
            catch
            {
                ; ;//String does not represent an IP address
            }
            if (!strRepresentsIPAddr)
            {
                IPHostEntry ipe = Dns.GetHostEntry(hostname);
                foreach (IPAddress ipAdrrInList in ipe.AddressList)
                {
                    if (ipAdrrInList != null)
                    {
                        ipAddr = ipAdrrInList;
                        break;
                    }
                }
            }
            return ipAddr;
        }
    }
}
