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
    public class BasicCoAPSeparateResponseClient : ICoAPSample
    {
        /// <summary>
        /// Holds an instance of the client channel
        /// </summary>
        private CoAPClientChannel _client = null;
        /// <summary>
        /// Entry point
        /// </summary>
        public void Run()
        {
            BasicCoAPSeparateResponseClient p = new BasicCoAPSeparateResponseClient();
            p.SetupClient();
            p.SendRequest();
            Thread.Sleep(Timeout.Infinite);
        }
        /// <summary>
        /// Setup the client
        /// </summary>
        public void SetupClient()
        {
            this._client = new CoAPClientChannel();
            this._client.Initialize("127.0.0.1", 5683);
            this._client.CoAPError += new CoAPErrorHandler(OnCoAPError);
            this._client.CoAPRequestReceived += new CoAPRequestReceivedHandler(OnCoAPRequestReceived);
            this._client.CoAPResponseReceived += new CoAPResponseReceivedHandler(OnCoAPResponseReceived);
        }
        /// <summary>
        /// Send the request to get the temperature
        /// </summary>
        public void SendRequest()
        {
            string urlToCall = "coap://127.0.0.1:5683/sensors/temp/separate";
            UInt16 mId = this._client.GetNextMessageID();//Using this method to get the next message id takes care of pending CON requests
            CoAPRequest tempReq = new CoAPRequest(CoAPMessageType.CON, CoAPMessageCode.GET, mId);
            tempReq.SetURL(urlToCall);
            tempReq.AddTokenValue("123");
            /*Uncomment the two lines below to use non-default values for timeout and retransmission count*/
            /*Dafault value for timeout is 2 secs and retransmission count is 4*/
            //tempReq.Timeout = 10;
            //tempReq.RetransmissionCount = 5;

            this._client.Send(tempReq);
        }
        /// <summary>
        /// We should receive the temperature from sever in the response
        /// </summary>
        /// <param name="coapResp">CoAPResponse</param>
        void OnCoAPResponseReceived(CoAPResponse coapResp)
        {
            Console.WriteLine($"{this.GetType().Name} :: Response Received {coapResp}");

            if (coapResp.MessageType.Value == CoAPMessageType.ACK && coapResp.Code.Value == CoAPMessageCode.EMPTY)
            {
                //Server wants some time to process our request...wait
            }
        }
        /// <summary>
        /// Server sends the separate response as a CON request
        /// </summary>
        /// <param name="coapReq"></param>
        void OnCoAPRequestReceived(CoAPRequest coapReq)
        {
            Console.WriteLine($"{this.GetType().Name} :: Request received {coapReq}");

            //For a separate response, we get a CON request back...
            if (coapReq.MessageType.Value == CoAPMessageType.CON &&
                coapReq.Code.Value == CoAPMessageCode.POST)
            {
                //Do token matching here...TODO::
                Debug.Print(coapReq.ToString());
                //Send the acknowledgement that we received the separate response
                //We are sending the "CREATED" status code to indicate to server
                //we did something with the response
                CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK,
                                                    CoAPMessageCode.CREATED,
                                                    coapReq);
                this._client.Send(resp);
            }
        }
        /// <summary>
        /// Handle error
        /// </summary>
        /// <param name="e">The exception that occurred</param>
        /// <param name="associatedMsg">The associated message (if any)</param>
        void OnCoAPError(Exception e, AbstractCoAPMessage associatedMsg)
        {
            Console.WriteLine($"{this.GetType().Name} :: Error {e} , associated message {associatedMsg}");
        }
    }
}
