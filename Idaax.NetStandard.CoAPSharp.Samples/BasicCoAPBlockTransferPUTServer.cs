using Idaax.CoAP.Channels;
using Idaax.CoAP.Helpers;
using Idaax.CoAP.Message;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Idaax.NetStandard.CoAPSharp.Samples
{
    public class BasicCoAPBlockTransferPUTServer : ICoAPSample
    {
        /// <summary>
        /// The server channel
        /// </summary>
        private CoAPServerChannel _server = null;
        /// <summary>
        /// Holds the received bytes along with the sequence number
        /// </summary>
        private Hashtable _rxBytes = null;
        /// <summary>
        /// Entry point
        /// </summary>
        public void Run()
        {
            BasicCoAPBlockTransferPUTServer p = new BasicCoAPBlockTransferPUTServer();
            p.StartServer();
            Thread.Sleep(Timeout.Infinite);
        }
        /// <summary>
        /// Start the server
        /// </summary>
        public void StartServer()
        {
            this._server = new CoAPServerChannel();
            this._server.Initialize(null, 5683);

            this._server.CoAPError += new CoAPErrorHandler(OnCoAPError);
            this._server.CoAPRequestReceived += new CoAPRequestReceivedHandler(OnCoAPRequestReceived);
            this._server.CoAPResponseReceived += new CoAPResponseReceivedHandler(OnCoAPResponseReceived);
        }
        /// <summary>
        /// Not using this for now
        /// </summary>
        /// <param name="coapResp">CoAPResponse</param>
        void OnCoAPResponseReceived(CoAPResponse coapResp)
        {
            Console.WriteLine($"{this.GetType().Name} :: Response Received {coapResp}");
        }
        /// <summary>
        /// This is where we will get the PUT request
        /// </summary>
        /// <param name="coapReq">CoAPRequest</param>
        void OnCoAPRequestReceived(CoAPRequest coapReq)
        {
            Console.WriteLine($"{this.GetType().Name} :: Request received {coapReq}");

            string path = coapReq.GetPath();
            /*Error checking not done to simplify example*/
            if (coapReq.MessageType.Value == CoAPMessageType.CON &&
                coapReq.Code.Value == CoAPMessageCode.PUT &&
                path == "largedata/blockput")
            {
                if (this._rxBytes == null) this._rxBytes = new Hashtable();
                CoAPBlockOption rxBlockOption = coapReq.GetBlockOption(CoAPHeaderOption.BLOCK1);
                if (rxBlockOption != null)
                {
                    byte[] rxBytes = coapReq.Payload.Value;
                    if (this._rxBytes.Contains(rxBlockOption.SequenceNumber))
                        this._rxBytes[rxBlockOption.SequenceNumber] = rxBytes;//Update
                    else
                        this._rxBytes.Add(rxBlockOption.SequenceNumber, rxBytes);//Add
                    //Now send an ACK
                    CoAPBlockOption ackBlockOption = new CoAPBlockOption(rxBlockOption.SequenceNumber,
                                                        false /*incidate to client that we have guzzled all the bytes*/,
                                                        rxBlockOption.SizeExponent);
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK, CoAPMessageCode.CONTENT, coapReq.ID.Value);
                    resp.Token = coapReq.Token;
                    resp.RemoteSender = coapReq.RemoteSender;
                    resp.SetBlockOption(CoAPHeaderOption.BLOCK1, ackBlockOption);
                    this._server.Send(resp);
                }
            }
        }
        /// <summary>
        /// Not using this for now
        /// </summary>
        /// <param name="e">The exception that occurred</param>
        /// <param name="associatedMsg">The associated message that caused the exception (if any)</param>
        void OnCoAPError(Exception e, AbstractCoAPMessage associatedMsg)
        {
            Console.WriteLine($"{this.GetType().Name} :: Error {e} , associated message {associatedMsg}");
        }
        /// <summary>
        /// This method is called after all data is received
        /// </summary>
        private void ProcessReceivedData()
        {
            //do some processing here
        }
    }
}
