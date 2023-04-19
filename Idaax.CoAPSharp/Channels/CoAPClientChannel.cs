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
using System.Net.Sockets;
using System.Threading;

using Idaax.CoAP.Exceptions;
using Idaax.CoAP.Message;
using Idaax.CoAP.Helpers;

namespace Idaax.CoAP.Channels
{
    /// <summary>
    /// This class represents a CoAP channel that behaves like a client.
    /// This is the default implementation. If you need a different implementation,
    /// extend the AbstractCoAPChannel class and create your own
    /// </summary>
    public class CoAPClientChannel : AbstractCoAPChannel
    {
        #region Implementation
        /// <summary>
        /// Holds the client socket
        /// </summary>
        protected Socket _clientSocket = null;
        /// <summary>
        /// Holds the remote endpoint to which this client is connected to
        /// </summary>
        protected EndPoint _remoteEP = null;
        /// <summary>
        /// For thread lifetime management
        /// </summary>
        protected bool _isDone = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public CoAPClientChannel()
        {
            //Setup basic parameters
            this.AckTimeout = AbstractCoAPChannel.DEFAULT_ACK_TIMEOUT_SECS;
            this.MaxRetransmissions = AbstractCoAPChannel.DEFAULT_MAX_RETRANSMIT;
        }
        #endregion

        #region Lifetime Management
        /// <summary>
        /// Initialize all basic aspects of the client channel
        /// </summary>
        /// <param name="host">The IP host</param>
        /// <param name="port">The port number</param>
        public override void Initialize(string host, int port)
        {
            Shutdown();
            
            //Connect to host            
            IPAddress ipAddr = AbstractNetworkUtils.GetIPAddressFromHostname(host) ;
            this._remoteEP = new IPEndPoint(ipAddr, port);
            this._clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this._clientSocket.Connect(this._remoteEP);
            //Initialize ACK Q
            this._msgPendingAckQ = new TimedQueue((UInt16)this.AckTimeout);
            this._msgPendingAckQ.OnResponseTimeout += new TimedQueue.TimedOutWaitingForResponse(OnTimedOutWaitingForResponse);
            //Start message processing thread
            this._isDone = false;
            Thread waitForClientThread = new Thread(new ThreadStart(ProcessReceivedMessages));
            waitForClientThread.Start();
        }
        
        /// <summary>
        /// Shutdown the client channel
        /// </summary>
        public override void Shutdown()
        {
            this._isDone = true;
            Thread.Sleep(2000);
            if (this._clientSocket != null)
                this._clientSocket.Close();
            if (this._msgPendingAckQ != null)
                this._msgPendingAckQ.Shutdown();
            this._msgPendingAckQ = null;
            this._remoteEP = null;
        }
        #endregion

        #region Operations
        /// <summary>
        /// Send a CoAP message to the server
        /// </summary>
        /// <param name="coapMsg">The CoAP message to send to server</param>
        /// <returns>Number of bytes sent</returns>
        public override int Send(AbstractCoAPMessage coapMsg)
        {
            if (coapMsg == null) throw new ArgumentNullException("Message is NULL");
            if (this._clientSocket == null) throw new InvalidOperationException("CoAP client not yet started");
            int bytesSent = 0;
            try
            {
                byte[] coapBytes = coapMsg.ToByteStream();
                if (coapBytes.Length > AbstractNetworkUtils.GetMaxMessageSize())
                    throw new UnsupportedException("Message size too large. Not supported. Try block option");
                bytesSent = this._clientSocket.Send(coapBytes);
                if (coapMsg.MessageType.Value == CoAPMessageType.CON)
                {
                    //confirmable message...need to wait for a response
                    if (coapMsg.Timeout <= 0) coapMsg.Timeout = AbstractCoAPChannel.DEFAULT_ACK_TIMEOUT_SECS;
                    coapMsg.DispatchDateTime = DateTime.UtcNow;
                    this._msgPendingAckQ.AddToWaitQ(coapMsg);
                }
            }
            catch (Exception e)
            {
                this._msgPendingAckQ.RemoveFromWaitQ(coapMsg.ID.Value);                
                this.HandleError(e, coapMsg);
            }
            
            return bytesSent;
        }
        /// <summary>
        /// Once a confirmable message is sent, it must wait for an ACK or RST
        /// If nothing comes within the timeframe, this event is raised.
        /// </summary>
        /// <param name="coapMsg">An instance of AbstractCoAPMessage</param>
        private void OnTimedOutWaitingForResponse(AbstractCoAPMessage coapMsg)
        {
            //make an attempt to retransmit
            coapMsg.RetransmissionCount++;
            if (coapMsg.RetransmissionCount > this.MaxRetransmissions)
            {
                //Exhausted max retransmit
                this.HandleError(new UndeliveredException("Cannot deliver message. Exhausted retransmit attempts"), coapMsg);
            }
            else
            {
                coapMsg.Timeout = (int)(coapMsg.RetransmissionCount * this.AckTimeout * AbstractCoAPChannel.DEFAULT_ACK_RANDOM_FACTOR);
                //attempt resend
                this.Send(coapMsg);
            }
        }
        #endregion

        #region Thread
        /// <summary>
        /// This thread continuously looks for messages on the socket
        /// Once available, it will post them for handling downstream
        /// </summary>
        protected void ProcessReceivedMessages()
        {
            byte[] buffer = null;
            int maxSize = AbstractNetworkUtils.GetMaxMessageSize();
            while (!this._isDone)
            {
                Thread.Sleep(1000);
                try
                {
                    if (this._clientSocket.Available >= 4 /*Min size of CoAP block*/)
                    {
                        buffer = new byte[maxSize * 2];
                        int bytesRead = this._clientSocket.Receive(buffer);
                        byte[] udpMsg = new byte[bytesRead];
                        Array.Copy(buffer, udpMsg, bytesRead);
                        byte mType = AbstractCoAPMessage.PeekMessageType(udpMsg);
                        
                        if ( (mType == CoAPMessageType.CON ||
                              mType == CoAPMessageType.NON) && AbstractCoAPMessage.PeekIfMessageCodeIsRequestCode(udpMsg))
                        {
                            //This is a request
                            CoAPRequest coapReq = new CoAPRequest();
                            coapReq.FromByteStream(udpMsg);
                            coapReq.RemoteSender = this._remoteEP;//Setup who sent this message
                            string uriHost = ((IPEndPoint)this._remoteEP).Address.ToString();
                            UInt16 uriPort = (UInt16)((IPEndPoint)this._remoteEP).Port;

                            //setup the default values of host and port
                            //setup the default values of host and port
                            if (!coapReq.Options.HasOption(CoAPHeaderOption.URI_HOST))
                                coapReq.Options.AddOption(CoAPHeaderOption.URI_HOST, AbstractByteUtils.StringToByteUTF8(uriHost));
                            if (!coapReq.Options.HasOption(CoAPHeaderOption.URI_PORT))
                                coapReq.Options.AddOption(CoAPHeaderOption.URI_PORT, AbstractByteUtils.GetBytes(uriPort));

                            this.HandleRequestReceived(coapReq);
                        }
                        else
                        {
                            //This is a response
                            CoAPResponse coapResp = new CoAPResponse();
                            coapResp.FromByteStream(udpMsg);
                            coapResp.RemoteSender = this._remoteEP;//Setup who sent this message
                            //Remove the waiting confirmable message from the timeout queue
                            if (coapResp.MessageType.Value == CoAPMessageType.ACK ||
                                coapResp.MessageType.Value == CoAPMessageType.RST)
                            {
                                this._msgPendingAckQ.RemoveFromWaitQ(coapResp.ID.Value);
                            }
                            this.HandleResponseReceived(coapResp);
                        }
                    }
                }
                catch (SocketException se)
                {
                    //Close this client connection
                    this._isDone = true;
                    this.HandleError(se, null);
                }
                catch (ArgumentNullException argEx)
                {
                    this.HandleError(argEx, null);
                }
                catch (ArgumentException argEx)
                {
                    this.HandleError(argEx, null);
                }
                catch (CoAPFormatException fEx)
                {
                    //Invalid message..
                    this.HandleError(fEx, null);
                }                
            }
        }
        #endregion
    }
}
