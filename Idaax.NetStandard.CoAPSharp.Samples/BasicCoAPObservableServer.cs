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
    public class BasicCoAPObservableServer : ICoAPSample
    {
        private const string OBSERVED_RESOURCE_URI = "coap://127.0.0.1:5683/sensors/temp/observe";

        /// <summary>
        /// Holds the server channel instance
        /// </summary>
        private CoAPServerChannel _server = null;

        /// <summary>
        /// Entry point
        /// </summary>
        public void Run()
        {
            BasicCoAPObservableServer p = new BasicCoAPObservableServer();
            p.StartServer();

            int lastMeasuredTemp = 0;
            /*
                * Read the temperature every 10 seconds and notify all listeners
                * if there is a change
                */
            while (true)
            {
                int temp = p.GetRoomTemperature();
                if (temp != lastMeasuredTemp)
                {
                    p.NotifyListeners(temp);
                    lastMeasuredTemp = temp;
                }
                Thread.Sleep(30000);
            }
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
            //Add all the resources that this server allows observing
            this._server.ObserversList.AddObservableResource(OBSERVED_RESOURCE_URI);
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
            /*We have skipped error handling in code below to just focus on observe option*/
            if (coapReq.MessageType.Value == CoAPMessageType.CON && coapReq.Code.Value == CoAPMessageCode.GET)
            {
                if (!coapReq.IsObservable() /*Does the request have "Observe" option*/)
                {
                    /*Request is not to observe...we do not support anything no-observable*/
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK,
                                                            CoAPMessageCode.NOT_IMPLEMENTED,
                                                            coapReq /*Copy all necessary values from request in the response*/);
                    this._server.Send(resp);
                }
                else if (!this._server.ObserversList.IsResourceBeingObserved(coapReq.GetURL()) /*do we support observation on this path*/)
                {
                    //Observation is not supported on this path..just to tell you how to check
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK,
                                                            CoAPMessageCode.NOT_FOUND,
                                                            coapReq /*Copy all necessary values from request in the response*/);
                    this._server.Send(resp);
                }
                else
                {
                    //This is a request to observe this resource...register this client
                    this._server.ObserversList.AddResourceObserver(coapReq);

                    /*Request contains observe option and path is correct*/
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK,
                                                        CoAPMessageCode.EMPTY,
                                                        coapReq /*Copy all necessary values from request in the response*/);

                    //send it..tell client we registered it's request to observe
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

            //If we receive a RST, then remve that client from notifications
            if (coapResp.MessageType.Value == CoAPMessageType.RST)
            {
                this._server.ObserversList.RemoveResourceObserver(coapResp);
            }
        }
        /// <summary>
        /// Notify listeners of the new temperature
        /// </summary>
        /// <param name="temp">The temperature</param>
        public void NotifyListeners(int temp)
        {
            ArrayList resObservers = this._server.ObserversList.GetResourceObservers(OBSERVED_RESOURCE_URI);
            if (resObservers == null || resObservers.Count == 0) return;

            //The next observe sequence number
            UInt16 obsSeq = (UInt16)Convert.ToInt16(DateTime.Now.ToString("mmss"));//Will get accomodated in 24-bits limit and will give good sequence

            foreach (CoAPRequest obsReq in resObservers)
            {
                UInt16 mId = this._server.GetNextMessageID();
                CoAPRequest notifyReq = new CoAPRequest(CoAPMessageType.CON, CoAPMessageCode.PUT, mId);
                notifyReq.RemoteSender = obsReq.RemoteSender;
                notifyReq.Token = obsReq.Token;
                //Add observe option with sequence number
                notifyReq.AddOption(CoAPHeaderOption.OBSERVE, AbstractByteUtils.GetBytes(obsSeq));

                //The payload will be JSON
                Hashtable ht = new Hashtable();
                ht.Add("temp", temp);
                string jsonStr = JSONResult.ToJSON(ht);
                notifyReq.AddPayload(jsonStr);
                //Tell recipient about the content-type of the response
                notifyReq.AddOption(CoAPHeaderOption.CONTENT_FORMAT, AbstractByteUtils.GetBytes(CoAPContentFormatOption.APPLICATION_JSON));
                //send it
                this._server.Send(notifyReq);
            }
        }
        /// <summary>
        /// A dummy method to simulate reading temperature from a connected
        /// sensor to this machine
        /// </summary>
        /// <returns>int</returns>
        public int GetRoomTemperature()
        {
            int temp = (DateTime.Now.Second < 15) ? 25 : DateTime.Now.Second;
            return temp;
        }
    }
}
