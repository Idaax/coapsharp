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
using Idaax.CoAP.Helpers;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Idaax.CoAP.Message;
using System.Collections;

using Idaax.CoAP.Exceptions;

namespace Idaax.CoAP.Channels
{
    /// <summary>
    /// This class implements a server node type. Any constrained device, that is
    /// willing to act as a server, shoul make use of this class
    /// </summary>
    public class CoAPServerChannel : AbstractCoAPChannel
    {
        #region Implementation
        /// <summary>
        /// The underlying socket
        /// </summary>
        protected Socket _socket = null;
        /// <summary>
        /// For thread lifetime management
        /// </summary>
        protected bool _isDone = false;
        /// <summary>
        /// Holds the port number to listen on
        /// </summary>
        protected int _port = 0;
        /// <summary>
        /// Holds the hostname/ip address
        /// </summary>
        protected string _host = null;
        /// <summary>
        /// Holds messages pending separate response
        /// </summary>
        protected SeparateResponseQueue _separateResponseQ = null;
        /// <summary>
        /// Holds a list of observers
        /// </summary>
        protected ObserversList _observers = null;
        #endregion                

        #region Properties
        /// <summary>
        /// Accessor for the list of clients that are currently 
        /// observing one or more resources on this server
        /// </summary>
        public ObserversList ObserversList { get { return this._observers; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public CoAPServerChannel()
        {
            //Setup basic parameters
            this.AckTimeout = AbstractCoAPChannel.DEFAULT_ACK_TIMEOUT_SECS;
            this.MaxRetransmissions = AbstractCoAPChannel.DEFAULT_MAX_RETRANSMIT;
        }
        #endregion

        #region Server Management
        /// <summary>
        /// Start the server
        /// </summary>
        /// <param name="host">Hostname to use for this server (DNS/IP)</param>
        /// <param name="port">The port number to listen on</param>
        public override void Initialize(string host, int port)        
        {
            this._host = (host == null || host.Trim().Length ==0 ) ? "unknown" : host;
            if (port <= 0)
                this._port = AbstractNetworkUtils.GetDefaultCoAPPort();
            else
                this._port = port;
            
            Shutdown(); //close all previous connections
            
            //Create the wait q
            this._msgPendingAckQ = new TimedQueue((uint)AbstractCoAPChannel.DEFAULT_ACK_TIMEOUT_SECS);
            this._msgPendingAckQ.OnResponseTimeout += new TimedQueue.TimedOutWaitingForResponse(OnTimedOutWaitingForResponse);
            
            //Create the separate response q
            this._separateResponseQ = new SeparateResponseQueue();

            //Create the observers list
            this._observers = new ObserversList();

            // Create a socket, bind it to the server's port and listen for client connections            
            this._isDone = false;
            this.ReInitSocket();
            Thread waitForClientThread = new Thread(new ThreadStart(WaitForConnections));
            waitForClientThread.Start();
        }        
        /// <summary>
        /// Re-initialize the socket only
        /// </summary>
        /// <returns>bool (true on success else false)</returns>
        protected bool ReInitSocket()
        {
            bool success = false;
            try
            {
                if (this._socket != null) this._socket.Close();
                this._socket = null;
                this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); /*CoAP is UDP based*/
                EndPoint localEndPoint = new IPEndPoint(IPAddress.Any, this._port);
                this._socket.Bind(localEndPoint);
                success = true;
            }
            catch (Exception e)
            {
                this.HandleError(e, null);
            }
            return success;
        }
        /// <summary>
        /// Shutdown the client channel
        /// </summary>
        public override void Shutdown()
        {
            this._isDone = true;
            if (this._socket != null)
            {
                this._socket.Close();
                this._socket = null;
            }
            if (this._msgPendingAckQ != null)
                this._msgPendingAckQ.Shutdown();
            this._msgPendingAckQ = null;            
            this._separateResponseQ = null;
            this._observers = null;
        }
        #endregion

        #region Message Sending
        /// <summary>
        /// Send the CoAP message to client
        /// </summary>
        /// <param name="coapMsg">The CoAP message to send</param>
        /// <returns>Number of bytes sent</returns>
        public override int Send(AbstractCoAPMessage coapMsg)
        {
            if (coapMsg == null) throw new ArgumentNullException("Message is NULL");            
            if (this._socket == null) throw new InvalidOperationException("CoAP server not yet started");

            int bytesSent = 0;
            try
            {
                //We do not want server to die when a socket send error occurs...
                //just clean all settings specific to the remote client and
                //keep going                
                byte[] coapBytes = coapMsg.ToByteStream();
                bytesSent = this._socket.SendTo(coapBytes, coapMsg.RemoteSender);
                if (coapMsg.MessageType.Value == CoAPMessageType.CON)
                {
                    //confirmable message...need to wait for a response
                    coapMsg.DispatchDateTime = DateTime.UtcNow;
                    this._msgPendingAckQ.AddToWaitQ(coapMsg);
                }
            }
            catch(Exception e)
            {
                this._msgPendingAckQ.RemoveFromWaitQ(coapMsg.ID.Value);
                if (coapMsg.GetType() == typeof(CoAPRequest))
                {
                    CoAPRequest coapReq = (CoAPRequest)coapMsg;
                    this._observers.RemoveResourceObserver(coapReq.GetURL() , coapReq.Token.Value);
                }
                else if (coapMsg.GetType() == typeof(CoAPResponse))
                {
                    CoAPResponse coapResp = (CoAPResponse)coapMsg;
                    this._observers.RemoveResourceObserver(coapResp);
                }
                this.HandleError(e, coapMsg);
            }
            
            return bytesSent;
        }
        /// <summary>
        /// Once a confirmable message is sent, it must wait for an ACK or RST
        /// If nothing comes within the timeframe, this event is raised.
        /// </summary>
        /// <param name="coapMsg">An instance of AbstractCoAPMessage</param>
        protected virtual void OnTimedOutWaitingForResponse(AbstractCoAPMessage coapMsg)
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
                coapMsg.Timeout = (int)(Math.Pow(AbstractCoAPChannel.DEFAULT_ACK_TIMEOUT_SECS, coapMsg.RetransmissionCount + 1) * AbstractCoAPChannel.DEFAULT_ACK_RANDOM_FACTOR);
                //attempt resend
                this.Send(coapMsg);
            }
        }
        /// <summary>
        /// Add this request to the pending separate response queue.
        /// The message can be extracted later and acted upon
        /// </summary>
        /// <param name="coapReq">CoAPRequest</param>
        public virtual void AddToPendingSeparateResponse(CoAPRequest coapReq)
        {
            if (this._separateResponseQ == null)
                throw new InvalidOperationException("Please initialize the server first");
            if (coapReq == null)
                throw new ArgumentNullException("CoAPRequest to add to this queue cannot be NULL");            
            this._separateResponseQ.Add(coapReq);
        }
        /// <summary>
        /// Get the next request from the Q that was pending a separate response.
        /// If nothing is pending then NULL value is returned
        /// </summary>
        /// <returns>CoAPRequest</returns>
        public virtual CoAPRequest GetNextRequestPendingSeparateResponse()
        {
            if (this._separateResponseQ == null)
                throw new InvalidOperationException("Please initialize the server first");
            CoAPRequest coapReq = (CoAPRequest)this._separateResponseQ.GetNextPendingRequest();
            return coapReq;
        }
        #endregion

        #region Socket Thread
        /// <summary>
        /// This is the thread where the socket server will accept client connections and process
        /// </summary>
        protected void WaitForConnections()
        {
            EndPoint sender = null;
            byte[] buffer = null;
            ArrayList previousBytes = new ArrayList();
            int bytesRead = 0;
            byte mType = 0;
            UInt16 mId = 0;
            byte[] udpMsg = null;
            int maxSize = AbstractNetworkUtils.GetMaxMessageSize();
            while (!this._isDone)
            {
                try
                {
                    if (this._socket.Available >= 4 /*Min size of CoAP block*/)
                    {
                        sender = new IPEndPoint(IPAddress.Any, 0);
                        buffer = new byte[maxSize * 2];
                        bytesRead = this._socket.ReceiveFrom(buffer, ref sender);
                        udpMsg = new byte[bytesRead];
                        Array.Copy(buffer, udpMsg, bytesRead);
                        
                        mType = AbstractCoAPMessage.PeekMessageType(udpMsg);
                        mId = AbstractCoAPMessage.PeekMessageID(udpMsg);

                        if ((mType == CoAPMessageType.CON ||
                              mType == CoAPMessageType.NON) && AbstractCoAPMessage.PeekIfMessageCodeIsRequestCode(udpMsg))
                            this.ProcessRequestMessageReceived(udpMsg, ref sender);
                        else
                            this.ProcessResponseMessageReceived(udpMsg, ref sender);
                    }
                    else
                    {
                        //Nothing on the socket...wait
                        Thread.Sleep(5000);
                    }
                }
                catch (SocketException se)
                {
                    //Try to re-initialize socket, and proceed only when the socket
                    //is successfully re-initialized
                    this._isDone = !this.ReInitSocket();
                    this.HandleError(se, null);
                }
                catch (ArgumentNullException argEx)
                {
                    if (mType == CoAPMessageType.CON)
                        this.RespondToBadCONRequest(mId);
                    this.HandleError(argEx, null);
                }
                catch (ArgumentException argEx)
                {
                    if (mType == CoAPMessageType.CON)
                        this.RespondToBadCONRequest(mId);
                    this.HandleError(argEx, null);
                }
                catch (CoAPFormatException fEx)
                {
                    //Invalid message..
                    if (mType == CoAPMessageType.CON)
                        this.RespondToBadCONRequest(mId);
                    this.HandleError(fEx, null);
                }
                                             
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Handle request received from remote clients
        /// </summary>
        /// <param name="msgStream">The byte stream that represents a request message</param>
        /// <param name="sender">The remote sender endpoint</param>
        protected virtual void ProcessRequestMessageReceived(byte[] msgStream , ref EndPoint sender)
        {            
            CoAPRequest coapReq = new CoAPRequest();
            IPEndPoint remoteSender = (IPEndPoint) sender;
            try
            {
                coapReq.FromByteStream(msgStream);
                coapReq.RemoteSender = new IPEndPoint(remoteSender.Address , remoteSender.Port);//Setup who sent this message
                
                //setup the default values of host and port
                if (!coapReq.Options.HasOption(CoAPHeaderOption.URI_HOST))
                    coapReq.Options.AddOption(CoAPHeaderOption.URI_HOST, AbstractByteUtils.StringToByteUTF8(this._host));
                if (!coapReq.Options.HasOption(CoAPHeaderOption.URI_PORT))
                    coapReq.Options.AddOption(CoAPHeaderOption.URI_PORT, AbstractByteUtils.GetBytes((UInt16)this._port));

                if (coapReq.MessageType.Value == CoAPMessageType.CON &&
                    coapReq.Code.Value == CoAPMessageCode.EMPTY)
                {
                    //This is a PING..send a RST
                    this.RespondToPing(coapReq);
                }
                else
                {                    
                    this.HandleRequestReceived(coapReq);//Other messages, let program handle it
                }
            }
            catch
            {
                ;//TOCHECK::Do nothing, we do not want to crash the server just because we
                //could not process one received message...will check later how to improve this
            }            
        }
        /// <summary>
        /// Handle response message received from remote clients
        /// </summary>
        /// <param name="msgStream">The byte stream that represents a response message</param>
        /// <param name="sender">The remote sender endpoint</param>
        protected virtual void ProcessResponseMessageReceived(byte[] msgStream, ref EndPoint sender)
        {
            CoAPResponse coapResp = new CoAPResponse();
            IPEndPoint remoteSender = (IPEndPoint)sender;
            try
            {
                //This is a response                
                coapResp.FromByteStream(msgStream);
                coapResp.RemoteSender = new IPEndPoint(remoteSender.Address, remoteSender.Port);//Setup who sent this message
                //Remove the waiting confirmable message from the timeout queue
                if (coapResp.MessageType.Value == CoAPMessageType.RST ||
                    coapResp.MessageType.Value == CoAPMessageType.ACK)
                    this._msgPendingAckQ.RemoveFromWaitQ(coapResp.ID.Value);
                //If this is a RST, remove any observers
                if (coapResp.MessageType.Value == CoAPMessageType.RST)
                    this._observers.RemoveResourceObserver(coapResp);
                this.HandleResponseReceived(coapResp);
            }
            catch
            {
                ;//TOCHECK::Do nothing, we do not want to crash the server just because we
                //could not process one received message...will check later how to improve this
            }
        }
        /// <summary>
        /// When a CON is received with EMPTY message type from the client, it means the
        /// client is simply pinging. We need to send a RST message
        /// </summary>
        /// <param name="pingReq">The ping request</param>
        protected virtual void RespondToPing(CoAPRequest pingReq)
        {
            CoAPResponse resp = new CoAPResponse(CoAPMessageType.RST, CoAPMessageCode.EMPTY, pingReq);
            this.Send(resp);
        }
        /// <summary>
        /// When a CON is received but we cannot understand the message any further for any reason
        /// (e.g. invalid format), we want to send a RST. We make a best case attempt to
        /// find out the message ID if possible, else we send zero
        /// </summary>
        /// <param name="mId">The message Id</param>
        protected virtual void RespondToBadCONRequest(UInt16 mId)
        {
            CoAPResponse resp = new CoAPResponse(CoAPMessageType.RST, CoAPMessageCode.BAD_REQUEST , mId);            
            this.Send(resp);
        }
        #endregion
    }
}
