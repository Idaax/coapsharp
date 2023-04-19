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
    public class BasicCoAPObserverClient: ICoAPSample
    {
        /// <summary>
        /// Holds an instance of the CoAP client
        /// </summary>
        private CoAPClientChannel _client = null;
        /// <summary>
        /// Will keep a count of how many notifications we received so far
        /// </summary>
        private int countOfNotifications = 0;
        /// <summary>
        /// The last observed sequence number
        /// </summary>
        private int lastObsSeq = 0;
        /// <summary>
        /// Last time the observable notification was received.
        /// </summary>
        private DateTime lastObsRx = DateTime.MinValue;
        /// <summary>
        /// Entry point
        /// </summary>
        public void Run()
        {
            BasicCoAPObserverClient p = new BasicCoAPObserverClient();
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
            string urlToCall = "coap://127.0.0.1:5683/sensors/temp/observe";
            UInt16 mId = this._client.GetNextMessageID();//Using this method to get the next message id takes care of pending CON requests
            CoAPRequest tempReq = new CoAPRequest(CoAPMessageType.CON, CoAPMessageCode.GET, mId);
            tempReq.SetURL(urlToCall);
            //Important::Add the Observe option
            tempReq.AddOption(CoAPHeaderOption.OBSERVE, null);//Value of observe option has no meaning in request
            tempReq.AddTokenValue(DateTime.Now.ToString("HHmmss"));//must be <= 8 bytes
            /*Uncomment the two lines below to use non-default values for timeout and retransmission count*/
            /*Dafault value for timeout is 2 secs and retransmission count is 4*/
            //tempReq.Timeout = 10;
            //tempReq.RetransmissionCount = 5;

            this._client.Send(tempReq);
        }
        /// <summary>
        /// We should receive an ACK to indicate we successfully registered with
        /// the server to observe the resource
        /// </summary>
        /// <param name="coapResp">CoAPResponse</param>
        void OnCoAPResponseReceived(CoAPResponse coapResp)
        {
            Console.WriteLine($"{this.GetType().Name} :: Response Received {coapResp}");

            if (coapResp.MessageType.Value == CoAPMessageType.ACK &&
                coapResp.Code.Value == CoAPMessageCode.EMPTY)
                Console.WriteLine("Registered successfully to observe temperature");
            else
                Console.WriteLine("Failed to register for temperature observation");
        }
        /// <summary>
        /// Going forward, we will receive temperature notifications from 
        /// server in a CON request
        /// </summary>
        /// <param name="coapReq">CoAPRequest</param>
        void OnCoAPRequestReceived(CoAPRequest coapReq)
        {
            Console.WriteLine($"{this.GetType().Name} :: Request received {coapReq}");

            if (coapReq.MessageType.Value == CoAPMessageType.CON)
            {
                //Extract the temperature..but first, check if the notification is fresh
                //The server sends a 4-digit sequence number
                int newObsSeq = AbstractByteUtils.ToUInt16(coapReq.Options.GetOption(CoAPHeaderOption.OBSERVE).Value);
                if ((lastObsSeq < newObsSeq && ((newObsSeq - lastObsSeq) < (System.Math.Pow(2, 23)))) ||
                    (lastObsSeq > newObsSeq && ((lastObsSeq - newObsSeq) > (System.Math.Pow(2, 23)))) ||
                    DateTime.Now > lastObsRx.AddSeconds(128))
                {
                    //The value received from server is new....read the new temperature
                    //We got the temperature..it will be in payload in JSON
                    string payload = AbstractByteUtils.ByteToStringUTF8(coapReq.Payload.Value);
                    Hashtable keyVal = JSONResult.FromJSON(payload);
                    int temp = Convert.ToInt32(keyVal["temp"].ToString());
                    //do something with the temperature now  
                }
                //update how many notifications received
                this.countOfNotifications++;
                if (this.countOfNotifications > 5)
                {
                    //We are no longer interested...send RST to de-register
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.RST, CoAPMessageCode.EMPTY, coapReq.ID.Value);
                    resp.RemoteSender = coapReq.RemoteSender;
                    resp.Token = coapReq.Token;//Do not forget this...this is how messages are correlated                    
                    this._client.Send(resp);
                }
                else
                {
                    //we are still interested...send ACK
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK, CoAPMessageCode.EMPTY, coapReq.ID.Value);
                    resp.RemoteSender = coapReq.RemoteSender;
                    resp.Token = coapReq.Token;//Do not forget this...this is how messages are correlated       
                    this._client.Send(resp);
                }
                lastObsSeq = newObsSeq;
                lastObsRx = DateTime.Now;
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
