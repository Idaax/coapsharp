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
    public class BasicCoAPCONClient : ICoAPSample
    {
        /// <summary>
        /// Holds an instance of the CoAP client
        /// </summary>
        private CoAPClientChannel _client = null;
        /// <summary>
        /// Entry point
        /// </summary>
        public void Run()
        {
            BasicCoAPCONClient p = new BasicCoAPCONClient();
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
            string urlToCall = "coap://127.0.0.1:5683/sensors/temp";
            UInt16 mId = this._client.GetNextMessageID();//Using this method to get the next message id takes care of pending CON requests
            CoAPRequest tempReq = new CoAPRequest(CoAPMessageType.CON, CoAPMessageCode.GET, mId);
            tempReq.SetURL(urlToCall);

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

            if (coapResp.MessageType.Value == CoAPMessageType.ACK &&
                coapResp.Code.Value == CoAPMessageCode.CONTENT)
            {
                //We got the temperature..it will be in payload in JSON
                string payload = AbstractByteUtils.ByteToStringUTF8(coapResp.Payload.Value);
                Hashtable keyVal = JSONResult.FromJSON(payload);
                int temp = Convert.ToInt32(keyVal["temp"].ToString());
                //do something with the temperature now
            }
        }
        /// <summary>
        /// Not doing anything now
        /// </summary>
        /// <param name="coapReq"></param>
        void OnCoAPRequestReceived(CoAPRequest coapReq)
        {
            Console.WriteLine($"{this.GetType().Name} :: Request received {coapReq}");
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
