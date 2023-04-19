/******************************************************************************
    CoAPSharp - C# Implementation of CoAP for .NET
    This library was originally written for .NET Micro framework. It is now
    migrated to nanoFramework.
    
    MIT License

    Copyright (c) [2023] [Idaax Inc., www.coapsharp.com]

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
 *****************************************************************************/
using System;
using System.Text;
using Idaax.CoAP.Helpers;

namespace Idaax.CoAP.Message
{
    /// <summary>
    /// Represents a standard response object in CoAP exchange
    /// </summary>
    public class CoAPResponse : AbstractCoAPMessage
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public CoAPResponse() { this.Version = new CoAPVersion(); this.Options = new CoAPHeaderOptions(); }
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="mType">The message type</param>
        /// <param name="mCode">The message code</param>
        /// <param name="id">The message Id</param>
        public CoAPResponse(byte mType, byte mCode, UInt16 id)
        {
            if (mType == CoAPMessageType.CON)
                throw new ArgumentException("A response message must be of type ACK or RST or NON");

            if (!CoAPMessageCode.DoesMessageCodeRepresentAResponse(mCode))
                throw new ArgumentException("The message code can only be used for a request");

            this.Version = new CoAPVersion();

            this.MessageType = new CoAPMessageType();
            this.MessageType.Value = mType;

            this.Code = new CoAPMessageCode();
            if (!this.Code.IsValid(mCode))
                throw new ArgumentException("Invalid message code in request");
            this.Code.Value = mCode;
            this.ID = new CoAPMessageID(id);
            this.Options = new CoAPHeaderOptions();
        }
        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="mType">The message type</param>
        /// <param name="mCode">The message code</param>
        /// <param name="coapReq">The CoAPRequest against which this response is being constructed</param>
        public CoAPResponse(byte mType, byte mCode, CoAPRequest coapReq)
        {
            if (mType == CoAPMessageType.CON)
                throw new ArgumentException("A response message must be of type ACK or RST or NON");

            if (!CoAPMessageCode.DoesMessageCodeRepresentAResponse(mCode))
                throw new ArgumentException("The message code can only be used for a request");

            this.Version = new CoAPVersion();

            this.MessageType = new CoAPMessageType();
            this.MessageType.Value = mType;

            this.Code = new CoAPMessageCode();
            if (!this.Code.IsValid(mCode) )
                throw new ArgumentException("Response code invalid");
            this.Code.Value = mCode;
            this.ID = new CoAPMessageID(coapReq.ID.Value);
            this.Token = new CoAPToken(coapReq.Token.Value);
            //TOCHECK::this.Options = new CoAPHeaderOptions(coapReq.Options);
            this.Options = new CoAPHeaderOptions();
            //Other needed parameters
            this.RemoteSender = coapReq.RemoteSender;
        }
        #endregion

        #region Operations
        /// <summary>
        /// Set the location URL. This is set by the response to indicate "Created" result if the
        /// request is POST (to create a new resource)
        /// </summary>
        /// <param name="locationURL">The location URL relative to the URI that got created</param>
        public void SetLocation(string locationURL)
        {
            if (locationURL == null || locationURL.Trim().Length == 0) throw new ArgumentException("Invalid CoAP location URL");
            locationURL = locationURL.Trim().ToLower();

            if (locationURL.IndexOf("#") >= 0) throw new ArgumentException("Fragments not allowed in CoAP location URL");
            //Add these items as option
                        
            //Path components
            string[] segments = AbstractURIUtils.GetUriSegments(locationURL);

            if (segments != null && segments.Length > 0)
            {
                foreach (string segment in segments)
                {
                    if (segment.Trim().Length == 0) continue;
                    this.Options.AddOption(CoAPHeaderOption.LOCATION_PATH, AbstractByteUtils.StringToByteUTF8(AbstractURIUtils.UrlDecode(segment)));
                }
            }
            //Query
            string[] qParams = AbstractURIUtils.GetQueryParameters(locationURL);
            if (qParams != null && qParams.Length > 0)
            {
                foreach (string queryComponent in qParams)
                {
                    if (queryComponent.Trim().Length == 0) continue;
                    this.Options.AddOption(CoAPHeaderOption.LOCATION_QUERY, AbstractByteUtils.StringToByteUTF8(AbstractURIUtils.UrlDecode(queryComponent)));
                }
            }
        }
        #endregion
    }
}
