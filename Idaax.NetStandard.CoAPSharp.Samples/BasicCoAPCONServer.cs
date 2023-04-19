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
    public class BasicCoAPCONServer : ICoAPSample
    {
        /// <summary>
        /// Holds the server channel instance
        /// </summary>
        private CoAPServerChannel _server = null;
        /// <summary>
        /// Entry point
        /// </summary>
        public void Run()
        {
            BasicCoAPCONServer p = new BasicCoAPCONServer();
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
            this._server.CoAPResponseReceived += new CoAPResponseReceivedHandler(OnCoAPResponseReceived);
            this._server.CoAPRequestReceived += new CoAPRequestReceivedHandler(OnCoAPRequestReceived);
            this._server.CoAPError += new CoAPErrorHandler(OnCoAPError);
        }
        /// <summary>
        /// Called when an error occurs.Not used in this sample
        /// </summary>
        /// <param name="e">The exception</param>
        /// <param name="associatedMsg">Associated message if any, else null</param>
        void OnCoAPError(Exception e, AbstractCoAPMessage associatedMsg)
        {
            Console.WriteLine($"{this.GetType().Name} :: Error {e} , associated message {associatedMsg}");
        }
        /// <summary>
        /// Called when a CoAP request is received...we will only support CON requests 
        /// of type GET... the path is sensors/temp
        /// </summary>
        /// <param name="coapReq">CoAPRequest object</param>
        void OnCoAPRequestReceived(CoAPRequest coapReq)
        {
            Console.WriteLine($"{this.GetType().Name} :: Request received {coapReq}");

            string reqPath = (coapReq.GetPath() != null) ? coapReq.GetPath().ToLower() : "";

            if (coapReq.MessageType.Value == CoAPMessageType.CON)
            {
                if (coapReq.Code.Value != CoAPMessageCode.GET)
                {
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK,
                                                        CoAPMessageCode.METHOD_NOT_ALLOWED,
                                                        coapReq /*Copy all necessary values from request in the response*/);
                    //When you use the constructor that accepts a request, then automatically
                    //the message id , token and remote sender values are copied over to the response
                    this._server.Send(resp);
                }
                else if (reqPath != "sensors/temp")
                {
                    //We do not understand this..
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK,
                                                            CoAPMessageCode.NOT_FOUND,
                                                            coapReq /*Copy all necessary values from request in the response*/);
                    this._server.Send(resp);
                }
                else
                {
                    Debug.Print(coapReq.ToString());
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK,
                                                        CoAPMessageCode.CONTENT,
                                                        coapReq /*Copy all necessary values from request in the response*/);
                    //The payload will be JSON
                    Hashtable ht = new Hashtable();
                    ht.Add("temp", this.GetRoomTemperature());
                    string jsonStr = JSONResult.ToJSON(ht);
                    resp.AddPayload(jsonStr);
                    //Tell recipient about the content-type of the response
                    resp.AddOption(CoAPHeaderOption.CONTENT_FORMAT, AbstractByteUtils.GetBytes(CoAPContentFormatOption.APPLICATION_JSON));
                    //send it
                    this._server.Send(resp);
                }
            }
        }
        /// <summary>
        /// Called when CoAP response is received (ACK, RST). Not used in this sample
        /// </summary>
        /// <param name="coapResp">CoAPResponse</param>
        void OnCoAPResponseReceived(CoAPResponse coapResp)
        {
            Console.WriteLine($"{this.GetType().Name} :: Response Received {coapResp}");
        }
        /// <summary>
        /// A dummy method to simulate reading temperature from a connected
        /// sensor to this machine
        /// </summary>
        /// <returns>int</returns>
        private int GetRoomTemperature()
        {
            int temp = (DateTime.Now.Second < 15) ? 25 : DateTime.Now.Second;
            return temp;
        }

    }
}
