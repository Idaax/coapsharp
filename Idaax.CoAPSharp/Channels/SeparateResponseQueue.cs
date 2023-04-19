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
using System.Threading;
using Idaax.CoAP.Message;

namespace Idaax.CoAP.Channels
{
    /// <summary>
    /// In CoAP, there is a concept of separate response. If the server wants sometime to process
    /// the message, it can send an empty ACK and later on, when the data is ready, it can
    /// send the response message. This is separate response. The request response is matched
    /// based on the token value. 
    /// For CON request, an ACK is sent and then later on a CON response is sent
    /// For NON request, nothing is sent, but simply after a while another NON is sent to client
    /// </summary>
    public class SeparateResponseQueue 
    {
        #region Implementation
        /// <summary>
        /// Holds requests against which a separate response is to be sent
        /// </summary>
        protected Queue _separateResponseQ = new Queue();
        /// <summary>
        /// For queue access synchronization
        /// </summary>
        protected AutoResetEvent _separateRespQSync = new AutoResetEvent(true);
        #endregion

        #region Operations
        /// <summary>
        /// Add this request to the pending separate response queue.
        /// The message can be extracted later and acted upon
        /// </summary>
        /// <param name="coapReq">CoAPRequest</param>
        public virtual void Add(CoAPRequest coapReq)
        {
            if (coapReq == null)
                throw new ArgumentNullException("CoAPRequest to add to this queue cannot be NULL");
            this._separateRespQSync.WaitOne();
            this._separateResponseQ.Enqueue(coapReq);
            this._separateRespQSync.Set();
        }
        /// <summary>
        /// Get the next request from the Q that was pending a separate response.
        /// If nothing is pending then NULL value is returned
        /// </summary>
        /// <returns>CoAPRequest</returns>
        public virtual CoAPRequest GetNextPendingRequest()
        {
            CoAPRequest coapReq = null;
            this._separateRespQSync.WaitOne();
            if (this._separateResponseQ.Count > 0 && this._separateResponseQ.Peek() != null)
                coapReq = (CoAPRequest)this._separateResponseQ.Dequeue();
            this._separateRespQSync.Set();

            return coapReq;
        }
        #endregion
    }
}
