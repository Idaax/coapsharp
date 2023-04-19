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
    public class BasicCoAPNONServer : ICoAPSample
    {
        /// <summary>
        /// Holds the instance of the server channel class
        /// </summary>
        private CoAPServerChannel _coapServer = null;
        public void Run()
        {
            BasicCoAPNONServer server = new BasicCoAPNONServer();
            server.StartServer();
            Thread.Sleep(Timeout.Infinite);//block here
        }
        /// <summary>
        /// Start the server
        /// </summary>
        public void StartServer()
        {
            this._coapServer = new CoAPServerChannel();
            this._coapServer.Initialize(null, 5683);//Initialize and listen on default CoAP port
            //Setup event listeners
            this._coapServer.CoAPRequestReceived += new CoAPRequestReceivedHandler(OnCoAPRequestReceived);
            this._coapServer.CoAPResponseReceived += new CoAPResponseReceivedHandler(OnCoAPResponseReceived);
            this._coapServer.CoAPError += new CoAPErrorHandler(OnCoAPError);
        }
        /// <summary>
        /// Called when error occurs
        /// </summary>
        /// <param name="e">The exception that occurred</param>
        /// <param name="associatedMsg">The associated message (if any)</param>    
        public void OnCoAPError(Exception e, AbstractCoAPMessage associatedMsg)
        {
            Console.WriteLine($"{this.GetType().Name} :: Error {e} , associated message {associatedMsg}");
        }
        /// <summary>
        /// Called when a response is received against a sent request
        /// </summary>
        /// <param name="coapResp">The CoAPResponse object</param>
        void OnCoAPResponseReceived(CoAPResponse coapResp)
        {
            //This sample will not receive any response
        }
        /// <summary>
        /// Called when a request is received
        /// </summary>
        /// <param name="coapReq">The CoAPRequest object</param>
        void OnCoAPRequestReceived(CoAPRequest coapReq)
        {
            Console.WriteLine($"{this.GetType().Name} :: Request received {coapReq}");
            //This sample only works on NON requests of type GET
            //This sample simualtes a temperature sensor at the path "sensors/temp"

            string reqURIPath = (coapReq.GetPath() != null) ? coapReq.GetPath().ToLower() : "";
            /**
                * Draft 18 of the specification, section 5.2.3 states, that if against a NON message,
                * a response is required, then it must be sent as a NON message
                */
            if (coapReq.MessageType.Value != CoAPMessageType.NON)
            {
                //only NON  combination supported..we do not understand this send a RST back
                CoAPResponse msgTypeNotSupported = new CoAPResponse(CoAPMessageType.RST, /*Message type*/
                                                                    CoAPMessageCode.NOT_IMPLEMENTED, /*Not implemented*/
                                                                    coapReq.ID.Value /*copy message Id*/);
                msgTypeNotSupported.Token = coapReq.Token; //Always match the request/response token
                msgTypeNotSupported.RemoteSender = coapReq.RemoteSender;
                //send response to client
                this._coapServer.Send(msgTypeNotSupported);
            }
            else if (coapReq.Code.Value != CoAPMessageCode.GET)
            {
                //only GET method supported..we do not understand this send a RST back
                CoAPResponse unsupportedCType = new CoAPResponse(CoAPMessageType.RST, /*Message type*/
                                                    CoAPMessageCode.METHOD_NOT_ALLOWED, /*Method not allowed*/
                                                    coapReq.ID.Value /*copy message Id*/);
                unsupportedCType.Token = coapReq.Token; //Always match the request/response token
                unsupportedCType.RemoteSender = coapReq.RemoteSender;
                //send response to client
                this._coapServer.Send(unsupportedCType);
            }
            else if (reqURIPath != "sensors/temp")
            {
                //classic 404 not found..we do not understand this send a RST back 
                CoAPResponse unsupportedPath = new CoAPResponse(CoAPMessageType.RST, /*Message type*/
                                                    CoAPMessageCode.NOT_FOUND, /*Not found*/
                                                    coapReq.ID.Value /*copy message Id*/);
                unsupportedPath.Token = coapReq.Token; //Always match the request/response token
                unsupportedPath.RemoteSender = coapReq.RemoteSender;
                //send response to client
                this._coapServer.Send(unsupportedPath);
            }
            else
            {
                //All is well...send the measured temperature back
                //Again, this is a NON message...we will send this message as a JSON
                //string
                Hashtable valuesForJSON = new Hashtable();
                valuesForJSON.Add("temp", this.GetRoomTemperature());
                string tempAsJSON = JSONResult.ToJSON(valuesForJSON);
                //Now prepare the object
                CoAPResponse measuredTemp = new CoAPResponse(CoAPMessageType.NON, /*Message type*/
                                                    CoAPMessageCode.CONTENT, /*Carries content*/
                                                    coapReq.ID.Value/*copy message Id*/);
                measuredTemp.Token = coapReq.Token; //Always match the request/response token
                //Add the payload
                measuredTemp.Payload = new CoAPPayload(tempAsJSON);
                //Indicate the content-type of the payload
                measuredTemp.AddOption(CoAPHeaderOption.CONTENT_FORMAT,
                                    AbstractByteUtils.GetBytes(CoAPContentFormatOption.APPLICATION_JSON));
                //Add remote sender address details
                measuredTemp.RemoteSender = coapReq.RemoteSender;
                //send response to client
                this._coapServer.Send(measuredTemp);
            }
        }
        /// <summary>
        /// A dummy method to get the room temperature...in real life,
        /// this would be the real work the machine/sensor is required to do
        /// </summary>
        /// <returns>Temp in degree C</returns>
        private int GetRoomTemperature()
        {
            int temp = DateTime.Now.Second;
            if (temp < 15) temp = 25;// just do not want to show that it's too cold!
            return temp;
        }
    }
}
