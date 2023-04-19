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
    public class BasicCoAPNONClient : ICoAPSample
    {
        /// <summary>
        /// Holds the instance of the CoAP client channel object
        /// </summary>
        private static CoAPClientChannel _coapClient = null;
        /// <summary>
        /// Used for matching request / response / associated request
        /// </summary>
        private static string _mToken = "";

        /// <summary>
        /// The entry point method
        /// </summary>
        public void Run()
        {
            string serverIP = "127.0.0.1";
            int serverPort = 5683;

            BasicCoAPNONClient client = new BasicCoAPNONClient();

            _coapClient = new CoAPClientChannel();
            _coapClient.Initialize(serverIP, serverPort);
            _coapClient.CoAPResponseReceived += new CoAPResponseReceivedHandler(client.OnCoAPResponseReceived);
            _coapClient.CoAPRequestReceived += new CoAPRequestReceivedHandler(client.OnCoAPRequestReceived);
            _coapClient.CoAPError += new CoAPErrorHandler(client.OnCoAPError);
            //Send a NON request to get the temperature...in return we will get a NON request from the server
            CoAPRequest coapReq = new CoAPRequest(CoAPMessageType.NON,
                                                CoAPMessageCode.GET,
                                                100);//hardcoded message ID as we are using only once
            string uriToCall = "coap://" + serverIP + ":" + serverPort + "/sensors/temp";
            coapReq.SetURL(uriToCall);
            _mToken = DateTime.Now.ToString("HHmmss");//Token value must be less than 8 bytes
            coapReq.Token = new CoAPToken(_mToken);//A random token
            _coapClient.Send(coapReq);
            Thread.Sleep(Timeout.Infinite);//blocks
        }
        /// <summary>
        /// Called when error occurs
        /// </summary>
        /// <param name="e">The exception that occurred</param>
        /// <param name="associatedMsg">The associated message (if any)</param>    
        void OnCoAPError(Exception e, AbstractCoAPMessage associatedMsg)
        {
            Console.WriteLine($"{this.GetType().Name} :: Error {e} , associated message {associatedMsg}");
        }

        /// <summary>
        /// Called when a request is received...
        /// </summary>
        /// <param name="coapReq">The CoAPRequest object</param>
        void OnCoAPRequestReceived(CoAPRequest coapReq)
        {
            Console.WriteLine($"{this.GetType().Name} :: Request received {coapReq}");
        }

        /// <summary>
        /// Called when a response is received against a sent request
        /// </summary>
        /// <param name="coapResp">The CoAPResponse object</param>
        void OnCoAPResponseReceived(CoAPResponse coapResp)
        {
            Console.WriteLine($"{this.GetType().Name} :: Response Received {coapResp}");
            string tokenRx = (coapResp.Token != null && coapResp.Token.Value != null) ? AbstractByteUtils.ByteToStringUTF8(coapResp.Token.Value) : "";
            if (tokenRx == _mToken)
            {
                //This response is against the NON request for getting temperature we issued earlier
                if (coapResp.Code.Value == CoAPMessageCode.CONTENT)
                {
                    //Get the temperature
                    string tempAsJSON = AbstractByteUtils.ByteToStringUTF8(coapResp.Payload.Value);
                    Hashtable tempValues = JSONResult.FromJSON(tempAsJSON);
                    int temp = Convert.ToInt32(tempValues["temp"].ToString());
                    //Now do something with this temperature received from the server
                }
                else
                {
                    //Will come here if an error occurred..
                }
            }
        }
    }
}
