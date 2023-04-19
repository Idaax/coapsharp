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
    public class BasicCoAPBlockTransferPUTClient : ICoAPSample
    {
        /// <summary>
        /// What is our block size in terms of bytes
        /// </summary>
        private const int BLOCK_SIZE_BYTES = 16;
        /// <summary>
        /// We want to transfer 35 bytes, in 16 byte blocks..3 transfers
        /// </summary>
        private byte[] _dataToTransfer = new byte[35];
        /// <summary>
        /// Holds the sequence number of the block
        /// </summary>
        private UInt32 _blockSeqNo = 0;
        /// <summary>
        /// Holds the client channel instance
        /// </summary>
        private CoAPClientChannel _client = null;
        /// <summary>
        /// Holds how many bytes got transferred to server
        /// </summary>
        private int _totalBytesTransferred = 0;
        /// <summary>
        /// Entry point
        /// </summary>
        public void Run()
        {
            BasicCoAPBlockTransferPUTClient p = new BasicCoAPBlockTransferPUTClient();
            p.SetupClient();
            p.TransferToServerInBlocks(); //Initiate the first transfer
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
        /// Check if we have data to send
        /// </summary>
        /// <returns>bool</returns>
        public bool HasDataToSend()
        {
            //In this example, we are only transferring on large data set...
            return (this._totalBytesTransferred < this._dataToTransfer.Length);
        }
        /// <summary>
        /// Transfer data to server in blocks
        /// </summary>
        /// <returns>true if there are more blocks to be transferred</returns>
        public void TransferToServerInBlocks()
        {
            CoAPRequest blockPUTReq = new CoAPRequest(CoAPMessageType.CON, CoAPMessageCode.PUT, this._client.GetNextMessageID());
            blockPUTReq.SetURL("coap://127.0.0.1:5683/largedata/blockput");
            blockPUTReq.AddTokenValue(DateTime.Now.ToString("HHmm"));
            //Get needed bytes from source            
            int copyBeginIdx = (int)(this._blockSeqNo * BLOCK_SIZE_BYTES);
            int bytesToCopy = ((copyBeginIdx + BLOCK_SIZE_BYTES) < this._dataToTransfer.Length) ? BLOCK_SIZE_BYTES : (this._dataToTransfer.Length - copyBeginIdx);
            byte[] blockToSend = new byte[bytesToCopy];
            Array.Copy(this._dataToTransfer, copyBeginIdx, blockToSend, 0, bytesToCopy);
            //Calculate how many more bytes left to transfer
            bool hasMore = (this._totalBytesTransferred + bytesToCopy < this._dataToTransfer.Length);
            //Add the bytes to the payload
            blockPUTReq.Payload = new CoAPPayload(blockToSend);
            //Now add block option to the request
            blockPUTReq.SetBlockOption(CoAPHeaderOption.BLOCK1, new CoAPBlockOption(this._blockSeqNo, hasMore, CoAPBlockOption.BLOCK_SIZE_16));
            //send
            this._client.Send(blockPUTReq);
            //Updated bytes transferred
            this._totalBytesTransferred += bytesToCopy;
        }
        /// <summary>
        /// Once a response is received, check if that has block option set.
        /// If yes, server has responded back. Ensure you check the more flag.
        /// If that falg was set in the response, that means, server is still
        /// getting the data that you sent in the previous PUT request. So wait
        /// for a final ACK
        /// </summary>
        /// <param name="coapResp">CoAPResponse</param>
        void OnCoAPResponseReceived(CoAPResponse coapResp)
        {
            Console.WriteLine($"{this.GetType().Name} :: Response Received {coapResp}");

            if (coapResp.MessageType.Value == CoAPMessageType.ACK)
            {
                CoAPBlockOption returnedBlockOption = coapResp.GetBlockOption(CoAPHeaderOption.BLOCK1);
                if (returnedBlockOption != null && !returnedBlockOption.HasMoreBlocks)
                {
                    //send the next block
                    this._blockSeqNo++;
                    if (this.HasDataToSend()) this.TransferToServerInBlocks();
                }
            }
        }
        /// <summary>
        /// Not used in this sample
        /// </summary>
        /// <param name="coapReq"></param>
        void OnCoAPRequestReceived(CoAPRequest coapReq)
        {
            Console.WriteLine($"{this.GetType().Name} :: Request received {coapReq}");
        }
        /// <summary>
        /// Not used in this sample
        /// </summary>
        /// <param name="e"></param>
        /// <param name="associatedMsg"></param>
        void OnCoAPError(Exception e, AbstractCoAPMessage associatedMsg)
        {
            Console.WriteLine($"{this.GetType().Name} :: Error {e} , associated message {associatedMsg}");
        }
    }
}
