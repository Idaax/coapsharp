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
    public class BasicCoAPSeparateResponseServer : ICoAPSample
    {
        /// <summary>
        /// Holds the instance of the server channel
        /// </summary>
        private CoAPServerChannel _server = null;
        /// <summary>
        /// Entry point
        /// </summary>    
        public void Run()
        {
            BasicCoAPSeparateResponseServer p = new BasicCoAPSeparateResponseServer();
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
        /// Called when CON/NON message is received
        /// </summary>
        /// <param name="coapReq">CoAPRequest</param>
        void OnCoAPRequestReceived(CoAPRequest coapReq)
        {
            Console.WriteLine($"{this.GetType().Name} :: Request received {coapReq}");

            string reqPath = (coapReq.GetPath() != null) ? coapReq.GetPath().ToLower() : "";

            //Separate response is only for CON messages
            if (coapReq.MessageType.Value == CoAPMessageType.CON)
            {
                //In this example, we only support GET for this separate response
                if (coapReq.Code.Value != CoAPMessageCode.GET)
                {
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK,
                                                        CoAPMessageCode.METHOD_NOT_ALLOWED,
                                                        coapReq /*Copy all necessary values from request in the response*/);
                    //When you use the constructor that accepts a request, then automatically
                    //the message id , token and remote sender values are copied over to the response
                    this._server.Send(resp);
                }
                else if (reqPath != "sensors/temp/separate")
                {
                    //We do not understand this..
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK,
                                                            CoAPMessageCode.NOT_FOUND,
                                                            coapReq /*Copy all necessary values from request in the response*/);
                    this._server.Send(resp);
                }
                else
                {
                    //Everything is good...send an empty ACK message to the client
                    //indicating, "please wait..we are processing your request and will get back to you soon"
                    CoAPResponse resp = new CoAPResponse(CoAPMessageType.ACK,
                                                        CoAPMessageCode.EMPTY,
                                                        coapReq.ID.Value /*Copy over request message Id*/);
                    //Add the remote sender details..this is used internally by CoAPSharp
                    resp.RemoteSender = coapReq.RemoteSender;
                    //send it
                    this._server.Send(resp);
                    //Do some heavy work before responding
                    //this._server.AddToPendingSeparateResponse(coapReq);
                    Thread heavyWork = new Thread(() => DoTimeTakingTask(coapReq));
                    heavyWork.Start();
                }
            }
        }
        /// <summary>
        /// Called when ACK/RST is received
        /// </summary>
        /// <param name="coapResp">CoAPResponse</param>
        void OnCoAPResponseReceived(CoAPResponse coapResp)
        {
            /*
             * The CON message that we sent earlier as a separate response will send
             * back an ACK that you can handle here for further processing
             */
            Console.WriteLine($"{this.GetType().Name} :: Response Received {coapResp}");
        }
        /// <summary>
        /// Called when an error occurs
        /// </summary>
        /// <param name="e">The exception object depicting the error</param>
        /// <param name="associatedMsg">The associated message (if any) whose processing resulted in this error</param>
        void OnCoAPError(Exception e, AbstractCoAPMessage associatedMsg)
        {
            Console.WriteLine($"{this.GetType().Name} :: Error {e} , associated message {associatedMsg}");
        }
        /// <summary>
        /// Simulation of a time taking task.
        /// Once this task is complete, we will send the separate response back
        /// </summary>
        /// <param name="sepReq">The request for separate response</param>
        private void DoTimeTakingTask(CoAPRequest sepReq)
        {
            Thread.Sleep(5000);//simulate a time taking task
            //Handling only one request in this sample
            //In real-life, you may want to use a queue for all pending responses
            if (sepReq != null)
            {
                //Send the response back as another CON request
                CoAPRequest req = new CoAPRequest(CoAPMessageType.CON, CoAPMessageCode.POST, sepReq.ID.Value);
                //Copy over needed values
                req.RemoteSender = sepReq.RemoteSender;
                req.Token = sepReq.Token;
                //Add some value that is the result of this heavy work
                req.AddOption(CoAPHeaderOption.CONTENT_FORMAT, AbstractByteUtils.GetBytes(CoAPContentFormatOption.TEXT_PLAIN));
                req.AddPayload("Result of heavy work");
                this._server.Send(req);
            }
        }
    }
}
